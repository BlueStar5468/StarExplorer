using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ItemsPanel
    {
        Panel root;
        public ItemsPanel(ItemsPanelData dataContext)
        {
            root = new Panel();

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

                ItemTemplate = new FuncDataTemplate<Item>((item, _) =>
                {
                    FolderItemsControl folderItemsControl = new FolderItemsControl(dataContext, item);
                    //事件绑定


                    return folderItemsControl.GetInstance();
                }, false)
            };
            ItemsControl.ItemsSource = dataContext.Items;
            //绑定items变化
            dataContext.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(dataContext.Items))
                {
                    ItemsControl.ItemsSource = null;
                    ItemsControl.ItemsSource = dataContext.Items;
                }
            };

            scrollViewer.Content = ItemsControl;

            root.Children.Add(scrollViewer);
        }

        public Panel GetInstance()
        {
            return root;
        }

    }
}
