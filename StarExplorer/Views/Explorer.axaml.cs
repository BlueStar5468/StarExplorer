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
using StarExplorer.Logic;

namespace StarExplorer.Views
{
    public partial class Explorer : Window
    {
        //依赖
        ExplorerData data;
        
        //数据
        //挂载的子Explorer的Border 注：不是子Explorer的实际内容，而是子Explorer的内容区
        Border SubExplorerBorder = null!;
        //存在于SubExplorerBorder上的Cavans绘图区容器
        Border SubExplorerMessageBorder = null!;
        
        //事件
        public event Action? AddButtonClicked;

        //本地委托
        EventHandler<Avalonia.Input.PointerPressedEventArgs>? pointerpressed;

        public Explorer(ExplorerData data)
        {
            this.data = data;
            
            CreateWindow();
            InitHandler();
            BindEvents();
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

        //封装方法

        private void CreateWindow()
        {
            InitializeComponent();

            //构建剩余窗口内容
            SubExplorerBorder = new Border()
            {
                Name = "SubExplorerBorder",
            };
            SubExplorerMessageBorder = new Border()
            {
                Background = null,  //注:保证null,否则会遮挡下层控件
                Name = "SubExplorerMessageBorder",
                IsHitTestVisible = true,   //注:不要使用点击穿透,为继承属性 必须整条链路全部可用才能点击
                ZIndex = 1000,
            };

            //注：保证Canvas后挂载以确保其在最上层
            this.SubExplorerRootPanel.Children.Add(SubExplorerBorder);
            this.SubExplorerRootPanel.Children.Add(SubExplorerMessageBorder);

        }

        private void InitHandler()
        {
            pointerpressed = (s, e) =>
            {
                //handler为false时没有实际控件接受这个事件
                //表示点击了空白区域,此时清除焦点
                if (e.Handled == false)
                {
                    this.Focus();  //清除焦点
                }
            };
        } 

        private void BindEvents()
        {
            //不需要清除事件绑定,因为窗口生命周期为整个应用程序生命周期
            AddButton.Click += (s, e) => { AddButtonClicked?.Invoke(); };

            this.PointerPressed += pointerpressed;
        }

        internal void MountToMainDisplay(Panel content)
        {
            SubExplorerBorder.Child = content;
        }
        
        internal void MountToTabGrid(Panel content)
        {
            TabGird.Children.Add(content);
        }

        internal void MountToSubExplorerMessageBorder(Panel content)
        {
            SubExplorerMessageBorder.Child = content;
        }

        //清除主要显示区的内容
        internal void ClearMainDisplay()
        {
            SubExplorerBorder.Child = null;
        }


        //初始刷新
        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);
        }

    }
}