using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace.Interop
{
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System;
	using System.Text;


	




	/// &lt;summary>
	/// EnumDesktopWindows Demo - shows the caption of all desktop windows.
	/// Authors: Svetlin Nakov, Martin Kulov 
	/// Bulgarian Association of Software Developers - http://www.devbg.org/en/
	/// &lt;/summary>
	public class user32
	{
		private delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);

		[DllImport("user32.dll")]
		static extern bool EnumDesktopWindows(IntPtr hDesktop,
		   EnumDesktopWindowsDelegate lpfn, IntPtr lParam);
		/// <summary>
		/// filter function
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

		/// <summary>
		/// check if windows visible
		/// </summary>
		/// <param name="hWnd"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		/// <summary>
		/// return windows text
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="lpWindowText"></param>
		/// <param name="nMaxCount"></param>
		/// <returns></returns>
		[DllImport("user32.dll", EntryPoint = "GetWindowText",
		ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

		/// <summary>
		/// enumarator on all desktop windows
		/// </summary>
		/// <param name="hDesktop"></param>
		/// <param name="lpEnumCallbackFunction"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
		ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);


		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern void GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		
		public static int CountVisibleWindows()
		{
			var collection = new List<string>();
			user32.EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
			{
				StringBuilder strbTitle = new StringBuilder(255);
				int nLength = user32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
				string strTitle = strbTitle.ToString();

				if(user32.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
				{
					collection.Add(strTitle);
				}
				return true;
			};

			if(user32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
			{
				return collection.Count;
			}
			return 0;
		}
	}

}
