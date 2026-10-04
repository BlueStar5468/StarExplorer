using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ItemsPanel
    {
        //管理数据
        Panel root;
        ItemsPanelData data;
        //所有的Item实例 注：不再维护所有Control引用 容易造成与Context不同步
        //List<FolderItemsControl> folderItemsControls = new List<FolderItemsControl>();
        //事件
        public event Action<string>? PathChanged; //当用户双击文件夹时触发,传递目标路径
        public ItemsPanel(ItemsPanelData dataContext)
        {
            root = new Panel();
            this.data = dataContext;

            DockPanel dock = new DockPanel();

            //构建列表头
            Grid headerGrid = new Grid();
            //TODO:可调整的列数量和宽度
            ColumnDefinition IconColumn = new ColumnDefinition();
            IconColumn.Bind(ColumnDefinition.WidthProperty, new Avalonia.Data.Binding(nameof(dataContext.FileItemHeight)) { Source = dataContext, Converter = new GridLengthConverter() });
            ColumnDefinition NameConlumn = new ColumnDefinition();
            NameConlumn.Bind(ColumnDefinition.WidthProperty, new Avalonia.Data.Binding(nameof(dataContext.FileItemNameWidth)) { Source = dataContext, Converter = new GridLengthConverter() });
            ColumnDefinition ColumnDefinition3 = new ColumnDefinition();
            headerGrid.ColumnDefinitions.Add(IconColumn);
            headerGrid.ColumnDefinitions.Add(NameConlumn);
            headerGrid.ColumnDefinitions.Add(ColumnDefinition3);

            TextBlock nameHeader = new TextBlock()
            {
                Text = "名称",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            TextBlock timeHeader = new TextBlock()
            {
                Text = "修改时间",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            Grid.SetColumn(nameHeader, 1);
            Grid.SetColumn(timeHeader, 2);
            headerGrid.Children.Add(nameHeader);
            headerGrid.Children.Add(timeHeader);

            DockPanel.SetDock(headerGrid, Dock.Top);
            dock.Children.Add(headerGrid);

            ScrollViewer scrollViewer = new ScrollViewer()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            };

            var ItemsControl = new ItemsControl
            {
                ItemsPanel = new FuncTemplate<Panel?>(() =>
                {
                    var panel = new StackPanel
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                        Orientation = Avalonia.Layout.Orientation.Vertical,
                    };
                    return panel;
                }),

                ItemTemplate = new FuncDataTemplate<IItemsDataContext>((itemDataContext, _) =>
                {
                    FolderItemsControl folderItemsControl = new FolderItemsControl(dataContext, itemDataContext);
                    //this.folderItemsControls.Add(folderItemsControl);
                    //事件绑定
                    folderItemsControl.DoubleClicked += OnItemDoubleClicked;
                    folderItemsControl.LeftClicked += OnItemLeftClicked;

                    return folderItemsControl.GetInstance();
                }, false)
                //此处false不复用是必要的,防止切换目录后,ItemsControl的ItemTemplate复用旧的FolderItemsControl,导致显示错误 
            };
            ItemsControl.ItemsSource = dataContext.ItemsDataContexts;
            //绑定items变化
            dataContext.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(dataContext.ItemsDataContexts))
                {
                    ItemsControl.ItemsSource = null;
                    ItemsControl.ItemsSource = dataContext.ItemsDataContexts;
                }
            };

            scrollViewer.Content = ItemsControl;

            dock.Children.Add(scrollViewer);

            root.Children.Add(dock);
        }

        public Panel GetInstance()
        {
            return root;
        }

        //封装方法
        private IItemsDataContext? GetItemDataContextByID(int id)
        {
            foreach (var dataContext in data.ItemsDataContexts)
            {
                if (dataContext.ID == id)
                {
                    return dataContext;
                }
            }
            return null;
        }

        private void OnItemDoubleClicked(int id)
        {
            Debug.WriteLine($"Item with ID {id} was double-clicked.");
            var itemDataContext = GetItemDataContextByID(id);
            if (itemDataContext == null) return;
            //在此处添加不同类型文件的双击逻辑
            if (itemDataContext.ItemToDisplay.Type == ItemType.Folder ||
                itemDataContext.ItemToDisplay.Type == ItemType.Folder_Link)
            {
                //开始组装目标地址
                string currentPath = itemDataContext.ItemToDisplay.Path;
                string targetPath = Path.Combine(currentPath, "");
                this.PathChanged?.Invoke(targetPath); //触发事件,传递目标路径
            }
        }

        private void OnItemLeftClicked(int id)
        {
            ResetSelection();
            IItemsDataContext? itemDataContext = GetItemDataContextByID(id);
            if (itemDataContext != null)
            {
                itemDataContext.IsSelected = true;
            }
        }

        private void ResetSelection()
        {
            foreach (var dataContext in data.ItemsDataContexts)
            {
                dataContext.IsSelected = false;
            }
        }
    }
}
