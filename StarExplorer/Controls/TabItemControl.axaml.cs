using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using StarExplorer.ViewModels;
using StarExplorer.Views;
using System;
using System.Threading;

namespace StarExplorer.Controls;

public partial class TabItemControl : UserControl , IDisposable
{
    CancellationTokenSource? pointerEnteredAnimationCacellationTokenSource;
    CancellationTokenSource? pointerExitedAnimationCacellationTokenSource;
    Animation? pointerEnteredAnimation;
    Animation? pointerExitedAnimation;
    public TabItemControl()
    {
        InitializeComponent();

        //事件绑定
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
    }

    //用于获取Color的方法(向上查找)
    private Color GetHoverColor()
    {
        var parent = this.GetVisualParent();
        while (parent != null)
        {
            if (parent is Explorer explorer && explorer.DataContext != null)
            {
                return Color.Parse(((ExplorerData)explorer.DataContext).HoverBackgroundColor);
            }
            parent = parent.GetVisualParent();
        }
        return Colors.Transparent; //如果未找到TabBarControl，则返回默认颜色
    }

    private Color GetBackgroundColor()
    {
        var parent = this.GetVisualParent();
        while (parent != null)
        {
            if (parent is Explorer explorer && explorer.DataContext != null)
            {
                return Color.Parse(((ExplorerData)explorer.DataContext).tabItemBackgroundColor);
            }
            parent = parent.GetVisualParent();
        }
        return Colors.Transparent; //如果未找到TabBarControl，则返回默认颜色
    }

    //事件处理方法
    private void OnPointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        pointerExitedAnimationCacellationTokenSource?.Cancel();
        pointerExitedAnimationCacellationTokenSource?.Dispose();
        pointerExitedAnimationCacellationTokenSource = null;

        pointerEnteredAnimationCacellationTokenSource?.Cancel();
        pointerEnteredAnimationCacellationTokenSource?.Dispose();
        pointerEnteredAnimationCacellationTokenSource = new CancellationTokenSource();

        try
        {
            pointerEnteredAnimation?.RunAsync(border, pointerEnteredAnimationCacellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            //动画被取消时会抛出此异常，捕获后无需处理
        }
    }

    private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        pointerEnteredAnimationCacellationTokenSource?.Cancel();
        pointerEnteredAnimationCacellationTokenSource?.Dispose();
        pointerEnteredAnimationCacellationTokenSource = null;

        pointerExitedAnimationCacellationTokenSource?.Cancel();
        pointerExitedAnimationCacellationTokenSource?.Dispose();
        pointerExitedAnimationCacellationTokenSource = new CancellationTokenSource();

        try
        {
            pointerExitedAnimation?.RunAsync(border, pointerExitedAnimationCacellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            //动画被取消时会抛出此异常，捕获后无需处理
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        //加载动画
        pointerEnteredAnimation = Animations.GetGradientAnimation(200, GetBackgroundColor(), GetHoverColor());
        pointerExitedAnimation = Animations.GetGradientAnimation(200, GetHoverColor(), GetBackgroundColor());
    }

    private void OnCloseButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (e.Handled == true) return;
        if (((ITabContent?)DataContext) == null) return;
        TabClosed?.Invoke(((ITabContent)DataContext).Index);
        e.Handled = true;
    }

    private void OnTabClicked(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled == true) return;
        if (e.Properties.IsLeftButtonPressed == true)
        {
            if (((ITabContent?)DataContext) == null) return;
            TabClicked?.Invoke(((ITabContent)DataContext).Index);
            e.Handled = true;
        }
    }

    //事件
    public delegate void TabClosedEventHandler(int tabId);
    public delegate void TabClickedEventHandler(int tabId);
    public event TabClosedEventHandler? TabClosed;
    public event TabClickedEventHandler? TabClicked;

    public void Dispose()
    {
        pointerEnteredAnimationCacellationTokenSource?.Cancel();
        pointerEnteredAnimationCacellationTokenSource?.Dispose();
        pointerEnteredAnimationCacellationTokenSource = null;

        pointerExitedAnimationCacellationTokenSource?.Cancel();
        pointerExitedAnimationCacellationTokenSource?.Dispose();
        pointerExitedAnimationCacellationTokenSource = null;
    }
}