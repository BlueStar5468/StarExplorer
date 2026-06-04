using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using StarExplorer.ViewModels;

namespace StarExplorer.Controls
{
    internal class TabPanel
    {
        ItemsControl root;
        public TabPanel(ExplorerData dataContext)
        {
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

                ItemTemplate = new FuncDataTemplate<ITabContent>((tabContent, _) =>
                {
                    Controls.TabControl tabItem = new Controls.TabControl(
                        Color.Parse(dataContext.tabItemBackgroundColor),
                        dataContext.TabItemWidth,
                        dataContext.TabHeight,
                        sideMargin: 3,
                        CornerRadius: 5,
                        imageSize: 16,
                        tabContent.Icon,
                        imageMargin: 5,
                        tabContent.Label,
                        closeButtonSize: 16,
                        id: tabContent.Index,
                        dataContext.HoverBackgroundColor,
                        tabContent
                    );
                    //标签页事件绑定
                    tabItem.TabClosed += dataContext.tabManager.CloseTab;
                    tabItem.TabClosed += dataContext.CheckExit;
                    tabItem.TabClicked += dataContext.tabManager.OnTabClicked;

                    return tabItem.GetInstance();
                }, supportsRecycling: true)
            };
            tabControl.ItemsSource = dataContext.tabManager.tabContents;

            Grid.SetColumn(tabControl, 0);

            root = tabControl;
        }

        public ItemsControl GetInstance()
        {
            return root;
        }
    }
}
