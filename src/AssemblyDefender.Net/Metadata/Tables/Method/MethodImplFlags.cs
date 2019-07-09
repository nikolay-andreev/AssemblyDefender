using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Specifies flags for the attributes of a method implementation.
	/// </summary>
	public static class MethodImplFlags
	{
		#region Code type (mask 0x0003)

		public const int CodeTypeMask = 0x0003;

		/// <summary>
		/// The default. The method is implemented in common intermediate language (CIL, a.k.a. IL or MSIL).
		/// keyword: cil
		/// </summary>
		public const int CIL = 0;

		/// <summary>
		/// The method is implemented in native platform-specific code.
		/// keyword: native
		/// </summary>
		public const int Native = 0x1;

		/// <summary>
		/// The method is implemented in optimized IL. The optimized IL is not supported in existing releases
		/// of the common language runtime, so this flag should not be set.
		/// keyword: optil
		/// </summary>
		public const int OptimizedIL = 0x2;

		/// <summary>
		/// The method implementation is automatically generated by the runtime itself.
		/// Only certain methods from the base class library (Mscorlib.dll) carry this flag.
		/// If this flag is set, the RVA of the method must be 0.
		/// keyword: runtime
		/// </summary>
		public const int Runtime = 0x3;

		#endregion

		#region Code management (mask 0x0004)

		public const int CodeManagementMask = 0x0004;

		/// <summary>
		/// The default. The code is managed. In the existing releases of the runtime,
		/// this flag cannot be paired with the native flag.
		/// keyword: managed
		/// </summary>
		public const int Managed = 0;

		/// <summary>
		/// The code is unmanaged. This flag must be paired with the native flag.
		/// keyword: unmanaged
		/// </summary>
		public const int Unmanaged = 0x4;

		#endregion

		#region Implementation and interoperability

		/// <summary>
		/// The method is defined, but the IL code of the method is not supplied.
		/// This flag is used primarily in edit-and-continue scenarios and in managed
		/// object files, produced by the Visual C++ compiler. This flag should not be set
		/// for any of the methods in a managed PE file.
		/// keyword: forwardref
		/// </summary>
		public const int ForwardRef = 0x10;

		/// <summary>
		/// The method signature must not be mangled during the interoperation with classic COM code
		/// keyword: preservesig
		/// </summary>
		public const int PreserveSig = 0x80;

		/// <summary>
		/// Reserved for internal use. This flag indicates that the method is internal to the runtime
		/// and must be called in a special way. If this flag is set, the RVA of the method must be 0.
		/// keyword: internalcall
		/// </summary>
		public const int InternalCall = 0x1000;

		/// <summary>
		/// Instruct the JIT compiler to automatically insert code to take a lock on entry to the method
		/// and release the lock on exit from the method. When an instance synchronized method is called,
		/// the lock is taken on the instance reference (the this parameter). For static methods,
		/// the lock is taken on the System.Type object associated with the class of the method.
		/// Methods belonging to value types cannot have this flag set.
		/// keyword: synchronized
		/// </summary>
		public const int Synchronized = 0x20;

		/// <summary>
		/// The runtime is not allowed to inline the method—that is, to replace the method call with
		/// explicit insertion of the method’s IL code.
		/// </summary>
		public const int NoInlining = 0x8;

		#endregion
	}
}