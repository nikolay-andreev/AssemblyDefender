using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectAssemblySign
	{
		#region Fields

		private string _keyFilePath;
		private string _password;
		private bool _delaySign;

		#endregion

		#region Ctors

		public ProjectAssemblySign()
		{
		}

		internal ProjectAssemblySign(IBinaryAccessor accessor, ProjectReadState state)
		{
			Read(accessor, state);
		}

		#endregion

		#region Properties

		public string KeyFile
		{
			get { return _keyFilePath; }
			set { _keyFilePath = value; }
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public bool DelaySign
		{
			get { return _delaySign; }
			set { _delaySign = value; }
		}

		#endregion

		#region Methods

		internal void Read(IBinaryAccessor accessor, ProjectReadState state)
		{
			_keyFilePath = ProjectHelper.MakeAbsolutePath(state.GetString(accessor.Read7BitEncodedInt()), state.BasePath);
			_password = state.GetString(accessor.Read7BitEncodedInt());
			_delaySign = accessor.ReadBoolean();
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write7BitEncodedInt(state.SetString(ProjectHelper.MakeRelativePath(_keyFilePath, state.BasePath)));
			accessor.Write7BitEncodedInt(state.SetString(_password));
			accessor.Write((bool)_delaySign);
		}

		#endregion
	}
}
