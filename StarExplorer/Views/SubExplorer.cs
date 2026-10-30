using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using StarExplorer.Controls;
using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StarExplorer.Views
{
    internal class SubExplorer : IDisposable
    {
        //数据
        SubExplorerData dataContext;
        Panel root;

        Border CurrentDisplayContent = null!;
        TextBox addressBox = null!;

        bool isAddressEntered = false;  //address失焦时是否需要更新路径

        //事件
        public event Action? HomeButtonCliked;
        public event Action? UpButtonClicked;
        public event Action<AddressBoxEventArgs>? AddressBoxEvent;
        //本地委托
        EventHandler<Avalonia.Input.KeyEventArgs>? addressBoxKeyPressedHandler;
        EventHandler<FocusChangedEventArgs>? addressBoxLoseFocusedHandler;


        public SubExplorer(SubExplorerData dataContext)
        {
            this.dataContext = dataContext;
            InitHandler();

            root = BuildPanel();
        }

        private Panel BuildPanel()
        {
            Panel root = new Panel();
            //layout分格
            DockPanel layout = new DockPanel();

            //地址栏
            {
                Border adressBorder = new Border();
                adressBorder.Bind(Border.BackgroundProperty, new Binding(nameof(dataContext.ThemeColor)) { Source = dataContext, Converter = new StarExplorer.Controls.BrushConverter() });
                adressBorder.Bind(Border.CornerRadiusProperty, new Binding(nameof(dataContext.AdressBoxCornerRadius)) { Source = dataContext, Converter = new CornerRadiusConverter() });
                adressBorder.Bind(Border.HeightProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });

                //用于在地址栏左右侧放置按钮的Grid
                Grid adressBorderGrid = new Grid();
                ColumnDefinition homeButtonColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) };
                ColumnDefinition upButtonColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) };
                ColumnDefinition addressBoxColumn = new ColumnDefinition();
                adressBorderGrid.ColumnDefinitions.Add(homeButtonColumn);
                adressBorderGrid.ColumnDefinitions.Add(upButtonColumn);
                adressBorderGrid.ColumnDefinitions.Add(addressBoxColumn);

                Button homeButton = new Button();
                homeButton.Content = "H";
                homeButton.Bind(Button.WidthProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });
                homeButton.Bind(Button.HeightProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });
                homeButton.Margin = new Avalonia.Thickness(10, 0, 0, 0);

                Button upButton = new Button();
                upButton.Content = "↑";
                upButton.Bind(Button.WidthProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });
                upButton.Bind(Button.HeightProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });
                upButton.Margin = new Avalonia.Thickness(10, 0, 0, 0);

                addressBox = new TextBox();
                addressBox.Bind(TextBox.CornerRadiusProperty, new Binding(nameof(dataContext.AdressBoxCornerRadius)) { Source = dataContext, Converter = new CornerRadiusConverter() });
                addressBox.Margin = new Avalonia.Thickness(10, 0, 10, 0);
                //此处绑定使用单向 因为输入需要验证路径的合法性
                addressBox.Bind(TextBox.TextProperty, new Binding(nameof(dataContext.Path)) { Source = dataContext, Mode = BindingMode.OneWay });
                addressBox.IsInactiveSelectionHighlightEnabled = false;
                //潜在泄漏点:adressBox非正常被长生命周期的对象持有引用
                addressBox.KeyDown += addressBoxKeyPressedHandler;
                addressBox.LostFocus += addressBoxLoseFocusedHandler;

                Grid.SetColumn(homeButton, 0);
                Grid.SetColumn(upButton, 1);
                Grid.SetColumn(addressBox, 2);
                adressBorderGrid.Children.Add(homeButton);
                adressBorderGrid.Children.Add(upButton);
                adressBorderGrid.Children.Add(addressBox);

                adressBorder.Child = adressBorderGrid;
                DockPanel.SetDock(adressBorder, Dock.Top);
                layout.Children.Add(adressBorder);

                //地址栏事件绑定
                homeButton.Click += (s, e) =>
                {
                    this.HomeButtonCliked?.Invoke();
                };

                upButton.Click += (s, e) =>
                {
                    this.UpButtonClicked?.Invoke();
                };
            }

            //显示区分格
            Grid DisplayGrid = new Grid();
            ColumnDefinition sideBarColumn = new ColumnDefinition();
            sideBarColumn.Bind(ColumnDefinition.WidthProperty, new Binding("SideBarWidth") { Source = dataContext , Converter = new GridLengthConverter()});
            ColumnDefinition DisplayColumn = new ColumnDefinition();
            DisplayColumn.Width = new GridLength(1, GridUnitType.Star);

            DisplayGrid.ColumnDefinitions.Add(sideBarColumn);
            DisplayGrid.ColumnDefinitions.Add(DisplayColumn);
            //侧边栏
            Border sideBorder = new Border();
            //TODO:侧边栏颜色，现在为临时颜色
            sideBorder.Background = SolidColorBrush.Parse(Colors.Gray.ToString());

            Grid.SetColumn(sideBorder, 0);
            DisplayGrid.Children.Add(sideBorder);

            CurrentDisplayContent = new Border();
            Grid.SetColumn(CurrentDisplayContent, 1);
            DisplayGrid.Children.Add(CurrentDisplayContent);

            layout.Children.Add(DisplayGrid);

            root.Children.Add(layout);
            return root;
        }

        public Panel GetInstance()
        {
            return root;
        }

        //封装方法

        private void InitHandler()
        {
            addressBoxKeyPressedHandler = (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    TextBox? adressBox = s as TextBox;
                    if (adressBox == null) return;  //理论上这部分不可能触发
                    string enteredPath = adressBox.Text ?? "/";
                    //在上层逻辑中验证路径合法性,此处只触发事件
                    isAddressEntered = true;
                    //此时不触发设置地址 统一在失焦后触发

                    //清除焦点
                    var p = adressBox.GetVisualParent();
                    while (p != null)
                    {
                        if (p is Avalonia.Controls.Window)
                            break;
                        p = p.GetVisualParent();
                    }
                    if (p is not Avalonia.Controls.Window)
                        throw new Exception("Root window not found");
                    var rootWindow = p as Avalonia.Controls.Window;
                    rootWindow?.Focus();  //将焦点设置到窗口上,以触发失焦事件
                }
            };

            addressBoxLoseFocusedHandler = (s, e) =>
            {
                this.AddressBoxEvent?.Invoke(new AddressBoxEventArgs((s as TextBox)?.Text ?? "/", isAddressEntered));
                //重置状态
                this.isAddressEntered = false;
            };
        }

        internal void SetMainDisplayContent(Panel content)
        {
            Grid.SetRow(content, 1);
            CurrentDisplayContent.Child = content;
        }

        internal void ClearDisplayContent()
        {
            CurrentDisplayContent.Child = new Border();
        }

        public void Dispose()
        {
            //清理事件
            this.HomeButtonCliked = null;
            this.AddressBoxEvent = null;
            //解除事件绑定
            addressBox.LostFocus -= this.addressBoxLoseFocusedHandler;
            addressBox.KeyDown -= this.addressBoxKeyPressedHandler;
        }
    }

    class AddressBoxEventArgs : EventArgs
    {
        public string TargetPath { get; set; }
        public bool InputORDispose { get; set; }
        public AddressBoxEventArgs(string enteredPath, bool inputORDispose)
        {
            TargetPath = enteredPath;
            InputORDispose = inputORDispose;
        }
    }
}
