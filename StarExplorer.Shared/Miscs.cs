using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Shared
{
    public class LogicDevices : INotifyPropertyChanged
    {
        private string? name;
        private string? type;
        private string? label;
        private string? fileSystem;
        private long totalSize;
        private long availableSize;
        private long totalFreeSize;

        bool isReady; //注:是否准备就绪，指设备是否可以访问和使用

        public string? Name { get => name; set { name = value; OnPropertyChanged(nameof(Name)); OnPropertyChanged(nameof(Title)); } }
        public string? Type { get => type; set { type = value; OnPropertyChanged(nameof(Type)); } }
        public string? Label { get => label; set { label = value; OnPropertyChanged(nameof(Label)); OnPropertyChanged(nameof(Title)); } }
        public string? FileSystem { get => fileSystem; set { fileSystem = value; OnPropertyChanged(nameof(FileSystem)); } }
        public long TotalSize { get => totalSize; set { totalSize = value; OnPropertyChanged(nameof(TotalSize)); OnPropertyChanged((nameof(UsedSize))); } }
        public long AvailableSize { get => availableSize; set { availableSize = value; OnPropertyChanged(nameof(AvailableSize)); OnPropertyChanged((nameof(UsedSize))); } }
        public long TotalFreeSize { get => totalFreeSize; set { totalFreeSize = value; OnPropertyChanged(nameof(TotalFreeSize)); } }
        public long UsedSize { get => TotalSize - AvailableSize; }
        //注:总空闲空间和可用空间不相同
        public bool IsReady { get => isReady; set { isReady = value; OnPropertyChanged(nameof(IsReady)); } }
        public string? Title { get => GetTitle(); }


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
            this.IsReady = isReady;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //获取设备标题，格式为“(卷标)名称”，如果卷标不可用则使用“名称”作为标题
        private String GetTitle()
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




    //文件相关

    public class Item
    {
        private string name;
        private string path;
        private ItemType type;
        private string createdTime;
        private string modifiedTime;
        private string? linkTarget;

        //通用构造方式
        public Item(string name, string path, ItemType type, string createdTime, string modifiedTime, string? linkTarget)
        {
            this.name = name;
            this.path = path;
            this.type = type;
            this.createdTime = createdTime;
            this.modifiedTime = modifiedTime;

            this.linkTarget = linkTarget;
        }
        //封装属性
        public string Name { get => name; }
        public string Path { get => path; }
        public ItemType Type { get => type; }
        public string CreatedTime { get => createdTime; }
        public string ModifiedTime { get => modifiedTime; }
        public string LinkTarget { get => LinkTarget; set => LinkTarget = value; }
    }

    public enum ItemType
    {
        Folder,
        Folder_Link,
        File,
        SymbolicLink,
        hardLink,
    }


}
