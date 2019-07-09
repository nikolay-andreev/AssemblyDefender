using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.UI.Model.Project;
using AD = AssemblyDefender;
using AssemblyDefender.Net;
using System.IO;

namespace AssemblyDefender.UI.Model
{
	public static class StrongNamePasswordCache
	{
		private static Dictionary<string, string> _keyFilePasswordByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		internal static bool TryGet(string filePath, out string password)
		{
			if (!_keyFilePasswordByPath.TryGetValue(filePath, out password))
			{
				password = Find(filePath);
				if (string.IsNullOrEmpty(password))
					return false;

				_keyFilePasswordByPath.Add(filePath, password);
			}


			return true;
		}

		private static string Find(string filePath)
		{
			var hash = new HashSet<string>();
			byte[] keyData = File.ReadAllBytes(filePath);
			foreach (string password in _keyFilePasswordByPath.Values)
			{
				if (hash.Contains(password))
					continue;

				if (StrongNameUtils.IsPasswordValid(keyData, password))
					return password;

				hash.Add(password);
			}

			var passwordViewModel = new PKCS12PasswordViewModel(AppService.Shell, filePath);
			AppService.UI.ShowPKCS12Password(passwordViewModel);

			return passwordViewModel.Password;
		}

		internal static void AddKeys(AD.Project project)
		{
			if (project != null)
			{
				foreach (var projectAssembly in project.Assemblies)
				{
					if (!projectAssembly.IsSigned)
						continue;

					var sign = projectAssembly.Sign;
					if (string.IsNullOrEmpty(sign.Password))
						continue;

					_keyFilePasswordByPath[sign.KeyFile] = sign.Password;
				}
			}
		}
	}
}
