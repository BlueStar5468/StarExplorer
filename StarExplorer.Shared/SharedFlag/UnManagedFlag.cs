using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Shared.SharedFlag
{
    public static class UnManagedFlag
    {
        public enum SHGFI : uint
        {
            SHGFI_ICON = 0x000000100,     // 获取图标
            SHGFI_LARGEICON = 0x000000000, // 获取大图标 必须与SHGFI_ICON一起使用
            SHGFI_SMALLICON = 0x000000001, // 获取小图标 必须与SHGFI_ICON一起使用
            SHGFI_SYSICONINDEX = 0x000004000, // 获取系统图标索引 并存入SHFILEINFO结构体的iIcon字段中 需要与SHGFI_ICON一起使用
            SHGFI_TYPENAME = 0x000000400, // 获取文件类型名称 并存入SHFILEINFO结构体的szTypeName字段中
            SHGFI_DISPLAYNAME = 0x000000200, // 获取文件显示名称 并存入SHFILEINFO结构体的szDisplayName字段中
            SHGFI_USEFILEATTRIBUTES = 0x000000010, // 使用文件属性直接推断文件类型，而不访问磁盘上的文件
                                                   //需要结合dwFileAttributes参数使用
            SHGFI_LINKOVERLAY = 0x000008000, // 为快捷方式添加覆盖图标
            SHGFI_ADDOVERLAYS = 0x000000020, // 为图标添加覆盖图标
        }

        public enum FileAttributes : uint
        {
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010, // 文件夹
            FILE_ATTRIBUTE_NORMAL = 0x00000080,    // 普通文件
            FILE_ATTRIBUTE_READONLY = 0x00000001,  // 只读文件
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,    // 隐藏文件
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,    // 系统文件
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,   // 存档文件
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100, // 临时文件
        }
    }
}
