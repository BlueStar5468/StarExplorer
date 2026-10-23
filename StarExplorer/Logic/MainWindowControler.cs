using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Controls;
using StarExplorer.Shared;
using StarExplorer.Views;
using System;
using System.Runtime.InteropServices;

namespace StarExplorer.Logic
{
    internal class MainWindowControler : IMainWindowControler , IModule
    {
        //此类是主窗口的控制器，构造此类代表创建一个主窗口

        //依赖项
        ISettings settings;
        ICoreData coreData;
        ITabManager tabManager;
        //管理的窗口
        Explorer? explorer;
        ExplorerData? explorerData;
        SubExplorerController subExplorerController = null!;
        TabBarPanelData? tabBarPanelData;
        ToastCanvasController toastCanvasController = null!;   

        //事件
        public event Action? AppExitRequested;
        //本地委托
        Action? tabChangedHandler;
        Action? tabAddButtonClickedHandler;
        EventHandler? explorerOpenedHandler;

        public MainWindowControler(ISettings settings, ICoreData coreData, ITabManager tabManager)
        {
            this.settings = settings;
            this.coreData = coreData;
            this.tabManager = tabManager;
        }


        public void Initialize(InitializeDataPack initializeDataPack)
        {
            CreateWindow();
            BindEvents();
        }

        private void CreateWindow()
        {
            //依据Settings创建DataContext
            explorerData = new ExplorerData(settings, coreData, tabManager);
            SetLayoutMode();

            explorer = new Explorer(explorerData);
            explorer.DataContext = explorerData;

            //创建toast通知画布 注:不要在画布创建前调用ShowToast方法,否则会爆空引用
            toastCanvasController = new ToastCanvasController(settings, coreData);
            explorer.MountToSubExplorerMessageBorder(toastCanvasController.GetInstance());
            
            //创建Tab栏以及初始标签页
            CreateTabBar();
            int id;
            var subExplorerCtl = CreateSubExplorer(settings, coreData, (IToastCanvasController)toastCanvasController);
            tabManager.NewTab(subExplorerCtl.GetInstance(),() => subExplorerCtl.Dispose(), out id);
            tabManager.SelectTab(id);
        }

        //内部操作方法
        private void BindEvents()
        {
            if (explorer == null || explorerData == null)
                throw new InvalidOperationException("主窗口或数据上下文尚未创建，无法绑定事件");

            InitHandler();

            //explorerData.AppExitRequested += OnAppExitRequested;
            explorer.AddButtonClicked += tabAddButtonClickedHandler;
            explorer.Opened += explorerOpenedHandler;

            tabManager.SelectedTabChanged += this.tabChangedHandler;
        }

        private void InitHandler()
        {
            this.tabChangedHandler += () =>
            {
                RefreshDisplay();
            };
            this.tabAddButtonClickedHandler += () =>
            {
                if (explorerData == null) throw new Exception("数据上下文尚未创建，无法添加标签页");
                var subExplorerCtl = CreateSubExplorer(settings, coreData, (IToastCanvasController)toastCanvasController);
                tabManager.NewTab(subExplorerCtl.GetInstance(), () => subExplorerCtl.Dispose(), out int _);
            };
            this.explorerOpenedHandler += (s,e) =>
            {
                if (Design.IsDesignMode) return;
                RefreshDisplay();
            };
        }

        private void RefreshDisplay()
        {
            if (explorer == null) throw new Exception("主窗口尚未创建，无法刷新显示");
            explorer.ClearMainDisplay();

            var content = tabManager.GetSelectedTabContent();
            if (content == null) return;
            explorer.MountToMainDisplay(content);
        }

        internal void CreateTabBar()
        {
            explorer?.MountToTabGrid(tabManager.GetTabbarPanel());
        }

        //封装方法
        public Window GetWindow()
        {
            if (explorer == null) 
                throw new InvalidOperationException("主窗口尚未创建");
            return explorer;
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

        private SubExplorerController CreateSubExplorer(ISettings settings, ICoreData coreData, IToastCanvasController toastCanvasController)
        {
            subExplorerController = new SubExplorerController(coreData, settings, toastCanvasController);
            return subExplorerController;
        }

        //事件响应方法

        private void OnAppExitRequested()
        {
            AppExitRequested?.Invoke();
        }

    }

    public interface IMainWindowControler : IModule
    {
        public Window GetWindow();
        public event Action? AppExitRequested;
    }
}
