using Avalonia.Controls;
using StarExplorer.Shared;
using StarExplorer.Views;
using System;
using System.Runtime.InteropServices;

namespace StarExplorer.Logic
{
    internal class MainWindowControler : IMainWindowControler
    {
        //此类是主窗口的控制器，构造此类代表创建一个主窗口

        //依赖项
        ISettings settings;
        ICoreData coreData;

        //管理的窗口
        Explorer? explorer;
        ExplorerData? explorerData;

        //事件
        public event Action? AppExitRequested;

        public MainWindowControler(ISettings settings, ICoreData coreData)
        {
            this.settings = settings;
            this.coreData = coreData;
        }

        private void CreateWindow()
        {
            //依据Settings创建DataContext
            explorerData = new ExplorerData(settings, coreData);
            explorerData.displayMode = DisplayMode.Devices;//默认显示设备列表
            SetLayoutMode();

            explorer = new Explorer(explorerData);
            explorer.DataContext = explorerData;
        }

        public void Initialize()
        {
            CreateWindow();
            BindEvents();
        }
        
        //封装方法
        public Window GetWindow()
        {
            if (explorer == null) 
                throw new InvalidOperationException("主窗口尚未创建");
            return explorer;
        }

        private void OnAppExitRequested()
        {
            AppExitRequested?.Invoke();
        }

        private void BindEvents()
        {
            if (explorer == null || explorerData == null) 
                throw new InvalidOperationException("主窗口或数据上下文尚未创建，无法绑定事件");
            explorerData.AppExitRequested += OnAppExitRequested;
        }

        public void SetLayoutMode()
        {
            if (explorerData == null)
                throw new InvalidOperationException("数据上下文尚未创建，无法设置布局模式");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                explorerData.Layout = ExplorerLayout.Desktop;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                explorerData.Layout = ExplorerLayout.Desktop;
            }
            else
            {
                explorerData.Layout = ExplorerLayout.Mobile;
            }
        }
    }

    public interface IMainWindowControler : IModule
    {
        public Window GetWindow();
        public event Action? AppExitRequested;
    }
}
