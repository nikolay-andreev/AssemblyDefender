using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;

namespace AssemblyDefender.Baml
{
	/// <summary>
	/// pack://application:,,,/ReferencedAssembly;v1.1.3.1;0a192c7e70ecea27;component/Subfolder/ResourceFile.xaml
	/// pack://siteoforigin:,,,/SomeAssembly;component/ResourceFile.xaml
	/// /ReferencedAssembly;component/Subfolder/ResourceFile.xaml
	/// </summary>
	public class PackUri
	{
		#region Fields

		private PackUriAuthority _authority;
		private AssemblyReference _assembly;
		private string _path;

		#endregion

		#region Ctors

		public PackUri()
		{
		}

		public PackUri(string path)
		{
			_path = path;
		}

		public PackUri(AssemblyReference assembly, string path)
		{
			_assembly = assembly;
			_path = path;
		}

		public PackUri(PackUriAuthority authority, AssemblyReference assembly, string path)
		{
			_authority = authority;
			_assembly = assembly;
			_path = path;
		}

		#endregion

		#region Properties

		public PackUriAuthority Authority
		{
			get { return _authority; }
			set { _authority = value; }
		}

		public AssemblyReference Assembly
		{
			get { return _assembly; }
			set { _assembly = value; }
		}

		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			var sb = new StringBuilder();

			switch (_authority)
			{
				case PackUriAuthority.None:
					break;

				case PackUriAuthority.Application:
					sb.Append("pack://application:,,,");
					break;

				case PackUriAuthority.SiteOfOrigin:
					sb.Append("pack://siteoforigin:,,,");
					break;

				default:
					throw new NotImplementedException();
			}

			if (_assembly != null)
			{
				sb.AppendFormat("/{0};", _assembly.Name);

				if (_assembly.Version != null)
				{
					sb.AppendFormat("v{0};", _assembly.Version.ToString());

					if (!BufferUtils.IsNullOrEmpty(_assembly.PublicKeyToken))
					{
						sb.AppendFormat("{0};", ConvertUtils.ToHexString(_assembly.PublicKeyToken));
					}
				}
			}

			if (_path != null)
			{
				sb.Append(_path);
			}

			return sb.ToString();
		}

		#endregion

		#region Static

		public static PackUri Parse(string input)
		{
			if (string.IsNullOrEmpty(input))
				return null;

			int pos = 0;

			var uri = new PackUri();

			if (input.StartsWith("pack://"))
			{
				pos += 7;

				string authorityString;
				if (!ReadTo(input, ref pos, ':', out authorityString))
					return null;

				if (authorityString == "application")
					uri.Authority = PackUriAuthority.Application;
				else if (authorityString == "siteoforigin")
					uri.Authority = PackUriAuthority.SiteOfOrigin;
				else
					return null;

				if (input.Length <= pos + 3)
					return null;

				if (input[pos++] != ',' || input[pos++] != ',' || input[pos++] != ',')
					return null;
			}

			string path;
			if (ReadTo(input, ref pos, ';', out path))
			{
				// Assembly
				if (path.Length < 1 || path[0] != '/')
					return null;

				string name = path.Substring(1);
				if (string.IsNullOrEmpty(name))
					return null;

				Version version = null;
				byte[] publicKeyToken = null;

				// Version
				if (ReadTo(input, ref pos, ';', out path))
				{
					if (path.Length < 1 || path[0] != 'v')
						return null;

					path = path.Substring(1);
					if (string.IsNullOrEmpty(path))
						return null;

					if (!Version.TryParse(path, out version))
						return null;

					// Public key token
					if (ReadTo(input, ref pos, ';', out path))
					{
						try
						{
							publicKeyToken = ConvertUtils.HexToByteArray(path);
						}
						catch (Exception)
						{
							return null;
						}

						if (input.Length > pos)
						{
							path = input.Substring(pos);
						}
						else
						{
							path = null;
						}
					}
				}

				uri.Assembly = new AssemblyReference(name, null, version, publicKeyToken);
			}

			uri.Path = path;

			return uri;
		}

		public static PackUri Parse(string s, bool throwOnError = false)
		{
			var uri = Parse(s);
			if (uri == null)
			{
				if (throwOnError)
				{
					throw new BamlException(SR.BamlLoadError);
				}

				return null;
			}

			return uri;
		}

		private static bool ReadTo(string input, ref int pos, char matchChar, out string readText)
		{
			int index = input.IndexOf(matchChar, pos);
			if (index < 0)
			{
				readText = input.Substring(pos);
				pos = input.Length;
				return false;
			}
			else
			{
				readText = input.Substring(pos, index - pos);
				pos = index + 1;
				return true;
			}
		}

		#endregion
	}
}
