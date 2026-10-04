using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using StarExplorer.Shared;
using System;
using System.Threading;

namespace StarExplorer.Controls
{
    internal class FolderItemsControl
    {
        Panel root;
        //数据
        IItemsDataContext itemDataContext;
        //动画
        Animation pointerEnteredAnimation;
        Animation pointerExitedAnimation;
        CancellationTokenSource? pointerEnteredAnimationCancelTokenSource;
        CancellationTokenSource? pointerExitedAnimationCancelTokenSource;
        //事件
        public Action<int>? LeftClicked;
        public Action<int>? RightClicked;
        public Action<int>? DoubleClicked;



        public FolderItemsControl(ItemsPanelData dataContext,IItemsDataContext itemDataContext)
        {
            root = new Panel();
            this.itemDataContext = itemDataContext;
            //此颜色完全透明,只是为了捕获鼠标事件
            root.Background = new SolidColorBrush(Colors.LightGray);
            //内容显示列
            Grid grid = new Grid();
            //TODO:可调整的列数量和宽度
            ColumnDefinition iconColumn = new ColumnDefinition();
            iconColumn.Bind(ColumnDefinition.WidthProperty, new Binding(nameof(dataContext.FileItemHeight)) { Source = dataContext, Converter = new GridLengthConverter() });
            ColumnDefinition nameColumn = new ColumnDefinition();
            nameColumn.Bind(ColumnDefinition.WidthProperty, new Binding(nameof(dataContext.FileItemNameWidth)) { Source = dataContext, Converter = new GridLengthConverter() });
            ColumnDefinition ColumnDefinition3 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };

            grid.ColumnDefinitions.Add(iconColumn);
            grid.ColumnDefinitions.Add(nameColumn);
            grid.ColumnDefinitions.Add(ColumnDefinition3);

            //Logo
            Image icon = new Image();
            icon.Stretch = Avalonia.Media.Stretch.Fill;
            icon.Bind(Image.MarginProperty, new Binding(nameof(dataContext.FileItemInternalMargin)) { Source = dataContext, Converter = new ThicknessConverter() });
            if (itemDataContext.ItemToDisplay.Type == ItemType.Folder)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FolderIcon)) { Source = dataContext });
            else if (itemDataContext.ItemToDisplay.Type == ItemType.Folder_Link)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FolderLinkIcon)) { Source = dataContext });
            else if (itemDataContext.ItemToDisplay.Type == ItemType.File)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FileIcon)) { Source = dataContext });
            else if (itemDataContext.ItemToDisplay.Type == ItemType.SymbolicLink)
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FileIcon)) { Source = dataContext });
            else
                icon.Bind(Image.SourceProperty, new Binding(nameof(dataContext.FileIcon)) { Source = dataContext });

            Grid.SetColumn(icon, 0);
            grid.Children.Add(icon);

            //名称
            TextBlock name = new TextBlock();
            name.Bind(TextBlock.TextProperty, new Binding(nameof(itemDataContext.ItemToDisplay.Name)) { Source = itemDataContext.ItemToDisplay });
            name.Bind(TextBlock.MarginProperty, new Binding(nameof(dataContext.FileItemInternalMargin)) { Source = dataContext, Converter = new ThicknessConverter() });
            name.Bind(TextBlock.FontSizeProperty, new Binding(nameof(dataContext.FileItemFontSize)) { Source = dataContext });
            name.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            name.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            Grid.SetColumn(name, 1);
            grid.Children.Add(name);

            //修改时间
            TextBlock adjTime = new TextBlock();
            adjTime.Bind(TextBlock.TextProperty, new Binding(nameof(itemDataContext.ItemToDisplay.ModifiedTime)) { Source = itemDataContext.ItemToDisplay });
            adjTime.Bind(TextBlock.MarginProperty, new Binding(nameof(dataContext.FileItemInternalMargin)) { Source = dataContext, Converter = new ThicknessConverter() });
            adjTime.Bind(TextBlock.FontSizeProperty, new Binding(nameof(dataContext.FileItemFontSize)) { Source = dataContext });
            adjTime.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            adjTime.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            Grid.SetColumn(adjTime, 2);
            grid.Children.Add(adjTime);

            root.Children.Add(grid);

            //动画构建
            this.pointerEnteredAnimation = Animations.GetGradientAnimation(50, Colors.LightGray, dataContext.HoverColor);
            this.pointerExitedAnimation = Animations.GetGradientAnimation(50, dataContext.HoverColor, Colors.LightGray);

            //事件绑定
            this.root.PointerEntered += OnPointerEntered;
            this.root.PointerExited += OnPointerExited;
            this.root.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(root).Properties.IsLeftButtonPressed)
                {
                    LeftClicked?.Invoke(itemDataContext.ID);
                }
                else if (e.GetCurrentPoint(root).Properties.IsRightButtonPressed)
                {
                    RightClicked?.Invoke(itemDataContext.ID);
                }
            };
            this.root.DoubleTapped += (s, e) =>
            {
                DoubleClicked?.Invoke(itemDataContext.ID);
            };
            //外部修改IsSelected时自动变颜色
            itemDataContext.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(itemDataContext.IsSelected))
                {
                    OnSelectedStatusChanged(dataContext);
                }
            };
        }

        //封装方法
        private void StopAnimation()
        {
            this.pointerExitedAnimationCancelTokenSource?.Cancel();
            this.pointerExitedAnimationCancelTokenSource?.Dispose();
            this.pointerExitedAnimationCancelTokenSource = null;

            this.pointerEnteredAnimationCancelTokenSource?.Cancel();
            this.pointerEnteredAnimationCancelTokenSource?.Dispose();
            this.pointerEnteredAnimationCancelTokenSource = null;
        }

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            StopAnimation();
            if (itemDataContext.IsSelected) return;
            this.pointerEnteredAnimationCancelTokenSource = new CancellationTokenSource();
            this.pointerEnteredAnimation.RunAsync(root, this.pointerEnteredAnimationCancelTokenSource.Token);
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            StopAnimation();
            if (itemDataContext.IsSelected) return;
            this.pointerExitedAnimationCancelTokenSource = new CancellationTokenSource();
            this.pointerExitedAnimation.RunAsync(root, this.pointerExitedAnimationCancelTokenSource.Token);
        }

        private void OnSelectedStatusChanged(ItemsPanelData dataContext)
        {
            if (itemDataContext.IsSelected)
            {
                StopAnimation();
                root.Background = new SolidColorBrush(dataContext.SelectedColor);
            }
            else
            {
                root.Background = new SolidColorBrush(Colors.LightGray);
            }
        }

        internal int GetID()
        {
            return this.itemDataContext.ID;
        }

        public Panel GetInstance()
        {
            return root;
        }

    }
}
