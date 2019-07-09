using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AssemblyDefender.UI.Model;

namespace AssemblyDefender.UI.Shell
{
	public static class Images
	{
		private static ResourceDictionary _resources;
		private static string[] _names;

		static Images()
		{
			Initialize();
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ArrowBackward
		{
			get { return GetImage(ImageType.ArrowBackward); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ArrowForward
		{
			get { return GetImage(ImageType.ArrowForward); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Bitmap
		{
			get { return GetImage(ImageType.Bitmap); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Check
		{
			get { return GetImage(ImageType.Check); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Check2
		{
			get { return GetImage(ImageType.Check2); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Close10
		{
			get { return GetImage(ImageType.Close10); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource CloseHover10
		{
			get { return GetImage(ImageType.CloseHover10); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ClosePressed10
		{
			get { return GetImage(ImageType.ClosePressed10); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Contact
		{
			get { return GetImage(ImageType.Contact); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Copy
		{
			get { return GetImage(ImageType.Copy); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Critical
		{
			get { return GetImage(ImageType.Critical); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Cut
		{
			get { return GetImage(ImageType.Cut); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Delete
		{
			get { return GetImage(ImageType.Delete); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource DeleteGray
		{
			get { return GetImage(ImageType.DeleteGray); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource DeleteRed
		{
			get { return GetImage(ImageType.DeleteRed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Error
		{
			get { return GetImage(ImageType.Error); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Error32
		{
			get { return GetImage(ImageType.Error32); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Warning
		{
			get { return GetImage(ImageType.Warning); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Folder_Closed
		{
			get { return GetImage(ImageType.Folder_Closed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Folder_Open
		{
			get { return GetImage(ImageType.Folder_Open); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource GoBack
		{
			get { return GetImage(ImageType.GoBack); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Help
		{
			get { return GetImage(ImageType.Help); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Hint
		{
			get { return GetImage(ImageType.Help); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ImageFile
		{
			get { return GetImage(ImageType.ImageFile); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Info
		{
			get { return GetImage(ImageType.Info); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Info32
		{
			get { return GetImage(ImageType.Info32); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Input
		{
			get { return GetImage(ImageType.Input); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Keys
		{
			get { return GetImage(ImageType.Keys); }
		}


		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ListView
		{
			get { return GetImage(ImageType.ListView); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Lock_Locked
		{
			get { return GetImage(ImageType.Lock_Locked); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource GearError32
		{
			get { return GetImage(ImageType.GearError32); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource LogoIcon
		{
			get { return GetImage(ImageType.LogoIcon); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource New
		{
			get { return GetImage(ImageType.New); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource NextBr
		{
			get { return GetImage(ImageType.NextBr); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Assembly
		{
			get { return GetImage(ImageType.Node_Assembly); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Assembly_Add
		{
			get { return GetImage(ImageType.Node_Assembly_Add); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Assembly_Invalid
		{
			get { return GetImage(ImageType.Node_Assembly_Invalid); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Class
		{
			get { return GetImage(ImageType.Node_Class); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Class_Friend
		{
			get { return GetImage(ImageType.Node_Class_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Class_Private
		{
			get { return GetImage(ImageType.Node_Class_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Class_Protected
		{
			get { return GetImage(ImageType.Node_Class_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Class_Sealed
		{
			get { return GetImage(ImageType.Node_Class_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Delegate
		{
			get { return GetImage(ImageType.Node_Delegate); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Delegate_Friend
		{
			get { return GetImage(ImageType.Node_Delegate_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Delegate_Private
		{
			get { return GetImage(ImageType.Node_Delegate_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Delegate_Protected
		{
			get { return GetImage(ImageType.Node_Delegate_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Delegate_Sealed
		{
			get { return GetImage(ImageType.Node_Delegate_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Enum
		{
			get { return GetImage(ImageType.Node_Enum); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_EnumItem
		{
			get { return GetImage(ImageType.Node_EnumItem); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_EnumItem_Friend
		{
			get { return GetImage(ImageType.Node_EnumItem_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_EnumItem_Private
		{
			get { return GetImage(ImageType.Node_EnumItem_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_EnumItem_Protected
		{
			get { return GetImage(ImageType.Node_EnumItem_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_EnumItem_Sealed
		{
			get { return GetImage(ImageType.Node_EnumItem_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Enum_Friend
		{
			get { return GetImage(ImageType.Node_Enum_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Enum_Private
		{
			get { return GetImage(ImageType.Node_Enum_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Enum_Protected
		{
			get { return GetImage(ImageType.Node_Enum_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Enum_Sealed
		{
			get { return GetImage(ImageType.Node_Enum_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Event
		{
			get { return GetImage(ImageType.Node_Event); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Event_Friend
		{
			get { return GetImage(ImageType.Node_Event_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Event_Private
		{
			get { return GetImage(ImageType.Node_Event_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Event_Protected
		{
			get { return GetImage(ImageType.Node_Event_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Event_Sealed
		{
			get { return GetImage(ImageType.Node_Event_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Event_Shortcut
		{
			get { return GetImage(ImageType.Node_Event_Shortcut); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Field
		{
			get { return GetImage(ImageType.Node_Field); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Field_Friend
		{
			get { return GetImage(ImageType.Node_Field_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Field_Private
		{
			get { return GetImage(ImageType.Node_Field_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Field_Protected
		{
			get { return GetImage(ImageType.Node_Field_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Field_Sealed
		{
			get { return GetImage(ImageType.Node_Field_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Interface
		{
			get { return GetImage(ImageType.Node_Interface); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Interface_Friend
		{
			get { return GetImage(ImageType.Node_Interface_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Interface_Private
		{
			get { return GetImage(ImageType.Node_Interface_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Interface_Protected
		{
			get { return GetImage(ImageType.Node_Interface_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Interface_Sealed
		{
			get { return GetImage(ImageType.Node_Interface_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Method
		{
			get { return GetImage(ImageType.Node_Method); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Method_Friend
		{
			get { return GetImage(ImageType.Node_Method_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Method_Private
		{
			get { return GetImage(ImageType.Node_Method_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Method_Protected
		{
			get { return GetImage(ImageType.Node_Method_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Method_Sealed
		{
			get { return GetImage(ImageType.Node_Method_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Method_Shortcut
		{
			get { return GetImage(ImageType.Node_Method_Shortcut); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Module
		{
			get { return GetImage(ImageType.Node_Module); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Namespace
		{
			get { return GetImage(ImageType.Node_Namespace); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Properties
		{
			get { return GetImage(ImageType.Node_Properties); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Properties_Friend
		{
			get { return GetImage(ImageType.Node_Properties_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Properties_Private
		{
			get { return GetImage(ImageType.Node_Properties_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Properties_Protected
		{
			get { return GetImage(ImageType.Node_Properties_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Properties_Sealed
		{
			get { return GetImage(ImageType.Node_Properties_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Properties_Shortcut
		{
			get { return GetImage(ImageType.Node_Properties_Shortcut); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_Resource
		{
			get { return GetImage(ImageType.Node_Resource); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_ValueType
		{
			get { return GetImage(ImageType.Node_ValueType); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_ValueType_Friend
		{
			get { return GetImage(ImageType.Node_ValueType_Friend); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_ValueType_Private
		{
			get { return GetImage(ImageType.Node_ValueType_Private); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_ValueType_Protected
		{
			get { return GetImage(ImageType.Node_ValueType_Protected); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Node_ValueType_Sealed
		{
			get { return GetImage(ImageType.Node_ValueType_Sealed); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Obfuscate
		{
			get { return GetImage(ImageType.Obfuscate); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Obfuscate_ControlFlow
		{
			get { return GetImage(ImageType.Obfuscate_ControlFlow); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Obfuscate_EncryptIL
		{
			get { return GetImage(ImageType.Obfuscate_EncryptIL); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Obfuscate_Rename
		{
			get { return GetImage(ImageType.Obfuscate_Rename); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Obfuscate_Resources
		{
			get { return GetImage(ImageType.Obfuscate_Resources); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Obfuscate_Strings
		{
			get { return GetImage(ImageType.Obfuscate_Strings); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Open
		{
			get { return GetImage(ImageType.Open); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Optimize
		{
			get { return GetImage(ImageType.Optimize); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Optimize_DevirtualizeMethod
		{
			get { return GetImage(ImageType.Optimize_DevirtualizeMethod); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Optimize_Prune
		{
			get { return GetImage(ImageType.Optimize_Prune); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Optimize_SealTypes
		{
			get { return GetImage(ImageType.Optimize_SealTypes); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Output
		{
			get { return GetImage(ImageType.Output); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Paste
		{
			get { return GetImage(ImageType.Paste); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Print
		{
			get { return GetImage(ImageType.Print); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Project
		{
			get { return GetImage(ImageType.Project); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Redo
		{
			get { return GetImage(ImageType.Redo); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Refresh
		{
			get { return GetImage(ImageType.Refresh); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ReportBug
		{
			get { return GetImage(ImageType.ReportBug); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Save
		{
			get { return GetImage(ImageType.Save); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource SaveAll
		{
			get { return GetImage(ImageType.SaveAll); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Search
		{
			get { return GetImage(ImageType.Search); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Stop
		{
			get { return GetImage(ImageType.Stop); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Undo
		{
			get { return GetImage(ImageType.Undo); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Web
		{
			get { return GetImage(ImageType.Web); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource LogoHeader
		{
			get { return GetImage(ImageType.LogoHeader); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource EarthConnection32
		{
			get { return GetImage(ImageType.EarthConnection32); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource EarthDelete32
		{
			get { return GetImage(ImageType.EarthDelete32); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource Compile
		{
			get { return GetImage(ImageType.Compile); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource OpenFile
		{
			get { return GetImage(ImageType.OpenFile); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource ExpandVertical
		{
			get { return GetImage(ImageType.ExpandVertical); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static ImageSource CollapseVertical
		{
			get { return GetImage(ImageType.CollapseVertical); }
		}

		public static ImageSource GetImage(ImageType type)
		{
			if (type == ImageType.None)
				return null;

			return (ImageSource)_resources[_names[(int)type - 1]];
		}

		private static void Initialize()
		{
			_resources = new ResourceDictionary
			{
				Source = new Uri("/Resources/Images.xaml", UriKind.RelativeOrAbsolute)
			};

			_names = new string[]
			{
				"ArrowBackward_ImageSource",
				"ArrowForward_ImageSource",
				"Bitmap_ImageSource",
				"Check_ImageSource",
				"Check2_ImageSource",
				"Close10_ImageSource",
				"CloseHover10_ImageSource",
				"ClosePressed10_ImageSource",
				"Contact_ImageSource",
				"Copy_ImageSource",
				"Critical_ImageSource",
				"Cut_ImageSource",
				"Delete_ImageSource",
                "DeleteGray_ImageSource",
				"DeleteRed_ImageSource",
				"Error_ImageSource",
				"Error32_ImageSource",
				"Warning_ImageSource",
				"Folder_Closed_ImageSource",
				"Folder_Open_ImageSource",
				"GoBack_ImageSource",
				"Help_ImageSource",
				"Hint_ImageSource",
				"ImageFile_ImageSource",
				"Info_ImageSource",
				"Info32_ImageSource",
				"Input_ImageSource",
				"Keys_ImageSource",
				"ListView_ImageSource",
				"Lock_Locked_ImageSource",
				"GearError32_ImageSource",
				"LogoIcon_ImageSource",
				"New_ImageSource",
				"NextBr_ImageSource",
				"Node_Assembly_ImageSource",
				"Node_Assembly_Add_ImageSource",
				"Node_Assembly_Invalid_ImageSource",
				"Node_Class_ImageSource",
				"Node_Class_Friend_ImageSource",
				"Node_Class_Private_ImageSource",
				"Node_Class_Protected_ImageSource",
				"Node_Class_Sealed_ImageSource",
				"Node_Delegate_ImageSource",
				"Node_Delegate_Friend_ImageSource",
				"Node_Delegate_Private_ImageSource",
				"Node_Delegate_Protected_ImageSource",
				"Node_Delegate_Sealed_ImageSource",
				"Node_Enum_ImageSource",
				"Node_EnumItem_ImageSource",
				"Node_EnumItem_Friend_ImageSource",
				"Node_EnumItem_Private_ImageSource",
				"Node_EnumItem_Protected_ImageSource",
				"Node_EnumItem_Sealed_ImageSource",
				"Node_Enum_Friend_ImageSource",
				"Node_Enum_Private_ImageSource",
				"Node_Enum_Protected_ImageSource",
				"Node_Enum_Sealed_ImageSource",
				"Node_Event_ImageSource",
				"Node_Event_Friend_ImageSource",
				"Node_Event_Private_ImageSource",
				"Node_Event_Protected_ImageSource",
				"Node_Event_Sealed_ImageSource",
				"Node_Event_Shortcut_ImageSource",
				"Node_Field_ImageSource",
				"Node_Field_Friend_ImageSource",
				"Node_Field_Private_ImageSource",
				"Node_Field_Protected_ImageSource",
				"Node_Field_Sealed_ImageSource",
				"Node_Interface_ImageSource",
				"Node_Interface_Friend_ImageSource",
				"Node_Interface_Private_ImageSource",
				"Node_Interface_Protected_ImageSource",
				"Node_Interface_Sealed_ImageSource",
				"Node_Method_ImageSource",
				"Node_Method_Friend_ImageSource",
				"Node_Method_Private_ImageSource",
				"Node_Method_Protected_ImageSource",
				"Node_Method_Sealed_ImageSource",
				"Node_Method_Shortcut_ImageSource",
				"Node_Module_ImageSource",
				"Node_Namespace_ImageSource",
				"Node_Properties_ImageSource",
				"Node_Properties_Friend_ImageSource",
				"Node_Properties_Private_ImageSource",
				"Node_Properties_Protected_ImageSource",
				"Node_Properties_Sealed_ImageSource",
				"Node_Properties_Shortcut_ImageSource",
				"Node_Resource_ImageSource",
				"Node_ValueType_ImageSource",
				"Node_ValueType_Friend_ImageSource",
				"Node_ValueType_Private_ImageSource",
				"Node_ValueType_Protected_ImageSource",
				"Node_ValueType_Sealed_ImageSource",
				"Obfuscate_ImageSource",
				"Obfuscate_ControlFlow_ImageSource",
				"Obfuscate_EncryptIL_ImageSource",
				"Obfuscate_Rename_ImageSource",
				"Obfuscate_Resources_ImageSource",
				"Obfuscate_Strings_ImageSource",
				"Open_ImageSource",
				"Optimize_ImageSource",
				"Optimize_DevirtualizeMethod_ImageSource",
				"Optimize_Prune_ImageSource",
				"Optimize_SealTypes_ImageSource",
				"Output_ImageSource",
				"Paste_ImageSource",
				"Print_ImageSource",
				"Project_ImageSource",
				"Redo_ImageSource",
				"Refresh_ImageSource",
				"ReportBug_ImageSource",
				"Save_ImageSource",
				"SaveAll_ImageSource",
				"Search_ImageSource",
				"Stop_ImageSource",
				"Undo_ImageSource",
				"Web_ImageSource",
				"LogoHeader_ImageSource",
				"EarthConnection32_ImageSource",
				"EarthDelete32_ImageSource",
                "Compile_ImageSource",
                "OpenFile_ImageSource",
				"ExpandVertical_ImageSource",
				"CollapseVertical_ImageSource",			
			};
		}
	}
}
