using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoPC
{
	public class WindowsFunctions
	{
		[DllImport("user32.dll")]
		public static extern IntPtr CloseClipboard();
		[DllImport("user32.dll")]
		public static extern void GetCursorPos(out Coordinate coords); // Windows function for finding the cursor's coords
		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow(); // Windows function for getting an instance of the desktop
		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowDC(IntPtr desktop); // Windows function for getting the "Device Context" for a window. Aka taskbar and title bars, etc.
		[DllImport("user32.dll")]
		public static extern IntPtr ReleaseDC(IntPtr desktop, IntPtr dc); // Windows function for releasing a DC
		[DllImport("gdi32.dll")] // This is the dll that handles all screen painting. We can use it to detect colors on screen. https://www.pinvoke.net/default.aspx/gdi32/GetPixel.html
		public static extern int GetPixel(IntPtr dc, int x, int y); // Windows function for getting pixel at a specified screen coordinate.
		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int x, int y);
		[DllImport("user32.dll")]
		public static extern void mouse_event(uint flags, uint x, uint y, uint data, uint extraInfo);
	}
}
