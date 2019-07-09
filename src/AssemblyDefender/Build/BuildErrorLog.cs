using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class BuildErrorLog
	{
		#region Fields

		private string _message;
		private string _type;
		private string _source;
		private string _hint;
		private string _helpLink;
		private string _stackTrace;
		private BuildErrorLog _inner;

		#endregion

		#region Ctors

		public BuildErrorLog()
		{
		}

		public BuildErrorLog(Exception e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			_message = string.IsNullOrEmpty(e.Message) ? Common.SR.InternalError : e.Message;
			_type = e.GetType().FullName;
			_source = e.Source;
			_helpLink = e.HelpLink;
			_stackTrace = e.StackTrace;

			if (e is ResolveReferenceException)
			{
				_hint = SR.ResolveReferenceErrorHint;
			}
			else if (e is NativeCodeException)
			{
				_hint = SR.NativeCodeErrorHint;
			}
			else if (e is InvalidOperationException
				|| e is NotImplementedException
				|| e is NullReferenceException
				|| e is ArgumentException)
			{
				_message = Common.SR.InternalError;
			}

			if (e.InnerException != null)
			{
				_inner = new BuildErrorLog(e.InnerException);
			}
		}

		#endregion

		#region Properties

		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public string Source
		{
			get { return _source; }
			set { _source = value; }
		}

		public string Hint
		{
			get { return _hint; }
			set { _hint = value; }
		}

		public string HelpLink
		{
			get { return _helpLink; }
			set { _helpLink = value; }
		}

		public string StackTrace
		{
			get { return _stackTrace; }
			set { _stackTrace = value; }
		}

		public BuildErrorLog Inner
		{
			get { return _inner; }
			set { _inner = value; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return ToString(false, true);
		}

		public string ToString(bool includeEnvironment = false, bool includeStackTrace = false)
		{
			var sb = new StringBuilder();

			if (includeEnvironment)
			{
				sb.AppendFormat("Operating system: {0}", Environment.OSVersion.VersionString).AppendLine();
				sb.AppendFormat("NET framework: {0}", Environment.Version.ToString()).AppendLine();
				sb.AppendFormat("64 bit OS: {0}", Environment.Is64BitOperatingSystem).AppendLine();
				sb.AppendFormat("64 bit process: {0}", Environment.Is64BitProcess).AppendLine();
				sb.AppendFormat("Command line: {0}", Environment.CommandLine).AppendLine();
			}

			PrintException(sb, this);

			if (includeStackTrace && !string.IsNullOrEmpty(_stackTrace))
			{
				sb.Append("--- Stack trace ----------------------------------------------------------").AppendLine();
				sb.Append(_stackTrace).AppendLine();
			}

			return sb.ToString();
		}

		private void PrintException(StringBuilder sb, BuildErrorLog e)
		{
			sb.Append("--- Exception ----------------------------------------------------------").AppendLine();

			sb.AppendFormat("Message: {0}", e.Message).AppendLine();

			if (!string.IsNullOrEmpty(e.Type))
				sb.AppendFormat("Type: {0}", e.Type).AppendLine();

			if (!string.IsNullOrEmpty(e.Source))
				sb.AppendFormat("Source: {0}", e.Source).AppendLine();

			if (!string.IsNullOrEmpty(e.Hint))
				sb.AppendFormat("Hint: {0}", e.Hint).AppendLine();

			if (!string.IsNullOrEmpty(e.HelpLink))
				sb.AppendFormat("Help link: {0}", e.HelpLink).AppendLine();

			if (e.Inner != null)
			{
				PrintException(sb, e.Inner);
			}
		}

		internal void Read(Blob blob, ref int pos)
		{
			_message = blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);
			_type = blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);
			_source = blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);
			_hint = blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);
			_helpLink = blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);
			_stackTrace = blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);

			if (blob.ReadBoolean(ref pos))
			{
				_inner = new BuildErrorLog();
				_inner.Read(blob, ref pos);
			}
		}

		internal void Write(Blob blob, ref int pos)
		{
			blob.WriteLengthPrefixedString(ref pos, _message, Encoding.UTF8);
			blob.WriteLengthPrefixedString(ref pos, _type, Encoding.UTF8);
			blob.WriteLengthPrefixedString(ref pos, _source, Encoding.UTF8);
			blob.WriteLengthPrefixedString(ref pos, _hint, Encoding.UTF8);
			blob.WriteLengthPrefixedString(ref pos, _helpLink, Encoding.UTF8);
			blob.WriteLengthPrefixedString(ref pos, _stackTrace, Encoding.UTF8);

			if (_inner != null)
			{
				blob.Write(ref pos, true);
				_inner.Write(blob, ref pos);
			}
			else
			{
				blob.Write(ref pos, false);
			}
		}

		#endregion
	}
}
