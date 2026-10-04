using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Logic;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace StarExplorer.Views
{
    public class ExplorerData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //数据
        //依赖
        internal ITabManager tabManager = null!;
        //配置
        private string label = "StarExplorer";
        private string themeColor = Colors.AliceBlue.ToString();
        private int tabItemHeight = 30;
        private ExplorerLayout layout;

        internal StartLocation startLocation = StartLocation.Devices;


        //设计时数据构造器
        public static ExplorerData DesignInstance => new ExplorerData()
        {
            Label = "StarExplorer (Design Mode)",
            Layout = ExplorerLayout.Desktop
        };
        public ExplorerData() { }

        public ExplorerData(ISettings settings, ICoreData coreData, ITabManager tabManager) 
        {
            this.tabManager = tabManager;
            
            InitSettings(settings); 
            BindSettings(settings);
        }


        private void InitSettings(ISettings settings)
        {
            tabItemHeight = settings.TabItemHeight;
        }
        private void BindSettings(ISettings settings)
        {
            settings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.TabItemHeight):
                        TabItemHeight = settings.TabItemHeight;
                        break;
                }
            };
        }

        //向UI线程委托修改前端属性的事件通知，确保线程安全
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public string Label
        {
            get => label;
            set
            {
                if (label != value)
                {
                    label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public ExplorerLayout Layout { get => layout; set { if (layout != value) { layout = value; OnPropertyChanged(nameof(Layout)); } } }
        public string ThemeColor { get => themeColor; set { if (themeColor != value) { themeColor = value; OnPropertyChanged(nameof(ThemeColor)); } } }
        public int TabItemHeight { get => tabItemHeight; set { if (tabItemHeight != value) { tabItemHeight = value; OnPropertyChanged(nameof(TabItemHeight)); } } }
    }
}
