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

        //数据
        int fileItemHeight;
        int fileItemSpacing;
        int fileItemInternalMargin;
        int fileItemFontSize;

        IImage? folderIcon;
        IImage? folderLinkIcon;
        IImage? fileIcon;

        List<Item> items = new List<Item>(); //当前目录的文件和文件夹列表

        //封装属性
        public int FileItemHeight { get { return fileItemHeight; } set { fileItemHeight = value; OnPropertyChanged(nameof(FileItemHeight)); } }
        public int FileItemSpacing { get { return fileItemSpacing; } set { fileItemSpacing = value; OnPropertyChanged(nameof(FileItemSpacing)); } }
        public int FileItemInternalMargin { get { return fileItemInternalMargin; } set { fileItemInternalMargin = value; OnPropertyChanged(nameof(FileItemInternalMargin)); } }
        public int FileItemFontSize { get { return fileItemFontSize; } set { fileItemFontSize = value; OnPropertyChanged(nameof(FileItemFontSize)); } }

        public IImage? FolderIcon { get { return folderIcon; } set { folderIcon = value; OnPropertyChanged(nameof(FolderIcon)); } }
        public IImage? FolderLinkIcon { get { return folderLinkIcon; } set { folderLinkIcon = value; OnPropertyChanged(nameof(FolderLinkIcon)); } }
        public IImage? FileIcon { get { return fileIcon; } set { fileIcon = value; OnPropertyChanged(nameof(FileIcon)); } }


        public List<Item> Items { get { return items; } set { items = value; OnPropertyChanged(nameof(Items)); } }


        public ItemsPanelData(ISettings settings)
        {
            InitSettings(settings);
            BindSettings(settings);
        }

        private void InitSettings(ISettings settings)
        {
            this.fileItemHeight = settings.FileItemHeight;
            this.fileItemSpacing = settings.FileItemSpacing;
            this.fileItemInternalMargin = settings.FileItemInternalMargin;
            this.fileItemFontSize = settings.TextFontSize;

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
            foreach (Item item in newItems)
            {
                Items.Add(item);
            }
            this.OnPropertyChanged(nameof(Items));
        }
    }
}
