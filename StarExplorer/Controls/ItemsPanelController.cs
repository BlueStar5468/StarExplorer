using StarExplorer.Logic;
using Avalonia.Controls;
using System.Collections.Generic;
using StarExplorer.Shared;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

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
        string? path;
        //本地委托
        Action<Exception,string>? fileExceptionHandler;

        public ItemsPanelController(ISettings settings, ICoreData coreData)
        {
            this.settings = settings;
            this.coreData = coreData;
            
            CreateWindow();
            BindEvents();
            InitHandler();
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
        }

        //异步刷新方法
        //此方法可作为fire-and-forget在本项目中的标准示例使用
        public void SetPathAndRefresh(string path)
        {
            Debug.WriteLine($"Setting path to: {path}");
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
                    this.path = path;
                    //注意 此方法构造的List并不是Observabal的 通知UI时已经套了线程调用 可以安全的在后台运行
                    panelData.UpdateItems(task.Result.Item2);
                    this.PathChanged?.Invoke(path);
                }
                else
                {
                    //失败时不更新path，保持原来的path不变
                    this.PathChanged?.Invoke(this.path!);
                }
            });
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

            //清理事件
            this.PathChanged = null;
            this.MessageGenerated = null;
        }
    }
}
