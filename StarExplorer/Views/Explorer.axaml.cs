using Avalonia.Controls;
using Avalonia;
using System.Threading.Tasks;
using StarExplorer.Shared;
using Avalonia.Media;
using StarExplorer.Controls;
using System;
using Avalonia.Platform;
using Avalonia.Controls.Templates;
using System.ComponentModel;
using Avalonia.Data;

namespace StarExplorer.Views
{
    public partial class Explorer : Window
    {
        //数据模型引用
        ExplorerData data;

        public Explorer(ExplorerData data)
        {
            InitializeComponent();

            this.data = data;


            //事件绑定
            MainDisplayPanel.SizeChanged += OnMainDisplaySizeChangedAsync;
            AddButton.Click += (s, e) => { data.tabManager.NewTab(GetDefaltDisplay(data.startLocation), data.tabBackgroundColor, out int _); };
            data.tabManager.PropertyChanged += OnSelectedTabChanged;
        }

        //以下构造器仅供设计时使用，运行时请使用带参数的构造器
        #region 设计时构造器
#pragma warning disable CS8618
        public Explorer()
#pragma warning restore CS8618 
        {
            InitializeComponent();
        }
        #endregion

        //刷新显示
        private void RefreshDisplay()
        {
            ClearMainDisplay();

            RefreshDisplayPanel();
        }

        //显示区设置
        //此方法除现有调用外，不应以任何形式被调用，除非你非常清楚调用它的后果（例如可能会导致性能问题或UI异常）。
        //如果需要刷新显示，请调用 RefreshDisplay() 方法。
        private void RefreshDisplayPanel()
        {
            //注：入口线程为主线程
            if (data.tabManager.CurrentTabId == -1) return; //没有选中标签页时直接返回
            MainDisplayPanel.Children.Add(data.tabManager.GetSelectedTab().Content);
        }

        //清除主要显示区的内容
        private void ClearMainDisplay()
        {
            MainDisplayPanel.Children.Clear();
        }

        //创建Tab栏
        private void CreateTabBar()
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
                        data.TabWidth,
                        data.TabHeight,
                        sideMargin: 3,
                        CornerRadius: 5,
                        imageSize: 16,
                        tabContent.Icon,
                        imageMargin: 5,
                        tabContent.Label,
                        closeButtonSize: 16,
                        id: tabContent.Index,
                        data.HoverBackgroundColor,
                        tabContent
                    );
                    //标签页事件绑定
                    tabItem.TabClosed += data.tabManager.CloseTab;
                    tabItem.TabClosed += data.CheckExit;
                    tabItem.TabClicked += data.tabManager.OnTabClicked;

                    return tabItem.GetInstance();
                }, supportsRecycling: true)
            };
            tabControl.ItemsSource = data.tabManager.tabContents;

            Grid.SetColumn(tabControl, 0);
            TabGird.Children.Add(tabControl);
        }


        //初始刷新
        protected override async void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);

            if (Design.IsDesignMode) return;
            //提醒VM加载资源
            //await data.LoadResorces();
            //创建Tab栏以及初始标签页
            CreateTabBar();
            //TODO: 初始标签页的内容应该根据实际需求进行设置，目前仅添加了一个空的 ItemsControl 作为占位符
            int id;
            data.tabManager.NewTab(GetDefaltDisplay(data.startLocation), data.tabBackgroundColor, out id);
            data.tabManager.SelectTab(id);

            RefreshDisplay();
        }

        //根据一个起始显示位置生成一个新的显示内容实例
        public Panel GetDefaltDisplay(StartLocation startLocation)
        {
            if (startLocation == StartLocation.Devices)
            {
                DivicesPanel divicesPanel = new DivicesPanel(data);
                return divicesPanel.GetInstance();
            }
            else
            {
                //TODO: 根据其他起始位置生成相应的显示内容，目前仅实现了设备显示的生成逻辑
                return new StackPanel();
            }
        }

        //事件响应方法
        public void OnMainDisplaySizeChangedAsync(object? sender, SizeChangedEventArgs args)
        {
            data.mainDisplaySize = MainDisplayPanel.Bounds.Size;
        }

        public void OnSelectedTabChanged(Object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentTabId")
            {
                foreach (var tab in data.tabManager.tabContents)
                {
                    if (tab.Index == data.tabManager.CurrentTabId)
                    {
                        tab.BackgroundColor = data.SelectedBackgroundColor;  
                        tab.IsSelected = true;
                    }
                    else
                    {
                        tab.BackgroundColor = data.tabBackgroundColor;
                        tab.IsSelected = false;
                    }
                }
            }
            RefreshDisplay();
        }
    }
}