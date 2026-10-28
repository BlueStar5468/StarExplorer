using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Logic;
using StarExplorer.Shared;
using StarExplorer.Shared.SharedInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ItemsPanelData : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        //依赖项
        IDManager idManager;
        ISettings settings;
        //本地委托
        PropertyChangedEventHandler? settingsChangedHandler;
        //数据
        int fileItemHeight;
        int fileItemSpacing;
        int fileItemInternalMargin;
        int fileItemFontSize;
        int fileItemNameWidth;
        Color hoverColor;
        Color selectedColor;

        IImage? folderIcon;
        IImage? folderLinkIcon;
        IImage? fileIcon;

        //对于某些后缀的图标 可能是不唯一的 此字典存储需要重复读取的扩展名
        Dictionary<string, string> uniqueFileImages = null!;
        //动态运行时的图标缓存
        Dictionary<string, IImage> fileImages = null!;

        //下面List用于绑定UI
        ObservableCollection<IItemsDataContext> itemsDataContexts = new ObservableCollection<IItemsDataContext>(); //组合item后的DataContext列表

        //封装属性
        public int FileItemHeight { get { return fileItemHeight; } set { fileItemHeight = value; OnPropertyChanged(nameof(FileItemHeight)); } }
        public int FileItemSpacing { get { return fileItemSpacing; } set { fileItemSpacing = value; OnPropertyChanged(nameof(FileItemSpacing)); } }
        public int FileItemInternalMargin { get { return fileItemInternalMargin; } set { fileItemInternalMargin = value; OnPropertyChanged(nameof(FileItemInternalMargin)); } }
        public int FileItemFontSize { get { return fileItemFontSize; } set { fileItemFontSize = value; OnPropertyChanged(nameof(FileItemFontSize)); } }
        public int FileItemNameWidth { get { return fileItemNameWidth; } set { fileItemNameWidth = value; OnPropertyChanged(nameof(FileItemNameWidth)); } }
        public Color HoverColor { get { return hoverColor; } set { hoverColor = value; OnPropertyChanged(nameof(HoverColor)); } }
        public Color SelectedColor { get { return selectedColor; } set { selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); } }

        public IImage? FolderIcon { get { return folderIcon; } set { folderIcon = value; OnPropertyChanged(nameof(FolderIcon)); } }
        public IImage? FolderLinkIcon { get { return folderLinkIcon; } set { folderLinkIcon = value; OnPropertyChanged(nameof(FolderLinkIcon)); } }
        public IImage? FileIcon { get { return fileIcon; } set { fileIcon = value; OnPropertyChanged(nameof(FileIcon)); } }


        //注:以下属性的OnPropertyChanged方法在此处没有实现 因为需要手动控制并配合UI修改引用绑定
        public ObservableCollection<IItemsDataContext> ItemsDataContexts { get { return itemsDataContexts; } set { itemsDataContexts = value; } }

        public ItemsPanelData(ISettings settings)
        {
            idManager = new IDManager(0);
            this.settings = settings;

            InitSettings(settings);
            BindSettings(settings);
        }

        private void InitSettings(ISettings settings)
        {
            this.fileItemHeight = settings.FileItemHeight;
            this.fileItemSpacing = settings.FileItemSpacing;
            this.fileItemInternalMargin = settings.FileItemInternalMargin;
            this.fileItemFontSize = settings.TextFontSize;
            this.fileItemNameWidth = settings.FileItemNameWidth;

            this.HoverColor = settings.HoverBackgroundColor_Color;
            this.SelectedColor = settings.SelectedBackgroundColor_Color;

            this.folderIcon = settings.FolderIcon;
            this.folderLinkIcon = settings.FolderLinkIcon;
            this.fileIcon = settings.FileIcon;

            //TODO: 初始化uniqueFileImages字典 目前没有使用

            //初始化图标缓存字典
            this.fileImages = new Dictionary<string, IImage>();
        }
        private void BindSettings(ISettings settings)
        {
            this.settingsChangedHandler = (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.FileItemHeight):
                        FileItemHeight = settings.FileItemHeight;
                        break;
                    case nameof(settings.FileItemSpacing):
                        FileItemSpacing = settings.FileItemSpacing;
                        break;
                    case nameof(settings.FolderIcon):
                        folderIcon = settings.FolderIcon;
                        break;
                    case nameof(settings.FolderLinkIcon):
                        folderLinkIcon = settings.FolderLinkIcon;
                        break;
                    case nameof(settings.FileIcon):
                        fileIcon = settings.FileIcon;
                        break;
                    case nameof(settings.FileItemInternalMargin):
                        fileItemInternalMargin = settings.FileItemInternalMargin;
                        break;
                    case nameof(settings.TextFontSize):
                        fileItemFontSize = settings.TextFontSize;
                        break;
                    case nameof(settings.FileItemNameWidth):
                        fileItemNameWidth = settings.FileItemNameWidth;
                        break;
                    case nameof(settings.HoverBackgroundColor_Color):
                        HoverColor = settings.HoverBackgroundColor_Color;
                        break;
                    case nameof(settings.SelectedBackgroundColor_Color):
                        SelectedColor = settings.SelectedBackgroundColor_Color;
                        break;
                }
            };
            settings.PropertyChanged += settingsChangedHandler;
        }

        //封装方法
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        internal void UpdateItems(List<Item> newItems)
        {
            ItemsDataContexts.Clear();
            idManager.Clear();
            var newContext = new ObservableCollection<IItemsDataContext>();
            foreach (Item item in newItems)
            {
                ItemDataContext itemDataContext = new ItemDataContext(idManager.GetID(), item, GetDefaultIcons(item));
                newContext.Add(itemDataContext);
            }
            //ItemsControl绑定的是属性而不是这个字段 因此可以替换引用来触发UI刷新 而不是Clear+AddRange
            ItemsDataContexts = newContext;
            this.OnPropertyChanged(nameof(ItemsDataContexts));
        }

        private IImage GetDefaultIcons(Item item)
        {
            //获取默认图标
            if (item.Type == ItemType.Folder)
            {
                return FolderIcon ?? throw new InvalidOperationException("FolderIcon is not set.");
            }
            else if (item.Type == ItemType.Folder_Link)
            {
                return FolderLinkIcon ?? throw new InvalidOperationException("FolderLinkIcon is not set.");
            }
            else
            {   //在此处添加更多的默认图标类型处理逻辑
                return FileIcon ?? throw new InvalidOperationException("FileIcon is not set.");
            }
        }

        public void Dispose()
        {
            //释放本地委托
            settings.PropertyChanged -= settingsChangedHandler;
            //销毁图标缓存
            this.fileImages.Clear();
            this.fileImages = null!;
        }
    }

    internal class ItemDataContext : IItemsDataContext, INotifyPropertyChanged
    {
        int id;
        Item itemToDisplay;
        IImage icon;
        bool isSelected = false;
        bool isIconLoaded = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ID { get { return id; } }
        public Item ItemToDisplay { get { return itemToDisplay; } }
        public bool IsIconLoaded { get { return isIconLoaded; } set { isIconLoaded = value; OnPropertyChanged(nameof(IsIconLoaded)); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public IImage Icon { get { return icon; } set { icon = value; OnPropertyChanged(nameof(Icon)); } }
        public ItemDataContext(int id, Item itemToDisplay, IImage icon)
        {
            this.id = id;
            this.itemToDisplay = itemToDisplay;
            this.icon = icon;
        }

        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }

    internal interface IItemsDataContext
    {
        public int ID { get; }
        public Item ItemToDisplay { get; }
        public bool IsSelected { get; set; }
        public bool IsIconLoaded { get; set; }
        public IImage Icon { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
