using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using StarExplorer.Controls;
using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Views
{
    internal class SubExplorer
    {
        //数据
        SubExplorerData dataContext;
        Panel root;

        public SubExplorer(SubExplorerData dataContext, StartLocation startLocation, ICoreData coreData)
        {
            this.dataContext = dataContext;
            root = BuildPanel(startLocation, coreData);
        }

        private Panel BuildPanel(StartLocation startLocation, ICoreData coreData)
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
            Grid grid1 = new Grid();
            RowDefinition row0 = new RowDefinition();
            RowDefinition row1 = new RowDefinition();
            row0.Bind(RowDefinition.HeightProperty, new Binding(nameof(dataContext.AdressBoxHeight)) { Source = dataContext, Converter = new GridLengthConverter() });
            grid1.RowDefinitions.Add(row0);
            grid1.RowDefinitions.Add(row1);

            //地址栏
            {
                Border adressBorder = new Border();
                adressBorder.Bind(Border.BackgroundProperty, new Binding(nameof(dataContext.ThemeColor)) { Source = dataContext, Converter = new StarExplorer.Controls.BrushConverter() });
                adressBorder.Bind(Border.CornerRadiusProperty, new Binding(nameof(dataContext.AdressBoxCornerRadius)) { Source = dataContext , Converter = new CornerRadiusConverter()});
                
                TextBox adressBox = new TextBox();
                adressBox.Bind(TextBox.CornerRadiusProperty, new Binding(nameof(dataContext.AdressBoxCornerRadius)) { Source = dataContext , Converter = new CornerRadiusConverter()});
                adressBox.Margin = new Avalonia.Thickness(10, 0, 10, 0);
                //TODO:绑定地址内容至核心

                adressBorder.Child = adressBox;
                Grid.SetRow(adressBorder, 0);
                grid1.Children.Add(adressBorder);
            }

            var mainDisplay = GetDefaltDisplay(startLocation, dataContext.settings, coreData);
            Grid.SetRow(mainDisplay, 1);
            grid1.Children.Add(mainDisplay);

            Grid.SetColumn(grid1, 1);
            grid.Children.Add(grid1);

            root.Children.Add(grid);
            return root;
        }

        public Panel GetInstance()
        {
            return root;
        }

        //封装方法
        private Panel GetDefaltDisplay(StartLocation startLocation, ISettings settings, ICoreData coreData)
        {
            //获取标签页的初始内容
            if (startLocation == StartLocation.Devices)
            {
                var panel = CreateDevicePanel(settings, coreData);
                return panel;
            }
            else
            {
                //TODO: 根据其他起始位置生成相应的显示内容，目前仅实现了设备显示的生成逻辑
                return new StackPanel();
            }
        }

        private Panel CreateDevicePanel(ISettings settings, ICoreData coreData)
        {
            DevicePanelController devicePanelController = new DevicePanelController(settings, coreData);

            return devicePanelController.GetInstance();
        }
    }
}
