using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
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
    internal class SubExplorer
    {
        //数据
        SubExplorerData dataContext;
        Panel root;

        Border CurrentDisplayContent;

        //事件
        public Action? HomeButtonCliked;

        public SubExplorer(SubExplorerData dataContext)
        {
            this.dataContext = dataContext;
            root = BuildPanel();
        }

        private Panel BuildPanel()
        {
            Panel root = new Panel();
            //分格
            Grid grid = new Grid();
            ColumnDefinition columnDefinition0 = new ColumnDefinition();
            columnDefinition0.Bind(ColumnDefinition.WidthProperty, new Binding("SideBarWidth") { Source = dataContext , Converter = new GridLengthConverter()});
            ColumnDefinition columnDefinition1 = new ColumnDefinition();
            columnDefinition1.Width = new GridLength(1, GridUnitType.Star);

            grid.ColumnDefinitions.Add(columnDefinition0);
            grid.ColumnDefinitions.Add(columnDefinition1);
            //侧边栏
            Border sideBorder = new Border();
            //TODO:侧边栏颜色，现在为临时颜色
            sideBorder.Background = SolidColorBrush.Parse(Colors.Gray.ToString());

            Grid.SetColumn(sideBorder, 0);
            grid.Children.Add(sideBorder);
            //主显示区
            Grid MainDisplayGrid = new Grid();
            RowDefinition row0 = new RowDefinition();
            RowDefinition row1 = new RowDefinition();
            row0.Bind(RowDefinition.HeightProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext, Converter = new GridLengthConverter() });
            MainDisplayGrid.RowDefinitions.Add(row0);
            MainDisplayGrid.RowDefinitions.Add(row1);

            //地址栏
            {
                Border adressBorder = new Border();
                adressBorder.Bind(Border.BackgroundProperty, new Binding(nameof(dataContext.ThemeColor)) { Source = dataContext, Converter = new StarExplorer.Controls.BrushConverter() });
                adressBorder.Bind(Border.CornerRadiusProperty, new Binding(nameof(dataContext.AdressBoxCornerRadius)) { Source = dataContext , Converter = new CornerRadiusConverter()});

                //用于在地址栏左右侧放置按钮的Grid
                Grid adressBorderGrid = new Grid();
                ColumnDefinition adressBorderGridColumn0 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Auto) };
                ColumnDefinition adressBorderGridColumn1 = new ColumnDefinition();
                adressBorderGrid.ColumnDefinitions.Add(adressBorderGridColumn0);
                adressBorderGrid.ColumnDefinitions.Add(adressBorderGridColumn1);
                 
                
                Button homeButton = new Button();
                homeButton.Content = "H";
                homeButton.Bind(Button.WidthProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });
                homeButton.Bind(Button.HeightProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext });
                homeButton.Margin = new Avalonia.Thickness(10, 0, 0, 0);

                TextBox adressBox = new TextBox();
                adressBox.Bind(TextBox.CornerRadiusProperty, new Binding(nameof(dataContext.AdressBoxCornerRadius)) { Source = dataContext , Converter = new CornerRadiusConverter()});
                adressBox.Margin = new Avalonia.Thickness(10, 0, 10, 0);
                //TODO:绑定地址内容至核心

                Grid.SetColumn(adressBox, 1);
                Grid.SetColumn(homeButton, 0);
                adressBorderGrid.Children.Add(homeButton);
                adressBorderGrid.Children.Add(adressBox);

                adressBorder.Child = adressBorderGrid;
                Grid.SetRow(adressBorder, 0);
                MainDisplayGrid.Children.Add(adressBorder);

                //地址栏事件绑定
                homeButton.Click += (s, e) =>
                {
                    this.HomeButtonCliked?.Invoke();
                };
            }

            CurrentDisplayContent = new Border();
            Grid.SetRow(CurrentDisplayContent, 1);
            MainDisplayGrid.Children.Add(CurrentDisplayContent);

            Grid.SetColumn(MainDisplayGrid, 1);
            grid.Children.Add(MainDisplayGrid);

            root.Children.Add(grid);
            return root;
        }

        public Panel GetInstance()
        {
            return root;
        }

        //封装方法

        internal void SetMainDisplayContent(Panel content)
        {
            Grid.SetRow(content, 1);
            CurrentDisplayContent.Child = content;
        }

        internal void ClearDisplayContent()
        {
            CurrentDisplayContent.Child = new Border();
        }
    }
}
