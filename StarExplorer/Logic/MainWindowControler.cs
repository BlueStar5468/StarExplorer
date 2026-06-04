using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using StarExplorer.Controls;
using StarExplorer.Shared;
using StarExplorer.Views;
using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        //事件
        public event Action? AppExitRequested;

        public MainWindowControler(ISettings settings, ICoreData coreData, ITabManager tabManager)
        {
            this.settings = settings;
            this.coreData = coreData;
            this.tabManager = tabManager;
        }

        private void CreateWindow()
        {
            //依据Settings创建DataContext
            explorerData = new ExplorerData(settings, coreData);
            explorerData.displayMode = DisplayMode.Devices;//默认显示设备列表
            SetLayoutMode();

            explorer = new Explorer(explorerData);
            explorer.DataContext = explorerData;
            
            //创建自绘组件
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
            //explorerData.AppExitRequested += OnAppExitRequested;
            explorer.AddButtonClicked += AddTab;
            explorer.Opened += OnWindowOpened;
            tabManager.PropertyChanged += OnSelectedTabChanged;
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

        private Panel CreateDevicePanel()
        {
            DevicePanelData data = new DevicePanelData(settings, coreData);
            DevicesPanel devicesPanel = new DevicesPanel(data);

            return devicesPanel.GetInstance();
        }

        internal void CreateTabBar()
        {
            var tabControl = new ItemsControl
            {
                ItemsPanel = new FuncTemplate<Panel?>(() =>
                {
                    StackPanel panel = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    return panel;
                }),

                ItemTemplate = new FuncDataTemplate<ITabContent>((tabContent, _) =>
                {
                    Controls.TabControl tabItem = new Controls.TabControl(
                        Color.Parse(tabContent.BackgroundColor),
                        explorerData.TabWidth,
                        explorerData.TabHeight,
                        sideMargin: 3,
                        CornerRadius: 5,
                        imageSize: 16,
                        tabContent.Icon,
                        imageMargin: 5,
                        tabContent.Label,
                        closeButtonSize: 16,
                        id: tabContent.Index,
                        explorerData.HoverBackgroundColor,
                        tabContent
                    );
                    //标签页事件绑定
                    tabItem.TabClosed += tabManager.CloseTab;
                    //tabItem.TabClosed += data.CheckExit;
                    tabItem.TabClicked += tabManager.OnTabClicked;

                    return tabItem.GetInstance();
                }, supportsRecycling: true)
            };
            tabControl.ItemsSource = tabManager.tabContents;

            Panel container = new Panel();
            Grid.SetColumn(container, 0);
            container.Children.Add(tabControl);
            explorer.MountToTabGrid(container);
        }

        private void AddTab()
        {
            if (explorerData == null) throw new Exception("数据上下文尚未创建，无法添加标签页");
            tabManager.NewTab(GetDefaltDisplay(explorerData.startLocation), explorerData.tabBackgroundColor, out int _);
        }

        private void OnWindowOpened(object? sender , EventArgs e)
        {
            if (Design.IsDesignMode) return;
            if (explorerData == null) throw new Exception("数据上下文尚未创建，无法执行窗口打开后的初始化逻辑");
            if (explorer == null) throw new Exception("主窗口尚未创建，无法执行窗口打开后的初始化逻辑");
            //创建Tab栏以及初始标签页
            CreateTabBar();
            //TODO: 初始标签页的内容应该根据实际需求进行设置，目前仅添加了一个空的 ItemsControl 作为占位符
            int id;
            tabManager.NewTab(GetDefaltDisplay(explorerData.startLocation), explorerData.tabBackgroundColor, out id);
            tabManager.SelectTab(id);

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (explorer == null) throw new Exception("主窗口尚未创建，无法刷新显示");
            explorer.ClearMainDisplay();
            
            if (tabManager.CurrentTabId == -1) return; //没有选中标签页时直接返回
            explorer.MountToMainDisplay(tabManager.GetSelectedTab().Content);
        }

        private void OnSelectedTabChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (explorerData == null) throw new Exception("数据上下文尚未创建");
            if (e.PropertyName == "CurrentTabId")
            {
                foreach (var tab in tabManager.tabContents)
                {
                    if (tab.Index == tabManager.CurrentTabId)
                    {
                        tab.BackgroundColor = explorerData.SelectedBackgroundColor;
                        tab.IsSelected = true;
                    }
                    else
                    {
                        tab.BackgroundColor = explorerData.tabBackgroundColor;
                        tab.IsSelected = false;
                    }
                }
            }
            RefreshDisplay();
        }


    }

    public interface IMainWindowControler : IModule
    {
        public Window GetWindow();
        public event Action? AppExitRequested;
    }
}
