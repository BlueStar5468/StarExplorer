using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Controls;
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
        ITabManager tabManager;
        //管理的窗口
        Explorer? explorer;
        ExplorerData? explorerData;
        TabBarPanelData? tabBarPanelData;

        //事件
        public event Action? AppExitRequested;

        public MainWindowControler(ISettings settings, ICoreData coreData, ITabManager tabManager)
        {
            this.settings = settings;
            this.coreData = coreData;
            this.tabManager = tabManager;
        }


        public void Initialize()
        {
            CreateWindow();
            BindEvents();
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

        //内部操作方法
        private void BindEvents()
        {
            if (explorer == null || explorerData == null)
                throw new InvalidOperationException("主窗口或数据上下文尚未创建，无法绑定事件");
            //explorerData.AppExitRequested += OnAppExitRequested;
            explorer.AddButtonClicked += AddTab;
            explorer.Opened += OnWindowOpened;
            tabManager.PropertyChanged += OnSelectedTabChanged;
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
            DevicePanelData data = new DevicePanelData(settings, coreData);
            DevicesPanel devicesPanel = new DevicesPanel(data);

            return devicesPanel.GetInstance();
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
            tabManager.NewTab(GetDefaltDisplay(explorerData.startLocation), out int _);
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

        public Panel GetDefaltDisplay(StartLocation startLocation)
        {
            //获取标签页的初始内容
            if (startLocation == StartLocation.Devices)
            {
                var panel = CreateDevicePanel();
                return panel;
            }
            else
            {
                //TODO: 根据其他起始位置生成相应的显示内容，目前仅实现了设备显示的生成逻辑
                return new StackPanel();
            }
        }

        //事件响应方法
        private void OnWindowOpened(object? sender, EventArgs e)
        {
            if (Design.IsDesignMode) return;
            if (explorerData == null) throw new Exception("数据上下文尚未创建，无法执行窗口打开后的初始化逻辑");
            if (explorer == null) throw new Exception("主窗口尚未创建，无法执行窗口打开后的初始化逻辑");
            //创建Tab栏以及初始标签页
            CreateTabBar();
            int id;
            tabManager.NewTab(GetDefaltDisplay(explorerData.startLocation), out id);
            tabManager.SelectTab(id);

            RefreshDisplay();
        }

        private void OnSelectedTabChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (explorerData == null) throw new Exception("数据上下文尚未创建");
            if (tabBarPanelData == null) throw new Exception("标签栏数据尚未创建，无法响应标签页变更事件");
            if (e.PropertyName == "CurrentTabId")
            {
                foreach (var tab in tabBarPanelData.TabDisplayContents)
                {
                    if (tab.Index == tabManager.CurrentTabId)
                    {
                        tab.CurrentBackgroundColor = Color.Parse(explorerData.SelectedBackgroundColor);
                        tab.IsSelected = true;
                    }
                    else
                    {
                        tab.CurrentBackgroundColor = Color.Parse(explorerData.tabBackgroundColor);
                        tab.IsSelected = false;
                    }
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
