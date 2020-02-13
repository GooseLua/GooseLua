using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GooseLua {
    class Utils {
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        public static Bitmap Screenshot() {
            Screen Display = Screen.PrimaryScreen;
            Bitmap b = new Bitmap(Display.Bounds.Width, Display.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(Display.Bounds.Left, Display.Bounds.Top, 0, 0, Display.Bounds.Size);

            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            try {
                if (GetCursorInfo(out pci)) {
                    if (pci.flags == CURSOR_SHOWING) {
                        DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                        g.ReleaseHdc();
                    }
                }
            } catch {
                g.DrawString("^", new Font("Courier New", 12f), Brushes.White, new PointF(Cursor.Position.X, Cursor.Position.Y));
            }
            return b;
        }

        public static T Shift<T>(T[] arr) {
            T o = arr[0];
            Array.Copy(arr, 1, arr, 0, arr.Length - 1);
            Array.Clear(arr, arr.Length - 1, 1);
            return o;
        }
    }
}
