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
using System.Collections.Generic;
using Tmds.DBus.Protocol;

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
        CancellationTokenSource tokenSource = null!; //需要时再创建
        object tokenSourceLock = new object(); //锁对象 用于保护tokenSource的访问
        //本地委托
        Action<Exception,string>? fileExceptionHandler;
        Action<Exception, string>? imageLoadExceptionHandler;
        Action<string>? itemsIconLoadHandler;

        public ItemsPanelController(ISettings settings, ICoreData coreData, string path)
        {
            this.settings = settings;
            this.coreData = coreData;
            
            CreateWindow();
            InitHandler();  //注意顺序!
            BindEvents();

            //设置初始路径
            this.SetPathAndRefresh(path);
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
        //此方法仅限内部使用
        public void SetPathAndRefresh(string path)
        {
            Debug.WriteLine($"Setting path to: {path}");
            //修改位置后 需要取消图标加载任务
            lock (tokenSourceLock)
            {
                this.tokenSource?.Cancel();
            }

            Task.Run(async () =>
            {
                bool success = false;
                try
                {
                    success = await SetPathAndRefreshAsync(path);
                }
                catch (Exception ex)
                {
                    fileExceptionHandler?.Invoke(ex, path);
                    success = false;
                }
                if (success)
                {
                    //通知外部路径已改变
                    this.PathChanged?.Invoke(path);
                }
                else
                {
                    //此处失败还没有替换panelData的Items
                    return;
                }
            });
        }

        public async Task<bool> SetPathAndRefreshAsync(string path)
        {
            Debug.WriteLine($"Setting path to: {path}");
            //修改位置后 需要取消图标加载任务
            lock (tokenSourceLock)
            {
                this.tokenSource?.Cancel();
            }

            bool result = await Task.Run(() =>
            {
                List<Item> newItems;
                try
                {
                    newItems = coreData.GetFoldersAndFilesByPath(path);
                }
                catch (Exception ex)
                {
                    fileExceptionHandler?.Invoke(ex, path);
                    return false;  //方便外层调用者得知执行失败
                }
                panelData.UpdateItems(newItems);
                this.itemsIconLoadHandler?.Invoke(path);
                return true;
            });
            return result;
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
