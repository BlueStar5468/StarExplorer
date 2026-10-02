using Avalonia.Controls;
using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ToastCanvasController : IToastCanvasController
    {
        //依赖项   
        ISettings settings;
        ICoreData coreData;
        //管理的窗口
        ToastCanvas toastCanvas = null!;
        ToastCanvasData data = null!;

        public ToastCanvasController(ISettings settings, ICoreData coreData)
        {
            this.settings = settings;
            this.coreData = coreData;

            CreateWindow();
            BindEvents();
        }

        private void CreateWindow()
        {
            //创建ToastCanvasData和ToastCanvas实例
            data = new ToastCanvasData(settings);
            toastCanvas = new ToastCanvas(data);
        }

        private void BindEvents()
        {
            //在此处绑定ToastCanvas相关事件
        }

        //外部接口
        public Canvas GetInstance()
        {
            //返回ToastCanvas实例
            return this.toastCanvas.GetInstance();
        }

        public void ShowToast(string label, string message)
        {
            //创建一个新的Toast实例并显示
            toastCanvas.CreateToast(new Message { label = label, message = message });
        }
    }

    public interface IToastCanvasController
    {
        void ShowToast(string label, string message);
    }
}
