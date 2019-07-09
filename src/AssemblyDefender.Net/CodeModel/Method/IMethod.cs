using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IMethod : ICodeNode, IMethodSignature, IMethodBase
	{
		MethodVisibilityFlags Visibility
		{
			get;
		}

		bool IsFinal
		{
			get;
		}

		bool IsVirtual
		{
			get;
		}

		bool IsHideBySig
		{
			get;
		}

		bool IsNewSlot
		{
			get;
		}

		bool IsStrict
		{
			get;
		}

		bool IsAbstract
		{
			get;
		}

		bool IsSpecialName
		{
			get;
		}

		bool IsRuntimeSpecialName
		{
			get;
		}

		bool RequireSecObject
		{
			get;
		}

		/// <summary>
		/// The code is unmanaged. This flag must be paired with the native flag.
		/// keyword: unmanaged
		/// </summary>
		bool IsUnmanaged
		{
			get;
		}

		/// <summary>
		/// The method is defined, but the IL code of the method is not supplied.
		/// This flag is used primarily in edit-and-continue scenarios and in managed
		/// object files, produced by the Visual C++ compiler. This flag should not be set
		/// for any of the methods in a managed PE file.
		/// keyword: forwardref
		/// </summary>
		bool IsForwardRef
		{
			get;
		}

		/// <summary>
		/// The method signature must not be mangled during the interoperation with classic COM code
		/// keyword: preservesig
		/// </summary>
		bool IsPreserveSig
		{
			get;
		}

		/// <summary>
		/// Reserved for internal use. This flag indicates that the method is internal to the runtime
		/// and must be called in a special way. If this flag is set, the RVA of the method must be 0.
		/// keyword: internalcall
		/// </summary>
		bool IsInternalCall
		{
			get;
		}

		/// <summary>
		/// Instruct the JIT compiler to automatically insert code to take a lock on entry to the method
		/// and release the lock on exit from the method. When an instance synchronized method is called,
		/// the lock is taken on the instance reference (the this parameter). For static methods,
		/// the lock is taken on the System.Type object associated with the class of the method.
		/// Methods belonging to value types cannot have this flag set.
		/// keyword: synchronized
		/// </summary>
		bool IsSynchronized
		{
			get;
		}

		/// <summary>
		/// The runtime is not allowed to inline the method—that is, to replace the method call with
		/// explicit insertion of the method’s IL code.
		/// </summary>
		bool IsNoInlining
		{
			get;
		}

		MethodCodeTypeFlags CodeType
		{
			get;
		}

		new IMethod DeclaringMethod
		{
			get;
		}

		new IType Owner
		{
			get;
		}

		new IType ReturnType
		{
			get;
		}

		new IReadOnlyList<IMethod> Overrides
		{
			get;
		}

		IReadOnlyList<IMethodParameter> Parameters
		{
			get;
		}

		new IReadOnlyList<IType> GenericArguments
		{
			get;
		}

		IReadOnlyList<IGenericParameter> GenericParameters
		{
			get;
		}
	}
}
