using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ItemsPanelData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        //依赖项

        //数据


        public ItemsPanelData(ISettings settings)
        {
            InitSettings(settings);
            BindSettings(settings);
        }

        private void InitSettings(ISettings settings)
        {

        }
        private void BindSettings(ISettings settings)
        {

        }






        //封装方法
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
