using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ToastCanvas
    {
        Canvas root;
        ToastCanvasData data;
        //数据
        int maxToastCount;
        //ID分配器
        IDManager iDManager;
        //toast通知panel引用存储 注:此处索引越大的toast越在右下角 且list索引不会存在空洞
        List<ToastControl> toastControls = new List<ToastControl>();

        public ToastCanvas(ToastCanvasData data)
        {
            this.data = data;
            this.maxToastCount = data.MaxToastCount; //此处只是初始化
            //绑定maxToastCount变化事件
            this.data.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(data.MaxToastCount))
                {
                    this.maxToastCount = data.MaxToastCount;
                }
            };
            iDManager = new IDManager(maxToastCount);

            Canvas canvas = new Canvas()
            {
                Background = null,  //注:null背景,不拦截点击事件
                Name = "ToastCanvas",
                IsHitTestVisible = true,   //注:不要使用点击穿透,为继承属性 必须整条链路全部可用才能点击
                ZIndex = 1000,
            };
            root = canvas;
        }

        //封装方法

        internal void CreateToast(Message message)
        {
            if (iDManager.isFull())//如果达到最大通知数量 清除最老的通知
            {
                //最老的通知应该是索引最小的
                this.RemoveToast(toastControls[0].GetID());
            }

            var id = iDManager.GetID();
            //创建一个新的Toast实例
            ToastControl toast = new ToastControl(
                data,
                new Vector2d(-data.ToastWidth - 50,GetLayoutPosition(0).Y),    //注:此处为了实现右下角锚定,使用的是Right-Bottom坐标系
                new Vector2d(GetLayoutPosition(0).X,GetLayoutPosition(0).Y),
                message, id);
            //将Toast添加到Canvas中
            Canvas.SetRight(toast.GetInstance(), -data.ToastWidth - 50);
            Canvas.SetBottom(toast.GetInstance(), 20);
            //如果当前已有Toast,则将其移动到新的位置
            if (this.toastControls.Count > 0)
            {
                for (int i = 0; i < this.toastControls.Count; i++)
                {
                    var newPositon = GetLayoutPosition(this.toastControls.Count - 1 - i + 1);//+1是因为新toast已经占据了一个位置
                    this.toastControls[i].MoveTo(newPositon);
                }
            }
            toastControls.Add(toast);
            root.Children.Add(toast.GetInstance());
            //事件绑定
            toast.ToastClosed += RemoveToast;
        }

        private void RemoveToast(int id)
        {
            //从Canvas中移除Toast
            var toastToRemove = GetInstanceByID(id);
            if (toastToRemove == null) return;
            //由于List里index越大的在越下面 所以移除后,需要将index小于被移除的toast的所有toast向下移动
            //注意此时要移除的还没有从List中移除 所以要移除项的index还没有被新的index覆盖
            for (int i = 0; i < toastControls.IndexOf(toastToRemove); i++)
            {
                var newPositon = GetLayoutPosition(toastControls.Count - 1 - i - 1);//-1是因为被移除的toast已经占据了一个位置
                toastControls[i].MoveTo(newPositon);
            }
            toastControls.Remove(toastToRemove);
            root.Children.Remove(toastToRemove.GetInstance());
            //释放ID
            iDManager.RecycleID(id);
        }

        private Vector2d GetLayoutPosition(int index)
        {
            //注:此处使用Right-Bottom坐标系
            //注:此布局使用的index是从底部向上数,与索引List的索引相反,即索引越大,位置越靠下
            double x_right = 20; //在正常显示时,toast距离右边界的距离 临时硬编码
            double y_bottom = 20 + index * (data.ToastHeight + data.ToastSpacing); //在正常显示时,toast距离下边界的距离 临时硬编码
            return new Vector2d(x_right, y_bottom);
        }

        private ToastControl? GetInstanceByID(int id)
        {
            return toastControls.FirstOrDefault(t => t.GetID() == id);
        }

        internal Canvas GetInstance()
        {
            return root;
        }
    }
}
