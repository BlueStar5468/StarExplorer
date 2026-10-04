using Avalonia.Media;
using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ToastCanvasData : INotifyPropertyChanged
    {
        private int maxToastCount;
        private int toastDurationMS; //toast显示时间
        private int toastSpacing; //toast的上下间距
        private int toastWidth;
        private int toastHeight;
        private int toastCornerRadius;
        private int toastTimeMS; //toast显示时间
        private bool toastAutoHeight;
        private Color toastBackgroundColor;
        private Color toastBorderColor;
        private Color toastLabelPanelColor;

        public int MaxToastCount { get => maxToastCount; set { if (maxToastCount != value) { maxToastCount = value; OnPropertyChanged(nameof(MaxToastCount)); } } }
        public int ToastDurationMS { get => toastDurationMS; set { if (toastDurationMS != value) { toastDurationMS = value; OnPropertyChanged(nameof(ToastDurationMS)); } } }
        public int ToastSpacing { get => toastSpacing; set { if (toastSpacing != value) { toastSpacing = value; OnPropertyChanged(nameof(ToastSpacing)); } } }
        public int ToastWidth { get => toastWidth; set { if (toastWidth != value) { toastWidth = value; OnPropertyChanged(nameof(ToastWidth)); } } }
        public int ToastHeight { get => toastHeight; set { if (toastHeight != value) { toastHeight = value; OnPropertyChanged(nameof(ToastHeight)); } } }
        public int ToastCornerRadius { get => toastCornerRadius; set { if (toastCornerRadius != value) { toastCornerRadius = value; OnPropertyChanged(nameof(ToastCornerRadius)); } } }
        public int ToastTimeMS { get => toastTimeMS; set { if (toastTimeMS != value) { toastTimeMS = value; OnPropertyChanged(nameof(ToastTimeMS)); } } }
        public bool ToastAutoHeight { get => toastAutoHeight; set { if (toastAutoHeight != value) { toastAutoHeight = value; OnPropertyChanged(nameof(ToastAutoHeight)); } } }  
        public Color ToastBackgroundColor { get => toastBackgroundColor; set { if (toastBackgroundColor != value) { toastBackgroundColor = value; OnPropertyChanged(nameof(ToastBackgroundColor)); } } }
        public Color ToastBorderColor { get => toastBorderColor; set { if (toastBorderColor != value) { toastBorderColor = value; OnPropertyChanged(nameof(ToastBorderColor)); } } }
        public Color ToastLabelPanelColor { get => toastLabelPanelColor; set { if (toastLabelPanelColor != value) { toastLabelPanelColor = value; OnPropertyChanged(nameof(ToastLabelPanelColor)); } } }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ToastCanvasData(ISettings settings)
        {
            //初始化数据
            InitSettings(settings);
            BindSettings(settings);
        }

        //封装方法
        private void InitSettings(ISettings settings)
        {
            MaxToastCount = settings.MaxToastCount;
            ToastDurationMS = settings.ToastDurationMS;
            ToastSpacing = settings.ToastSpacing;
            ToastWidth = settings.ToastWidth;
            ToastHeight = settings.ToastHeight;
            ToastAutoHeight = settings.ToastAutoHeight;
            ToastCornerRadius = settings.ToastCornerRadius;
            ToastBackgroundColor = settings.ToastBackGroundColor_Color;
            ToastBorderColor = settings.ToastBorderColor_Color;
            ToastLabelPanelColor = settings.ToastLabelPanelColor_Color;
            ToastTimeMS = settings.ToastTimeMS;
        }

        private void BindSettings(ISettings settings)
        {
            settings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.MaxToastCount):
                        MaxToastCount = settings.MaxToastCount;
                        break;
                    case nameof(settings.ToastDurationMS):
                        ToastDurationMS = settings.ToastDurationMS;
                        break;
                    case nameof(settings.ToastSpacing):
                        ToastSpacing = settings.ToastSpacing;
                        break;
                    case nameof(settings.ToastWidth):
                        ToastWidth = settings.ToastWidth;
                        break;
                    case nameof(settings.ToastHeight):
                        ToastHeight = settings.ToastHeight;
                        break;
                    case nameof(settings.ToastAutoHeight):
                        ToastAutoHeight = settings.ToastAutoHeight;
                        break;
                    case nameof(settings.ToastCornerRadius):
                        ToastCornerRadius = settings.ToastCornerRadius;
                        break;
                    case nameof(settings.ToastBackGroundColor_Color):
                        ToastBackgroundColor = settings.ToastBackGroundColor_Color;
                        break;
                    case nameof(settings.ToastBorderColor_Color):
                        ToastBorderColor = settings.ToastBorderColor_Color;
                        break;
                    case nameof(settings.ToastLabelPanelColor_Color):
                        ToastLabelPanelColor = settings.ToastLabelPanelColor_Color;
                        break;
                    case nameof(settings.ToastTimeMS):
                        ToastTimeMS = settings.ToastTimeMS;
                        break;
                }
            };
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
