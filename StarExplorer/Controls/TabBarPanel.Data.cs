using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Logic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StarExplorer.Controls
{
    internal class TabBarPanelData : INotifyPropertyChanged
    {
        #region 设置项
        private Color tabItembackGroundColor;   //标签页项背景色
        private Color hoverColor;
        private Color selectedColor;
        private int itemWidth;                      //标签页项宽度
        private int itemHeight;                     //标签页项高度
        private int itemMargin;
        private int itemCornerRadius;
        private int barHeight;
        private int barMargin;
        private int barCornerRadius;
        #endregion

        #region 封装属性
        public Color TabItemBackGroundColor { get => tabItembackGroundColor; set { if (!Equals(tabItembackGroundColor, value)) { tabItembackGroundColor = value; OnPropertyChanged(nameof(TabItemBackGroundColor)); } } }
        public Color HoverColor { get => hoverColor; set { if (hoverColor != value) { hoverColor = value; OnPropertyChanged(nameof(HoverColor)); } } }
        public Color SelectedColor { get => selectedColor; set { if (selectedColor != value) { selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); } } }
        public int ItemWidth { get => itemWidth; set { if (itemWidth != value) { itemWidth = value; OnPropertyChanged(nameof(ItemWidth)); OnPropertyChanged(nameof(MaxTextWidth)); } } }

        public int ItemHeight { get => itemHeight; set { if (itemHeight != value) { itemHeight = value; OnPropertyChanged(nameof(ItemHeight)); OnPropertyChanged(nameof(CloseButtonSize)); OnPropertyChanged(nameof(ImageSize)); OnPropertyChanged(nameof(MaxTextWidth)); } } }

        public int ItemMargin { get => itemMargin; set { if (itemMargin != value) { itemMargin = value; OnPropertyChanged(nameof(ItemMargin)); OnPropertyChanged(nameof(MaxTextWidth)); } } }

        public int ItemCornerRadius { get => itemCornerRadius; set { if (itemCornerRadius != value) { itemCornerRadius = value; OnPropertyChanged(nameof(ItemCornerRadius)); } } }
        public int BarHeight { get => barHeight; set { if (barHeight != value) { barHeight = value; OnPropertyChanged(nameof(BarHeight)); } } }
        public int BarMargin { get => barMargin; set { if (barMargin != value) { barMargin = value; OnPropertyChanged(nameof(BarMargin)); } } }
        public int BarCornerRadius { get => barCornerRadius; set { if (barCornerRadius != value) { barCornerRadius = value; OnPropertyChanged(nameof(BarCornerRadius)); } } }

        public int ImageSize { get => ItemHeight;  }

        public int CloseButtonSize { get => ItemHeight;  }
        public int MaxTextWidth { get => ItemWidth - ImageSize - CloseButtonSize - 6 * ItemMargin; }
        #endregion

        #region 显示项抽象数据
        public ObservableCollection<ITabDisplayContent> TabDisplayContents { get; set; } = new ObservableCollection<ITabDisplayContent>();

        private void SyncDisplayContents(ITabManager tabManager)
        {
            ObservableCollection<ITabDisplayContent> newDisplayContents = new ObservableCollection<ITabDisplayContent>(TabDisplayContents);
            AddItem(tabManager, newDisplayContents);
            RemoveItem(tabManager, newDisplayContents);
            TabDisplayContents = newDisplayContents;
            OnPropertyChanged(nameof(TabDisplayContents));
        }

        private void AddItem(ITabManager tabManager, ObservableCollection<ITabDisplayContent> newContent)
        {
            foreach (var item in tabManager.tabContents)
            {
                bool isItemFounded = false;
                foreach (var displayItem in TabDisplayContents)
                {
                    if (displayItem.Index == item.Index)
                    {
                        isItemFounded = true;
                        break;
                    }
                }
                if (isItemFounded == false)
                {
                    TabDisplayContent newDisplayContent = new TabDisplayContent
                    {
                        Index = item.Index,
                        Content = item.Content,
                        CurrentBackgroundColor = TabItemBackGroundColor,
                        Icon = null!, //TODO:设置默认图标
                        Label = $"标签{item.Index}", //TODO:设置默认标签
                        IsSelected = false
                    };
                    newContent.Add(newDisplayContent);
                }

            }
        }

        private void RemoveItem(ITabManager tabManager, ObservableCollection<ITabDisplayContent> newContent)
        {
            foreach (var displayItem in TabDisplayContents)
            {
                bool isItemFounded = false;
                foreach (var item in tabManager.tabContents)
                {
                    if (displayItem.Index == item.Index)
                    {
                        isItemFounded = true;
                        break;
                    }
                }
                if (isItemFounded == false)
                {
                    newContent.Remove(displayItem);
                }
            }
        }

        #endregion
        public TabBarPanelData(ISettings settings, ITabManager tabManager)
        {
            InitSettings(settings);
            BindSettings(settings);
            //事件绑定
            tabManager.tabContents.CollectionChanged += (s, e) => SyncDisplayContents(tabManager);
        }

        private void InitSettings(ISettings settings)
        {
            TabItemBackGroundColor = settings.TabItemBackgroundColor_Color;
            HoverColor = settings.HoverBackgroundColor_Color;
            SelectedColor = settings.SelectedBackgroundColor_Color;

            ItemWidth = settings.TabItemWidth;
            ItemHeight = settings.TabItemHeight;
            ItemMargin = settings.TabItemMargin;
            ItemCornerRadius = settings.TabItemCornerRadius;
            BarHeight = settings.TabTotalHeight;
            BarMargin = settings.TabBarMargin;
            BarCornerRadius = settings.TabBarCornerRadius;
        }

        private void BindSettings(ISettings settings)
        {
            settings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.WindowDisplayBackgroundColor):
                        TabItemBackGroundColor = settings.TabItemBackgroundColor_Color;
                        break;
                    case nameof(settings.HoverBackgroundColor):
                        HoverColor = settings.HoverBackgroundColor_Color;
                        break;
                    case nameof(settings.SelectedBackgroundColor):
                        SelectedColor = settings.SelectedBackgroundColor_Color;
                        break;
                    case nameof(settings.TabItemWidth):
                        ItemWidth = settings.TabItemWidth;
                        break;
                    case nameof(settings.TabItemHeight):
                        ItemHeight = settings.TabItemHeight;
                        break;
                    case nameof(settings.TabItemMargin):
                        ItemMargin = settings.TabItemMargin;
                        break;
                    case nameof(settings.TabItemCornerRadius):
                        ItemCornerRadius = settings.TabItemCornerRadius;
                        break;
                    case nameof(settings.TabTotalHeight):
                        BarHeight = settings.TabTotalHeight;
                        break;
                    case nameof(settings.TabBarMargin):
                        BarMargin = settings.TabBarMargin;
                        break;
                    case nameof(settings.TabBarCornerRadius):
                        BarCornerRadius = settings.TabBarCornerRadius;
                        break;
                }
            };
        }

        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }

    internal class TabDisplayContent : ITabDisplayContent, INotifyPropertyChanged
    {
        private int index;
        private Panel content = null!;
        private Color currentBackgroundColor;
        private IImage icon = null!;
        private string label = string.Empty;
        private bool isSelected;

        public int Index { get => index; set { index = value; OnPropertyChanged(nameof(Index)); } }
        public Panel Content { get => content; set { content = value; OnPropertyChanged(nameof(Index)); }  }
        public Color CurrentBackgroundColor { get => currentBackgroundColor; set { currentBackgroundColor = value; OnPropertyChanged(nameof(CurrentBackgroundColor)); } }
        public IImage Icon { get => icon; set { icon = value; OnPropertyChanged(nameof(Icon)); } }
        public string Label { get => label; set { label = value; OnPropertyChanged(nameof(Label)); } }
        public bool IsSelected { get => isSelected; set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }

    internal interface ITabDisplayContent : ITabContent
    {
        public Color CurrentBackgroundColor { get; set; }
        public IImage Icon { get; set; }
        public string Label { get; set; }
        public bool IsSelected { get; set; }
    }
}
