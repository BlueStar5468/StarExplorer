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
        public event Action? AddButtonClicked;

        //数据模型引用
        ExplorerData data;

        public Explorer(ExplorerData data)
        {
            InitializeComponent();

            this.data = data;


            //事件绑定
            AddButton.Click += (s, e) => { AddButtonClicked?.Invoke(); };
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

        internal void MountToMainDisplay(Panel content)
        {
            SubExplorerBorder.Child = content;
        }
        
        internal void MountToTabGrid(Panel content)
        {
            TabGird.Children.Add(content);
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