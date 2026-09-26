using Avalonia.Controls;
using Avalonia.Data;
using StarExplorer.Shared;

namespace StarExplorer.Controls
{
    internal class FolderItemsControl
    {
        Panel root;
        public FolderItemsControl(ItemsPanelData dataContext, Item displayItem)
        {
            root = new Panel();
            //内容显示列
            Grid grid = new Grid();
            //TODO:可调整的列数量和宽度
            ColumnDefinition iconColumn = new ColumnDefinition() { Width = new GridLength(dataContext.FileItemHeight, GridUnitType.Pixel) };
            ColumnDefinition nameColumn = new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Pixel) };
            ColumnDefinition ColumnDefinition3 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };

            grid.ColumnDefinitions.Add(iconColumn);
            grid.ColumnDefinitions.Add(nameColumn);
            grid.ColumnDefinitions.Add(ColumnDefinition3);

            //Logo
            Image icon = new Image();
            icon.Stretch = Avalonia.Media.Stretch.Fill;
            icon.Bind(Image.MarginProperty, new Binding(nameof(dataContext.FileItemInternalMargin)) { Source = dataContext, Converter = new ThicknessConverter() });
            if (displayItem.Type == ItemType.Folder)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FolderIcon)) { Source = dataContext });
            else if (displayItem.Type == ItemType.Folder_Link)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FolderLinkIcon)) { Source = dataContext });
            else if (displayItem.Type == ItemType.File)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FileIcon)) { Source = dataContext });
            else if (displayItem.Type == ItemType.SymbolicLink)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FileIcon)) { Source = dataContext });
            else
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FileIcon)) { Source = dataContext });

            Grid.SetColumn(icon, 0);
            grid.Children.Add(icon);

            //名称
            TextBlock name = new TextBlock();
            name.Bind(TextBlock.TextProperty, new Binding(nameof(displayItem.Name)) { Source = displayItem });
            name.Bind(TextBlock.MarginProperty, new Binding(nameof(dataContext.FileItemInternalMargin)) { Source = dataContext, Converter = new ThicknessConverter() });
            name.Bind(TextBlock.FontSizeProperty, new Binding(nameof(dataContext.FileItemFontSize)) { Source = dataContext });
            name.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            name.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            Grid.SetColumn(name, 1);
            grid.Children.Add(name);

            //修改时间
            TextBlock adjTime = new TextBlock();
            adjTime.Bind(TextBlock.TextProperty, new Binding(nameof(displayItem.ModifiedTime)) { Source = displayItem });
            adjTime.Bind(TextBlock.MarginProperty, new Binding(nameof(dataContext.FileItemInternalMargin)) { Source = dataContext, Converter = new ThicknessConverter() });
            adjTime.Bind(TextBlock.FontSizeProperty, new Binding(nameof(dataContext.FileItemFontSize)) { Source = dataContext });
            adjTime.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            adjTime.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            Grid.SetColumn(adjTime, 2);
            grid.Children.Add(adjTime);

            root.Children.Add(grid);

        }

        public Panel GetInstance()
        {
            return root;
        }

    }
}
