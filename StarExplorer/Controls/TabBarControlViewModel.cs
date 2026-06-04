using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    public class TabBarControlViewModel : INotifyPropertyChanged
    {
        //属性和设置项
        private String backGroundColor;
        private String tabItemBackgroundColor;
        private String hoverColor;
        private String selectedColor;
        private int barMarginValue;
        private int itemSideMargin;
        private int itemHeightMargin;
        private CornerRadius cornerRadius;
        private int tabHeight;
        private int tabItemWidth;
        private int tabFontSize;
        public String BackGroundColor { get => backGroundColor; set { if (backGroundColor != value) { backGroundColor = value; OnPropertyChanged(nameof(BackGroundColor)); } } }
        public int BarMarginValue { get => barMarginValue; set { if (barMarginValue != value) { barMarginValue = value; OnPropertyChanged(nameof(BarMarginValue)); OnPropertyChanged(nameof(BarMargin)); } } }
        public Thickness BarMargin { get => new Thickness(BarMarginValue); }
        public int ItemSideMargin { get => itemSideMargin; set { if (itemSideMargin != value) { itemSideMargin = value; OnPropertyChanged(nameof(ItemSideMargin)); } } }
        public int ItemHeightMargin { get => itemHeightMargin; set { if (itemHeightMargin != value) { itemHeightMargin = value; OnPropertyChanged(nameof(ItemHeightMargin)); } } }
        public CornerRadius CornerRadius { get => cornerRadius; set { if (cornerRadius != value) { cornerRadius = value; OnPropertyChanged(nameof(CornerRadius)); } } }
        public String TabItemBackgroundColor { get => tabItemBackgroundColor; set { if (tabItemBackgroundColor != value) { tabItemBackgroundColor = value; OnPropertyChanged(nameof(TabItemBackgroundColor)); } } }
        public String HoverColor { get => hoverColor; set { if (hoverColor != value) { hoverColor = value; OnPropertyChanged(nameof(HoverColor)); } } }
        public String SelectedColor { get => selectedColor; set { if (selectedColor != value) { selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); } } }
        public int TabHeight { get => tabHeight; set { if (tabHeight != value) { tabHeight = value; OnPropertyChanged(nameof(TabHeight)); } } }
        public int TabItemWidth { get => tabItemWidth; set { if (tabItemWidth != value) { tabItemWidth = value; OnPropertyChanged(nameof(TabItemWidth)); } } }
        public int TabFontSize { get => tabFontSize; set { if (tabFontSize != value) { tabFontSize = value; OnPropertyChanged(nameof(TabFontSize)); } } }


        public int TabItemBorderHeight { get => tabHeight - 2 * barMarginValue - 2 * itemHeightMargin; }
        public int TabItemHeight { get => tabHeight - 2 * barMarginValue - 4 * itemHeightMargin; }
        public Thickness ItemMargin { get => new Thickness(itemSideMargin, itemHeightMargin, itemSideMargin, itemHeightMargin); }

        public static TabBarControlViewModel DesignInstance { get; } = new TabBarControlViewModel
        {
            BackGroundColor = Colors.Red.ToString(),
            TabItemBackgroundColor = Colors.LightBlue.ToString(),
            BarMarginValue = 2,
            ItemSideMargin = 3,
            ItemHeightMargin = 2,
            CornerRadius = new CornerRadius(5),
            TabHeight = 40,
            TabItemWidth = 100,
            TabFontSize = 14,
            hoverColor = "rgb(224,238,249)",
            tabs = new ObservableCollection<ITabContent>
             {
                 new TabContent("标签1", null, null, 0, Colors.LightBlue.ToString()),
             }
        };
#pragma warning disable CS8618 
        public TabBarControlViewModel() { }
#pragma warning restore CS8618

        public TabBarControlViewModel(ExplorerData data)
        {
            backGroundColor = data.ThemeColor;
            BarMarginValue = data.BarMarginValue;
            ItemSideMargin = data.ItemSideMargin;
            ItemHeightMargin = data.ItemHeightMargin;
            CornerRadius = new CornerRadius(data.BarCornerRadius);
            tabItemBackgroundColor = data.tabItemBackgroundColor;
            hoverColor = data.HoverBackgroundColor;
            selectedColor = data.SelectedBackgroundColor;
            TabHeight = data.TabHeight;
            TabItemWidth = data.TabItemWidth;
            TabFontSize = data.FontSizeText;
            tabs = data.tabManager.tabContents;
            //事件绑定
            //data.tabManager.tabContents.CollectionChanged += SyncTabContents;
        }

        //Tab项显示资源
        public ObservableCollection<ITabContent> tabs { get; set; } = new ObservableCollection<ITabContent>();



        //事件
        public event PropertyChangedEventHandler? PropertyChanged;



        

        //事件响应方法
        private void SyncTabContents(object? sender, NotifyCollectionChangedEventArgs e)
        {
            e.NewItems?.Cast<ITabContent>().ToList().ForEach(tab => tabs.Add(tab));
            e.OldItems?.Cast<ITabContent>().ToList().ForEach(tab => tabs.Remove(tab));
        }

        internal void CloseButtonClicked(int id)
        {
            
        }

        internal void TabClicked(int id)
        {

        }

        //封装方法
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TabContent : INotifyPropertyChanged, ITabContent
    {
        private String label = "未命名标签页";
        private String backgroundColor = null!;
        private IImage icon = null!;
        private int index = 0;
        private Panel content = null!;//标签页的通用内容容器，具体内容由外部设置和管理
        private bool isSelected = false;
        public bool IsSelected { get => isSelected; set { if (isSelected != value) { isSelected = value; OnPropertyChanged(nameof(IsSelected)); } } }
        public String Label { get => label; set { if (label != value) { label = value; OnPropertyChanged(nameof(Label)); } } }
        public IImage Icon { get => icon; set { if (icon != value) { icon = value; OnPropertyChanged(nameof(Icon)); } } }
        public int Index { get => index; set { if (index != value) { index = value; OnPropertyChanged(nameof(Index)); } } }
        public Panel Content { get => content; set { if (content != value) { content = value; OnPropertyChanged(nameof(Content)); } } }
        public String BackgroundColor { get => backgroundColor; set { if (backgroundColor != value) { backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); } } }

        public TabContent(String label, IImage icon, Panel content, int index, string backgroundColor)
        {
            this.Label = label;
            this.Icon = icon;
            this.Index = index;
            this.Content = content;
            this.BackgroundColor = backgroundColor;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }

    public interface ITabContent : INotifyPropertyChanged
    {
        public String Label { get; set; }
        public IImage Icon { get; set; }
        public int Index { get; set; }
        public Panel Content { get; set; }
        public String BackgroundColor { get; set; }
        public bool IsSelected { get; set; }
    }
}
