using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Logic;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ItemsPanelData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        //依赖项
        IDManager idManager;
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

        List<Item> items = new List<Item>(); //当前目录的文件和文件夹列表
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


        //注:以下两个属性的OnPropertyChanged方法在此处没有实现 因为需要手动控制并配合UI修改引用绑定
        public List<Item> Items { get { return items; } set { items = value; } }
        public ObservableCollection<IItemsDataContext> ItemsDataContexts { get { return itemsDataContexts; } set { itemsDataContexts = value; } }

        public ItemsPanelData(ISettings settings)
        {
            idManager = new IDManager(0);

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
        }
        private void BindSettings(ISettings settings)
        {
            settings.PropertyChanged += (s, e) =>
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
            Items.Clear();
            ItemsDataContexts.Clear();
            idManager.Clear();
            foreach (Item item in newItems)
            {
                Items.Add(item);
                ItemDataContext itemDataContext = new ItemDataContext(idManager.GetID(), item);
                ItemsDataContexts.Add(itemDataContext);
            }
            this.OnPropertyChanged(nameof(Items));
            this.OnPropertyChanged(nameof(ItemsDataContexts));
        }
    }

    internal class ItemDataContext : IItemsDataContext, INotifyPropertyChanged
    {
        int id;
        Item itemToDisplay;
        bool isSelected = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ID { get { return id; } }
        public Item ItemToDisplay { get { return itemToDisplay; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public ItemDataContext(int id, Item itemToDisplay)
        {
            this.id = id;
            this.itemToDisplay = itemToDisplay;
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
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
