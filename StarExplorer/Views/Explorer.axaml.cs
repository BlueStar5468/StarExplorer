using Avalonia.Controls;
using Avalonia;
using StarExplorer.ViewModels;
using System.Threading.Tasks;
using StarExplorer.Shared;
using Avalonia.Media;
using StarExplorer.Controls;
using System;
using Avalonia.Platform;
using Avalonia.Controls.Templates;
using System.ComponentModel;
using Avalonia.Data;
using System.Diagnostics;
using Avalonia.Interactivity;

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

            BindEvent();
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
            MountToMainDisplay(data.GetSelectedPanelContent());
        }

        //创建Tab栏
        private void CreateTabBar()
        {
            TabPanel tabPanel = new TabPanel(data);
            TabGird.Children.Add(tabPanel.GetInstance());
        }


        //初始刷新
        protected override async void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);

            if (Design.IsDesignMode) return;
            //提醒VM加载资源
            await data.LoadResorces();
            //创建Tab栏以及初始标签页
            //CreateTabBar();
            //初始标签页创建并切换
            int id;
            data.tabManager.NewTab(GetDefaltDisplay(data.startLocation), data.tabItemBackgroundColor, out id);
            data.tabManager.SelectAndSwitchTab(id);

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

        //封装方法
        private void MountToMainDisplay(Panel content)
        {
            ClearMainDisplay();
            MainDisplayPanel.Children.Add(content);
        }

        private void ClearMainDisplay()
        {
            MainDisplayPanel.Children.Clear();
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            int id;
            data.tabManager.NewTab(GetDefaltDisplay(data.startLocation), data.tabItemBackgroundColor, out id);
            data.tabManager.SelectAndSwitchTab(id);
        }

        private void BindEvent()
        {
            //事件绑定
            MainDisplayPanel.SizeChanged += data.MainDisplayPanelSizeBind;
            data.DisplayContentChanged += RefreshDisplay;
        }

        //事件响应方法
    }
}