using StarExplorer.Logic;
using Avalonia.Controls;
using StarExplorer.Shared;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Avalonia.Media;
using System.Collections.ObjectModel;

namespace StarExplorer.Controls
{
    internal class ItemsPanelController : IDisposable
    {
        //依赖
        ISettings settings;
        ICoreData coreData;
        //管理的ItemsPanel
        ItemsPanelData panelData = null!;
        ItemsPanel panel = null!;
        //事件
        public Action<string>? PathChanged;
        public Action<string, string>? MessageGenerated;
        //数据
        string? fallBackPath;
        CancellationTokenSource tokenSource = null!; //需要时再创建
        object tokenSourceLock = new object(); //锁对象 用于保护tokenSource的访问
        //本地委托
        Action<Exception,string>? fileExceptionHandler;
        Action<Exception, string>? imageLoadExceptionHandler;
        Action<string>? itemsIconLoadHandler;

        public ItemsPanelController(ISettings settings, ICoreData coreData,string path)
        {
            this.settings = settings;
            this.coreData = coreData;
            //初始化path引用
            this.fallBackPath = path;
            
            CreateWindow();
            InitHandler();  //注意顺序!
            BindEvents();
        }

        //封装方法
        private void CreateWindow()
        {
            panelData = new ItemsPanelData(settings);
            panel = new ItemsPanel(panelData);
        }

        private void BindEvents()
        {
            //在此处绑定ItemsPanel相关事件
            //必须确定OpenFolder正常后才能向外部发送PathChanged事件
            panel.PathChanged += SetPathAndRefresh;
        }

        private void InitHandler()
        {
            fileExceptionHandler = (e,path) =>
            {
                //注意注意！！！ 这里的异常处理不在UI线程上 发送消息请先切换到UI线程
                if (e is System.UnauthorizedAccessException)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        MessageGenerated?.Invoke("Error", $"Access denied to the path: {path}. Please check your permissions.");
                    });               
                }
                else if (e is System.IO.DirectoryNotFoundException)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        MessageGenerated?.Invoke("Error", $"The directory was not found: {path}. Please check the path.");
                    });                  
                }
                else
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        MessageGenerated?.Invoke("Error", $"An unexpected error occurred: {e.Message}");
                    });     
                }
            };

            imageLoadExceptionHandler = (e, path) =>
            {
                //注意注意！！！ 这里的异常处理不在UI线程上 发送消息请先切换到UI线程
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    MessageGenerated?.Invoke("Error", $"An error occurred while loading the image: {path}. Exception: {e.Message}");
                });
            };

            itemsIconLoadHandler = (path) =>
            {
                //此处可以处理布局更新后的逻辑
                Debug.WriteLine("ItemsPanel layout updated.Start to Load Icons Asyncly");

                this.LoadFileImageAsync(path);
            };
        }

        //异步刷新方法
        //此方法可作为fire-and-forget在本项目中的标准示例使用
        //待进行的重构:此方法位置不对 应该在SubExplorerController中处理 这里导致了对SubExplorer的path的引用需求
        public void SetPathAndRefresh(string path)
        {
            Debug.WriteLine($"Setting path to: {path}");
            //修改位置后 需要取消图标加载任务
            lock (tokenSourceLock)
            {
                this.tokenSource?.Cancel();
            }

            Task.Run(() =>
            {
                try
                {
                    return (success:true ,coreData.GetFoldersAndFilesByPath(path));
                }
                catch (Exception ex)
                {
                    fileExceptionHandler?.Invoke(ex, path);
                    return (success:false, null!);
                }
            }
            ).ContinueWith((task) =>
            {
                if (task.Result.success)
                {
                    //必须执行成功后才能更新path，否则可能会导致path与显示的内容不一致
                    this.fallBackPath = path;
                    //注意 此方法构造的List并不是Observabal的 通知UI时已经套了线程调用 可以安全的在后台运行
                    panelData.UpdateItems(task.Result.Item2);
                    this.PathChanged?.Invoke(path);
                    //同时开始加载文件图标
                    this.itemsIconLoadHandler?.Invoke(path);
                }
                else
                {
                    //失败时不更新path，保持原来的path不变
                    this.PathChanged?.Invoke(this.fallBackPath!);
                }
            });
        }

        //此方法用于在目录已经打开后再加载文件图标，避免阻塞UI线程
        private void LoadFileImageAsync(string path)
        {
            //此锁保证:
            //如果老任务未结束 则tokenSource不为Null 且可结束
            //如果老任务结束 此tokenSource为Null
            //老任务和新任务不会同时操作tokenSource
            lock (tokenSourceLock)
            {
                //清理资源
                this.tokenSource?.Cancel(); //取消上一次的任务(如果有)
                                            //Dispose由ContinueWith处理 以避免在任务运行时Dispose导致异常
                this.tokenSource = new CancellationTokenSource(); //创建新的CancellationTokenSource
                //以下备份操作时必要的 下次任务开始会直接导致主tokensource丢失 替换成新的
                //这部分备份操作也在锁的保护内
                CancellationTokenSource currentTokenSource;
                CancellationToken currentToken;
                currentTokenSource = tokenSource; //获取当前任务的tokenSource
                currentToken = tokenSource.Token; //获取当前任务的token
                //启动新进程
                Task.Run(() =>
                {
                    //此处保存ItemsDataContexts的引用是必要的 避免在异步任务中ItemsDataContexts被修改导致异                                                                                             //事实上 在旧任务被请求结束后 这个引用就是为了保证不出修改无法枚举异常 同时不会影响新集合
                    ObservableCollection<IItemsDataContext> itemsDataContexts = panelData.ItemsDataContexts; //获取当前ItemsDataContexts的引用
                    try
                    {
                        foreach (var itemDataContext in itemsDataContexts)
                        {
                            //此处只对文件类型的Item进行图标加载，文件夹类型的图标采用默认值
                            //如需动态加载文件夹图标，可在此处添加逻辑
                            if (itemDataContext.ItemToDisplay.Type == ItemType.File && !itemDataContext.IsIconLoaded)
                            {
                                var iconSource = coreData.GetFoldersAndFilesIconByPath(Path.GetFullPath(itemDataContext.ItemToDisplay.Path));
                                //转换byte[]为Avalonia.Media.Imaging.Bitmap
                                IImage bitmap = Utils.CreateImageFormRGBA(iconSource.Pixels, iconSource.Width, iconSource.Height);
                                //更新数据 由UI线程自动处理
                                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                                {
                                    itemDataContext.Icon = bitmap;
                                    itemDataContext.IsIconLoaded = true;
                                });
                            }
                            currentToken.ThrowIfCancellationRequested(); //检查是否取消任务
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException)
                            return; //如果是取消任务的异常 是正常情况
                                    //此处不应该有除了TaskCanceledException之外的异常，如果有，说明逻辑有问题
                                    //以上检查取消不一定能捕获到 如触发枚举修改异常等 这里再检查一次tokenSource是否被取消
                                    //否则输出异常
                        this.imageLoadExceptionHandler?.Invoke(ex, path);
                    }
                    finally
                    {
                        //清理资源
                        if (currentTokenSource != null)
                        {
                            //先把外部置空 标志任务已经结束
                            lock (tokenSourceLock)
                            {   //锁作用见上
                                tokenSource = null!;
                            }
                            //清理资源
                            currentTokenSource.Dispose();
                            currentTokenSource = null!;
                            itemsDataContexts = null!;
                            Debug.WriteLine("File icon loading task completed.");
                        }
                    }
                }, tokenSource.Token);
            }
        }

        public Panel GetInstance()
        {
            return panel.GetInstance();
        }

        public void Dispose()
        {
            //清理子控件
            panel.Dispose();
            //panel的data已经在panel.Dispose()中释放 不要双重释放

            //解除绑定
            panel.PathChanged -= SetPathAndRefresh;

            //清理事件
            this.PathChanged = null;
            this.MessageGenerated = null;
        }
    }
}
