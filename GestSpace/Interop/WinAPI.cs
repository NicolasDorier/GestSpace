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
	using System.Diagnostics;


	public class TabWindow
	{
		public string Name
		{
			get;
			set;
		}

		public string Title
		{
			get;
			set;
		}

		public IntPtr Handle
		{
			get;
			set;
		}
	}




	/// &lt;summary>
	/// EnumDesktopWindows Demo - shows the caption of all desktop windows.
	/// Authors: Svetlin Nakov, Martin Kulov 
	/// Bulgarian Association of Software Developers - http://www.devbg.org/en/
	/// &lt;/summary>
	public class user32
	{

		static int SW_SHOW = 5;
		static int SW_RESTORE = 9;
		static int GWL_HWNDPARENT = (-8);
		static int GWL_EXSTYLE = (-20);
		static int WS_EX_TOOLWINDOW = 0x80;
		static int WS_EX_APPWINDOW = 0x40000;

		static uint LB_ADDSTRING = 0x180;
		static uint LB_SETITEMDATA = 0x19A;

		//Declare Function AttachThreadInput Lib "user32" (ByVal idAttach As Long, ByVal idAttachTo As Long, ByVal fAttach As Long) As Long

		[DllImport("user32.dll")]
		static extern bool AttachThreadInput(uint idAttach, uint idAttachTo,
		   bool fAttach);
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

		enum GetWindow_Cmd : uint
		{
			GW_HWNDFIRST = 0,
			GW_HWNDLAST = 1,
			GW_HWNDNEXT = 2,
			GW_HWNDPREV = 3,
			GW_OWNER = 4,
			GW_CHILD = 5,
			GW_ENABLEDPOPUP = 6
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);


		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		enum ShowWindowCommands : int
		{
			/// <summary>
			/// Hides the window and activates another window.
			/// </summary>
			Hide = 0,
			/// <summary>
			/// Activates and displays a window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position.
			/// An application should specify this flag when displaying the window 
			/// for the first time.
			/// </summary>
			Normal = 1,
			/// <summary>
			/// Activates the window and displays it as a minimized window.
			/// </summary>
			ShowMinimized = 2,
			/// <summary>
			/// Maximizes the specified window.
			/// </summary>
			Maximize = 3, // is this the right value?
			/// <summary>
			/// Activates the window and displays it as a maximized window.
			/// </summary>       
			ShowMaximized = 3,
			/// <summary>
			/// Displays a window in its most recent size and position. This value 
			/// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
			/// the window is not activated.
			/// </summary>
			ShowNoActivate = 4,
			/// <summary>
			/// Activates the window and displays it in its current size and position. 
			/// </summary>
			Show = 5,
			/// <summary>
			/// Minimizes the specified window and activates the next top-level 
			/// window in the Z order.
			/// </summary>
			Minimize = 6,
			/// <summary>
			/// Displays the window as a minimized window. This value is similar to
			/// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
			/// window is not activated.
			/// </summary>
			ShowMinNoActive = 7,
			/// <summary>
			/// Displays the window in its current size and position. This value is 
			/// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
			/// window is not activated.
			/// </summary>
			ShowNA = 8,
			/// <summary>
			/// Activates and displays the window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position. 
			/// An application should specify this flag when restoring a minimized window.
			/// </summary>
			Restore = 9,
			/// <summary>
			/// Sets the show state based on the SW_* value specified in the 
			/// STARTUPINFO structure passed to the CreateProcess function by the 
			/// program that started the application.
			/// </summary>
			ShowDefault = 10,
			/// <summary>
			///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
			/// that owns the window is not responding. This flag should only be 
			/// used when minimizing windows from a different thread.
			/// </summary>
			ForceMinimize = 11
		}


		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

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

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
		[DllImport("user32.dll")]
 [return: MarshalAs(UnmanagedType.Bool)]
 static extern bool SetForegroundWindow(IntPtr hWnd);

		public static int CountVisibleWindows()
		{
			var collection = new List<string>();
			user32.EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
			{
				var strTitle = GetWindowText(hWnd);

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

		private static string GetWindowText(IntPtr hWnd)
		{
			StringBuilder strbTitle = new StringBuilder(255);
			int nLength = user32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
			return strbTitle.ToString();
		}

		public static void SetForeground(IntPtr hWnd)
		{
			uint pid;
			var lForeThreadID = GetWindowThreadProcessId(GetForegroundWindow(), out pid);
			var lThisThreadID = GetWindowThreadProcessId(hWnd, out pid);

			if(lForeThreadID != lThisThreadID)
			{
				AttachThreadInput(lForeThreadID, lThisThreadID, true);
				SetForegroundWindow(hWnd);
				AttachThreadInput(lForeThreadID, lThisThreadID, false);
			}
			else
				SetForegroundWindow(hWnd);

			if(IsIconic(hWnd))
				ShowWindow(hWnd, ShowWindowCommands.Restore);
			else
				ShowWindow(hWnd, ShowWindowCommands.Show);
		}
		public static IEnumerable<TabWindow> GetAltTabWindows()
		{
			List<TabWindow> result = new List<TabWindow>();
			EnumWindowsProc proc = delegate(IntPtr hWnd, IntPtr lParam)
			{
				if(IsWindowVisible(hWnd))
				{
					if(GetParent(hWnd) == IntPtr.Zero)
					{
						var bNoOwner = GetWindow(hWnd, GetWindow_Cmd.GW_OWNER) == IntPtr.Zero;
						var lExStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
						if((((lExStyle & WS_EX_TOOLWINDOW) == 0) && bNoOwner) ||
							(((lExStyle & WS_EX_APPWINDOW) != 0) && !bNoOwner))
						{
							var sWindowText = GetWindowText(hWnd);

							result.Add(new TabWindow()
							{
								Handle = hWnd,
								Name = string.IsNullOrEmpty(sWindowText) ? GetProcessName(hWnd) : sWindowText,
								Title = sWindowText
							});

						}
					}
				}
				return true;
			};
			EnumWindows(proc, IntPtr.Zero);
			return result;
		}

		private static string GetProcessName(IntPtr hWnd)
		{
			uint procId;
			GetWindowThreadProcessId(hWnd, out procId);
			using(var p = Process.GetProcessById((int)procId))
			{
				return p.ProcessName;
			}
		}
	}

}
