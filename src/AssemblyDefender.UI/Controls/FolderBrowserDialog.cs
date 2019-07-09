using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;

namespace AssemblyDefender.UI
{
	public class FolderBrowserDialog : CommonDialog
	{
		#region Fields

		private const int MAX_PATH = 260;
		private bool _selectedPathNeedsCheck;
		private bool _showFullPathInEditBox;
		private int _dialogOptions;
		private string _selectedPath;
		private string _title;
		private IntPtr _hwndEdit;
		private IntPtr _rootFolderLocation;
		private PInvoke.BrowseFolderCallbackProc _callback;
		private Environment.SpecialFolder _rootFolder;

		#endregion

		#region Ctors

		public FolderBrowserDialog()
		{
			Initialize();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Enable or disable the "New Folder" button in the browser dialog.
		/// </summary>
		public bool ShowNewFolderButton
		{
			get { return GetOption(BrowseFlags.BIF_NONEWFOLDERBUTTON); }
			set { SetOption(BrowseFlags.BIF_NONEWFOLDERBUTTON, value); }
		}

		/// <summary>
		/// Show an "edit box" in the folder browser.
		/// </summary>
		/// <remarks>
		/// The "edit box" normally shows the name of the selected folder.
		/// The user may also type a pathname directly into the edit box.
		/// </remarks>
		/// <seealso cref="ShowFullPathInEditBox"/>
		public bool ShowEditBox
		{
			get { return GetOption(BrowseFlags.BIF_EDITBOX); }
			set { SetOption(BrowseFlags.BIF_EDITBOX, value); }
		}

		/// <summary>
		/// Show the full path in the edit box as the user selects it.
		/// </summary>
		/// <remarks>
		/// This works only if ShowEditBox is also set to true.
		/// </remarks>
		public bool ShowFullPathInEditBox
		{
			get { return _showFullPathInEditBox; }
			set { _showFullPathInEditBox = value; }
		}

		public bool ShowBothFilesAndFolders
		{
			get { return GetOption(BrowseFlags.BIF_BROWSEINCLUDEFILES); }
			set { SetOption(BrowseFlags.BIF_BROWSEINCLUDEFILES, value); }
		}

		/// <summary>
		/// Gets or Sets whether to use the New Folder Browser dialog style.
		/// </summary>
		/// <remarks>
		/// The new style is resizable and includes a "New Folder" button.
		/// </remarks>
		public bool NewStyle
		{
			get { return GetOption(BrowseFlags.BIF_NEWDIALOGSTYLE); }
			set { SetOption(BrowseFlags.BIF_NEWDIALOGSTYLE, value); }
		}

		public bool DontIncludeNetworkFoldersBelowDomainLevel
		{
			get { return GetOption(BrowseFlags.BIF_DONTGOBELOWDOMAIN); }
			set { SetOption(BrowseFlags.BIF_DONTGOBELOWDOMAIN, value); }
		}

		/// <summary>
		/// This title appears near the top of the dialog box, providing direction to the user.
		/// </summary>
		public string Title
		{
			get { return _title; }
			set { _title = value ?? ""; }
		}

		/// <summary>
		/// Set or get the selected path.
		/// </summary>
		public string SelectedPath
		{
			get
			{
				if (_selectedPathNeedsCheck && !string.IsNullOrEmpty(_selectedPath))
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, _selectedPath).Demand();
					_selectedPathNeedsCheck = false;
				}

				return _selectedPath;
			}
			set
			{
				_selectedPath = value ?? string.Empty;
				_selectedPathNeedsCheck = true;
			}
		}

		public Environment.SpecialFolder RootFolder
		{
			get { return _rootFolder; }
			set
			{
				_rootFolder = value;
				_rootFolderLocation = IntPtr.Zero;
			}
		}

		#endregion

		#region Methods

		public override void Reset()
		{
			Initialize();
		}

		protected override bool RunDialog(IntPtr hwndOwner)
		{
			bool result = false;

			if (_rootFolderLocation == IntPtr.Zero)
			{
				PInvoke.Shell32.SHGetSpecialFolderLocation(hwndOwner, (int)_rootFolder, ref _rootFolderLocation);
				if (_rootFolderLocation == IntPtr.Zero)
				{
					PInvoke.Shell32.SHGetSpecialFolderLocation(hwndOwner, 0, ref _rootFolderLocation);
				}
			}

			_hwndEdit = IntPtr.Zero;

			IntPtr pidl = IntPtr.Zero;
			IntPtr hglobal = IntPtr.Zero;
			IntPtr pszPath = IntPtr.Zero;
			try
			{
				var browseInfo = new PInvoke.BROWSEINFO();
				hglobal = Marshal.AllocHGlobal(MAX_PATH * Marshal.SystemDefaultCharSize);
				pszPath = Marshal.AllocHGlobal(MAX_PATH * Marshal.SystemDefaultCharSize);
				_callback = new PInvoke.BrowseFolderCallbackProc(FolderBrowserCallback);

				if (_rootFolderLocation != IntPtr.Zero)
					browseInfo.pidlRoot = _rootFolderLocation;

				browseInfo.Owner = hwndOwner;
				browseInfo.pszDisplayName = hglobal;
				browseInfo.Title = _title;
				browseInfo.Flags = _dialogOptions;
				browseInfo.callback = _callback;
				browseInfo.lParam = IntPtr.Zero;
				browseInfo.iImage = 0;
				pidl = PInvoke.Shell32.SHBrowseForFolder(browseInfo);
				if (((_dialogOptions & BrowseFlags.BIF_BROWSEFORPRINTER) == BrowseFlags.BIF_BROWSEFORPRINTER) ||
					((_dialogOptions & BrowseFlags.BIF_BROWSEFORCOMPUTER) == BrowseFlags.BIF_BROWSEFORCOMPUTER))
				{
					_selectedPath = Marshal.PtrToStringAuto(browseInfo.pszDisplayName);
					result = true;
				}
				else
				{
					if (pidl != IntPtr.Zero)
					{
						PInvoke.Shell32.SHGetPathFromIDList(pidl, pszPath);
						_selectedPathNeedsCheck = true;
						_selectedPath = Marshal.PtrToStringAuto(pszPath);
						result = true;
					}
				}
			}
			finally
			{
				var sHMalloc = GetSHMalloc();
				sHMalloc.Free(_rootFolderLocation);
				_rootFolderLocation = IntPtr.Zero;
				if (pidl != IntPtr.Zero)
				{
					sHMalloc.Free(pidl);
				}
				if (pszPath != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(pszPath);
				}
				if (hglobal != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(hglobal);
				}
				_callback = null;
			}

			return result;
		}

		private int FolderBrowserCallback(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData)
		{
			switch (msg)
			{
				case BrowseForFolderMessages.BFFM_INITIALIZED:
					if (_selectedPath.Length != 0)
					{
						PInvoke.User32.SendMessage(new HandleRef(null, hwnd), BrowseForFolderMessages.BFFM_SETSELECTIONW, 1, _selectedPath);
						if (ShowEditBox && ShowFullPathInEditBox)
						{
							// get handle to the Edit box inside the Folder Browser Dialog
							_hwndEdit = PInvoke.User32.FindWindowEx(new HandleRef(null, hwnd), IntPtr.Zero, "Edit", null);
							PInvoke.User32.SetWindowText(_hwndEdit, _selectedPath);
						}
					}
					break;

				case BrowseForFolderMessages.BFFM_SELCHANGED:
					IntPtr pidl = lParam;
					if (pidl != IntPtr.Zero)
					{
						if (((_dialogOptions & BrowseFlags.BIF_BROWSEFORPRINTER) == BrowseFlags.BIF_BROWSEFORPRINTER) ||
							((_dialogOptions & BrowseFlags.BIF_BROWSEFORCOMPUTER) == BrowseFlags.BIF_BROWSEFORCOMPUTER))
						{
							// we're browsing for a printer or computer, enable the OK button unconditionally.
							PInvoke.User32.SendMessage(new HandleRef(null, hwnd), BrowseForFolderMessages.BFFM_ENABLEOK, 0, 1);
						}
						else
						{
							IntPtr pszPath = Marshal.AllocHGlobal(MAX_PATH * Marshal.SystemDefaultCharSize);
							bool haveValidPath = PInvoke.Shell32.SHGetPathFromIDList(pidl, pszPath);
							String displayedPath = Marshal.PtrToStringAuto(pszPath);
							Marshal.FreeHGlobal(pszPath);
							// whether to enable the OK button or not. (if file is valid)
							PInvoke.User32.SendMessage(new HandleRef(null, hwnd), BrowseForFolderMessages.BFFM_ENABLEOK, 0, haveValidPath ? 1 : 0);

							// Maybe set the Edit Box text to the Full Folder path
							if (haveValidPath && !String.IsNullOrEmpty(displayedPath))
							{
								if (ShowEditBox && ShowFullPathInEditBox)
								{
									if (_hwndEdit != IntPtr.Zero)
										PInvoke.User32.SetWindowText(_hwndEdit, displayedPath);
								}

								if ((_dialogOptions & BrowseFlags.BIF_STATUSTEXT) == BrowseFlags.BIF_STATUSTEXT)
								{
									PInvoke.User32.SendMessage(new HandleRef(null, hwnd), BrowseForFolderMessages.BFFM_SETSTATUSTEXT, 0, displayedPath);
								}
							}
						}
					}
					break;
			}

			return 0;
		}

		private static PInvoke.IMalloc GetSHMalloc()
		{
			var ppMalloc = new PInvoke.IMalloc[1];
			PInvoke.Shell32.SHGetMalloc(ppMalloc);
			return ppMalloc[0];
		}

		private void Initialize()
		{
			ShowNewFolderButton = true;
			ShowEditBox = true;
			NewStyle = true;
			DontIncludeNetworkFoldersBelowDomainLevel = false;
			_selectedPathNeedsCheck = false;
			_title = string.Empty;
			_selectedPath = string.Empty;
			_hwndEdit = IntPtr.Zero;
			_rootFolderLocation = IntPtr.Zero;
			_rootFolder = (Environment.SpecialFolder)0;
		}

		private bool GetOption(int option)
		{
			return (_dialogOptions & option) != 0;
		}

		private void SetOption(int option, bool value)
		{
			if (value)
				_dialogOptions |= option;
			else
				_dialogOptions &= ~option;
		}

		#endregion

		#region Nested types

		private static class CSIDL
		{
			public const int PRINTERS = 4;
			public const int NETWORK = 0x12;
		}

		private static class BrowseFlags
		{
			public const int BIF_DEFAULT = 0x0000;
			public const int BIF_BROWSEFORCOMPUTER = 0x1000;
			public const int BIF_BROWSEFORPRINTER = 0x2000;
			public const int BIF_BROWSEINCLUDEFILES = 0x4000;
			public const int BIF_BROWSEINCLUDEURLS = 0x0080;
			public const int BIF_DONTGOBELOWDOMAIN = 0x0002;
			public const int BIF_EDITBOX = 0x0010;
			public const int BIF_NEWDIALOGSTYLE = 0x0040;
			public const int BIF_NONEWFOLDERBUTTON = 0x0200;
			public const int BIF_RETURNFSANCESTORS = 0x0008;
			public const int BIF_RETURNONLYFSDIRS = 0x0001;
			public const int BIF_SHAREABLE = 0x8000;
			public const int BIF_STATUSTEXT = 0x0004;
			public const int BIF_UAHINT = 0x0100;
			public const int BIF_VALIDATE = 0x0020;
			public const int BIF_NOTRANSLATETARGETS = 0x0400;
		}

		private static class BrowseForFolderMessages
		{
			// messages FROM the folder browser
			public const int BFFM_INITIALIZED = 1;
			public const int BFFM_SELCHANGED = 2;
			public const int BFFM_VALIDATEFAILEDA = 3;
			public const int BFFM_VALIDATEFAILEDW = 4;
			public const int BFFM_IUNKNOWN = 5;

			// messages TO the folder browser
			public const int BFFM_SETSTATUSTEXT = 0x464;
			public const int BFFM_ENABLEOK = 0x465;
			public const int BFFM_SETSELECTIONA = 0x466;
			public const int BFFM_SETSELECTIONW = 0x467;
		}

		private static class PInvoke
		{
			public delegate int BrowseFolderCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);

			internal static class User32
			{
				[DllImport("user32.dll", CharSet = CharSet.Auto)]
				public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

				[DllImport("user32.dll", CharSet = CharSet.Auto)]
				public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

				[DllImport("user32.dll", SetLastError = true)]
				//public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
				//public static extern IntPtr FindWindowEx(HandleRef hwndParent, HandleRef hwndChildAfter, string lpszClass, string lpszWindow);
				public static extern IntPtr FindWindowEx(HandleRef hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

				[DllImport("user32.dll", SetLastError = true)]
				public static extern Boolean SetWindowText(IntPtr hWnd, String text);
			}

			[ComImport, Guid("00000002-0000-0000-c000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IMalloc
			{
				[PreserveSig]
				IntPtr Alloc(int cb);

				[PreserveSig]
				IntPtr Realloc(IntPtr pv, int cb);

				[PreserveSig]
				void Free(IntPtr pv);

				[PreserveSig]
				int GetSize(IntPtr pv);

				[PreserveSig]
				int DidAlloc(IntPtr pv);

				[PreserveSig]
				void HeapMinimize();
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			public class BROWSEINFO
			{
				public IntPtr Owner;
				public IntPtr pidlRoot;
				public IntPtr pszDisplayName;
				public string Title;
				public int Flags;
				public BrowseFolderCallbackProc callback;
				public IntPtr lParam;
				public int iImage;
			}

			[SuppressUnmanagedCodeSecurity]
			internal static class Shell32
			{
				[DllImport("shell32.dll", CharSet = CharSet.Auto)]
				public static extern IntPtr SHBrowseForFolder([In] PInvoke.BROWSEINFO lpbi);

				[DllImport("shell32.dll")]
				public static extern int SHGetMalloc([Out, MarshalAs(UnmanagedType.LPArray)] PInvoke.IMalloc[] ppMalloc);

				[DllImport("shell32.dll", CharSet = CharSet.Auto)]
				public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

				[DllImport("shell32.dll")]
				public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);
			}
		}

		#endregion
	}
}
