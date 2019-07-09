using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common;

namespace AssemblyDefender.Net.Fusion
{
	public static class AssemblyCache
	{
		public static string GetCachePath(AssemblyCacheType type)
		{
			int pcchPath = 0;
			UnmanagedApi.FusionNative.GetCachePath((uint)type, null, ref pcchPath);

			var cachePath = new StringBuilder(pcchPath);
			UnmanagedApi.FusionNative.GetCachePath((uint)type, cachePath, ref pcchPath);

			return cachePath.ToString();
		}

		public static bool FindAssemblyPath(string name, out string foundPath)
		{
			string path;
			path = FindAssemblyPath(name);
			if (!string.IsNullOrEmpty(path))
			{
				foundPath = path;
				return true;
			}

			path = FindAssemblyPath(name + ", ProcessorArchitecture=msil");
			if (!string.IsNullOrEmpty(path))
			{
				foundPath = path;
				return true;
			}

			path = FindAssemblyPath(name + ", ProcessorArchitecture=x86");
			if (!string.IsNullOrEmpty(path))
			{
				foundPath = path;
				return true;
			}

			foundPath = null;
			return false;
		}

		public static string FindAssemblyPath(string name)
		{
			UnmanagedApi.IAssemblyCache cache;
			HRESULT.ThrowOnFailure(UnmanagedApi.FusionNative.CreateAssemblyCache(out cache, 0));

			var assemblyInfo = new UnmanagedApi.ASSEMBLY_INFO()
			{
				cbAssemblyInfo = Marshal.SizeOf(typeof(UnmanagedApi.ASSEMBLY_INFO)),
			};

			uint queryFlags = (uint)UnmanagedApi.QUERYASMINFO_FLAG.VALIDATE;
			cache.QueryAssemblyInfo(queryFlags, name, ref assemblyInfo);
			if (assemblyInfo.cbAssemblyInfo == 0)
				return null;

			assemblyInfo.pszCurrentAssemblyPathBuf = new string(new char[assemblyInfo.cchBuf]);
			cache.QueryAssemblyInfo(queryFlags, name, ref assemblyInfo);

			return assemblyInfo.pszCurrentAssemblyPathBuf;
		}

		public static AssemblyEnumerator EnumAssemblies()
		{
			return EnumAssemblies(null);
		}

		public static AssemblyEnumerator EnumAssemblies(AssemblyName filterName)
		{
			UnmanagedApi.IAssemblyName asmName = filterName != null ? filterName.IAssemblyName : null;
			UnmanagedApi.IAssemblyEnum asmEnum;
			UnmanagedApi.FusionNative.CreateAssemblyEnum(out asmEnum, IntPtr.Zero, asmName, (uint)UnmanagedApi.ASM_CACHE_FLAGS.GAC, IntPtr.Zero);

			return new AssemblyEnumerator(asmEnum);
		}
	}
}
