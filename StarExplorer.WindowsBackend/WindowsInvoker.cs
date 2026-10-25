using System.Runtime.InteropServices;
using StarExplorer.Shared;

namespace StarExplorer.WindowsBackend
{
    internal static class WindowsInvoker
    {
        //获取文件属性和图标的相关结构体和方法
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] //对于接收来自C++的结构体，使用Sequential布局是必要的，以确保字段按声明顺序排列
        public struct SHFILEINFO
        {
            public IntPtr hIcon;    //注意此句柄需要释放
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        //GDI底层调用 用于在Windows转换图片句柄
        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool fIcon;      // TRUE 表示图标，FALSE 表示光标
            public uint xHotspot;   // 光标热点的 X 坐标（图标时忽略）
            public uint yHotspot;   // 光标热点的 Y 坐标（图标时忽略）
            public IntPtr hbmMask;  // 掩码位图句柄 (HBITMAP)
            public IntPtr hbmColor; // 颜色位图句柄 (HBITMAP)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader; // 位图头信息
            public RGBQUAD bmiColors;          // 颜色表（此处简化为单个，实际需按需处理）
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize; //结构体大小 设置为(uint)Marshal.SizeOf<BITMAPINFOHEADER>()
            public int biWidth; //bitmap的宽度（以像素为单位）
            public int biHeight;//bitmap的高度（以像素为单位）
            public ushort biPlanes; //必须设置为1
            public ushort biBitCount;//每个像素的位数（1, 4, 8, 16, 24, 或 32） 使用RGBA需要为32
            public uint biCompression;//压缩类型（0表示不压缩，1表示RLE8，2表示RLE4，3表示比特字段，4表示JPEG，5表示PNG）
            public uint biSizeImage;//图像大小（以字节为单位） 如果biCompression为0，则可以设置为0
            public int biXPelsPerMeter; //水平分辨率（像素/米） 设为0忽略
            public int biYPelsPerMeter; //垂直分辨率（像素/米） 设为0忽略
            public uint biClrUsed;  //使用的颜色数（如果为0，则表示使用所有颜色）
            public uint biClrImportant;//重要颜色数（如果为0，则表示所有颜色都重要）
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            //在32bit色彩中 此结构体无效
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved; // 保留，通常为 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;   // x64 下 8 字节，注意别写成 int
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, ref BITMAP lpvObject);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines, byte[] lpvBits, ref BITMAPINFO lpbmi, uint usage);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);
    }
}
