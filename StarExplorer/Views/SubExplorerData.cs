using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Views
{
    internal class SubExplorerData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        //依赖项
        internal ISettings settings;

        //当前子资源管理器的路径 仅由子资源管理器维护 修改请求全部集中于此
        string path = null!;    
        //数据
        int sideBarWidth;
        int adressBoxHeight;
        int adressBoxCornerRadius;
        Color themeColor;


        public SubExplorerData(ISettings settings) 
        {
            this.settings = settings;

            InitSettings(settings);
            BindSettings(settings);
        }

        //封装属性
        public int SideBarWidth { get => sideBarWidth; set { if (sideBarWidth != value) { sideBarWidth = value; OnPropertyChanged(nameof(SideBarWidth)); } } }
        public int AdressBoxHeight { get => adressBoxHeight; set { if (adressBoxHeight != value) { adressBoxHeight = value; OnPropertyChanged(nameof(AdressBoxHeight)); } } }
        public int AdressBoxCornerRadius { get => adressBoxCornerRadius; set { if (adressBoxCornerRadius != value) { adressBoxCornerRadius = value; OnPropertyChanged(nameof(AdressBoxCornerRadius)); } } }
        public string Path { get => path; }
        public Color ThemeColor { get => themeColor; set { if (themeColor != value) { themeColor = value; OnPropertyChanged(nameof(ThemeColor)); } } }
        



        //封装方法
        private void InitSettings(ISettings settings)
        {
            sideBarWidth = settings.SideBarWidth;
            adressBoxHeight = settings.AdressBoxHeight;
            themeColor = settings.ThemeColor_Color;
            adressBoxCornerRadius = settings.AdressBoxCornerRadius;
        }
        private void BindSettings(ISettings settings)
        {
            settings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.SideBarWidth):
                        sideBarWidth = settings.SideBarWidth;
                        break;
                    case nameof(settings.AdressBoxHeight):
                        adressBoxHeight = settings.AdressBoxHeight;
                        break;
                    case nameof(settings.ThemeColor_Color):
                        themeColor = settings.ThemeColor_Color;
                        break;
                    case nameof(settings.AdressBoxCornerRadius):
                        adressBoxCornerRadius = settings.AdressBoxCornerRadius;
                        break;
                }
            };
        }

        internal void SetPath(string path)
        {
            this.path = path;
            OnPropertyChanged(nameof(Path));
        }

        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
