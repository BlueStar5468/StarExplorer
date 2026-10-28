using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using StarExplorer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer
{
    internal static class Utils
    {
        public static IImage CreateImageFormRGBA(byte[] pixels, int width, int height)
        {
            int expectedLength = width * height * 4;

            if (pixels.Length != expectedLength)
            {
                throw new ArgumentException(
                    $"Invalid RGBA data length. " +
                    $"Expected: {expectedLength}, " +
                    $"Actual: {pixels.Length}."
                    );
            }

            var bitmap = new WriteableBitmap(
                new Avalonia.PixelSize(width, height),
                new Avalonia.Vector(96, 96),    //dpi
                Avalonia.Platform.PixelFormat.Rgba8888,
                Avalonia.Platform.AlphaFormat.Unpremul  //alpha混合已经在后端数据中处理
                );

            using ILockedFramebuffer framebuffer = bitmap.Lock();

            int sourceStride = width * 4;
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(
                    pixels,
                    y * sourceStride,
                    IntPtr.Add(framebuffer.Address, y * framebuffer.RowBytes),
                    sourceStride);
            }

            return bitmap;
        }


    }
}
