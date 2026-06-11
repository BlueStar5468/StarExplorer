using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StarExplorer.Logic;
using StarExplorer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class TabBarPanel
    {
        Panel root;

        public TabBarPanel(TabBarPanelData dataContext)
        {
            root = new Panel();

            var tabControl = new ItemsControl
            {
                ItemsPanel = new FuncTemplate<Panel?>(() =>
                {
                    StackPanel panel = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    return panel;
                }),

                ItemTemplate = new FuncDataTemplate<ITabDisplayContent>((tabDisplayContent, _) =>
                {
                    Controls.TabControl tabItem = new Controls.TabControl(dataContext, tabDisplayContent);
                    //标签页事件绑定
                    tabItem.TabCloseButtonClicked += (id) => TabCloseButtonClicked?.Invoke(id);
                    tabItem.TabClicked += (id) => TabClicked?.Invoke(id);

                    return tabItem.GetInstance();
                }, supportsRecycling: true)
            };
            //由于绑定目标在修改时会替换掉原有引用 故此处直接赋值会导致后续追踪丢失 故选择绑定属性来实现追踪
            tabControl.DataContext = dataContext;
            tabControl.Bind(ItemsControl.ItemsSourceProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.TabDisplayContents)));

            Grid.SetColumn(root, 0);
            root.Children.Add(tabControl);
        }

        public Panel GetInstance()
        {
            return root;
        }

        //事件
        public delegate void TabEventHandler(int id);
        public event TabEventHandler? TabCloseButtonClicked;
        public event TabEventHandler? TabClicked;
    }
}
