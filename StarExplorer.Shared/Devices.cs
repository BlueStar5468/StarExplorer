using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Shared
{
    public class LogicDevices
    {
        public string Name;
        public string Type;
        public string Label;
        public string FileSystem;
        public long TotalSize;
        public long AvailableSize;
        public long TotalFreeSize; //注:总空闲空间和可用空间不相同
        public string Title { get => GetTitle(); }

        bool isReady; //注:是否准备就绪，指设备是否可以访问和使用

        /// <summary>
        /// 初始化一个逻辑设备对象，包含设备名称、类型、标签、文件系统类型、总大小、可用大小和总空闲大小等属性。
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="type">类型</param>
        /// <param name="label">卷标</param>
        /// <param name="fileSystem">文件系统</param>
        /// <param name="totalSize">总大小 单位Byte</param>
        /// <param name="availableSize">可用大小 单位Byte</param>
        /// <param name="totalFreeSize">总可用大小 单位Byte</param>
        public LogicDevices(string name, string type, string label, string fileSystem, long totalSize, long availableSize, long totalFreeSize, bool isReady)
        {
            Name = name;
            Type = type;
            Label = label;
            FileSystem = fileSystem;
            TotalSize = totalSize;
            AvailableSize = availableSize;
            TotalFreeSize = totalFreeSize;
            this.isReady = isReady;
        }

        //获取设备标题，格式为“(卷标)名称”，如果卷标不可用则使用“名称”作为标题
        public String GetTitle()
        {
            String title;
            if (Label != null && Name != null)
            {
                title = $"({this.Name}) {this.Label}";
            }
            else 
            {
                title = this.Name ?? "未知设备";
            }

            return title;
        }
    }

    public class physicsDevices
    {
        

    }
}
