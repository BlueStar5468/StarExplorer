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
            tabManager.NewTab(CreateSubExplorer(settings, coreData, (IToastCanvasController)toastCanvasController), out id);
            tabManager.SelectTab(id);
        }

        //内部操作方法
        private void BindEvents()
        {
            if (explorer == null || explorerData == null)
                throw new InvalidOperationException("主窗口或数据上下文尚未创建，无法绑定事件");
            //explorerData.AppExitRequested += OnAppExitRequested;
            explorer.AddButtonClicked += AddTab;
            explorer.Opened += OnWindowOpened;
            tabManager.SelectedTabChanged += OnSelectedTabChanged;
            //测试:创建测试Toast通知
            //toastCanvasController.ShowToast("测试通知");
        }

        private void RefreshDisplay()
        {
            if (explorer == null) throw new Exception("主窗口尚未创建，无法刷新显示");
            explorer.ClearMainDisplay();

            if (tabManager.CurrentTabId == -1) return; //没有选中标签页时直接返回
            explorer.MountToMainDisplay(tabManager.GetSelectedTab().Content);
        }


        private Panel CreateDevicePanel()
        {
            DevicePanelController devicePanelController = new DevicePanelController(settings, coreData);

            return devicePanelController.GetInstance();
        }

        internal void CreateTabBar()
        {
            tabBarPanelData = new TabBarPanelData(settings, tabManager);
            TabBarPanel tabBarPanel = new TabBarPanel(tabBarPanelData);
            explorer?.MountToTabGrid(tabBarPanel.GetInstance());
            //标签页事件绑定
            tabBarPanel.TabClicked += OnTabBarClicked;
            tabBarPanel.TabCloseButtonClicked += OnTabCloseButtonClicked;
        }

        private void AddTab()
        {
            if (explorerData == null) throw new Exception("数据上下文尚未创建，无法添加标签页");
            tabManager.NewTab(CreateSubExplorer(settings, coreData, (IToastCanvasController)toastCanvasController), out int _);
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

        public Panel CreateSubExplorer(ISettings settings, ICoreData coreData, IToastCanvasController toastCanvasController)
        {
            subExplorerController = new SubExplorerController(coreData, settings, toastCanvasController);
            return subExplorerController.GetInstance();
        }

        //事件响应方法
        private void OnWindowOpened(object? sender, EventArgs e)
        {
            if (Design.IsDesignMode) return;

            RefreshDisplay();
        }

        private void OnSelectedTabChanged()
        {
            if (explorerData == null) throw new Exception("数据上下文尚未创建");
            if (tabBarPanelData == null) throw new Exception("标签栏数据尚未创建，无法响应标签页变更事件");

            foreach (var tab in tabBarPanelData.TabDisplayContents)
            {
                if (tab.Index == tabManager.CurrentTabId)
                {
                    tab.CurrentBackgroundColor = tabBarPanelData.SelectedColor;
                    tab.IsSelected = true;
                }
                else
                {
                    tab.CurrentBackgroundColor = tabBarPanelData.TabItemBackGroundColor;
                    tab.IsSelected = false;
                }
            }
            RefreshDisplay();
        }

        private void OnTabBarClicked(int id)
        {
            tabManager.SelectTab(id);
        }

        private void OnTabCloseButtonClicked(int id)
        {
            tabManager.CloseTab(id);
        }

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
