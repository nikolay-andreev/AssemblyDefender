using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public enum SecurityAction : short
	{
		/// <summary>
		/// keyword: request
		/// </summary>
		Request = 1,

		/// <summary>
		/// All callers higher in the call stack are required to have been granted the
		/// permission specified by the current permission object.
		/// keyword: demand
		/// </summary>
		Demand = 2,

		/// <summary>
		/// The calling code can access the resource identified by the current permission
		/// object, even if callers higher in the stack have not been granted permission
		/// to access the resource.
		/// keyword: assert
		/// </summary>
		Assert = 3,

		/// <summary>
		/// The ability to access the resource specified by the current permission object
		/// is denied to callers, even if they have been granted permission to access it.
		/// keyword: deny
		/// </summary>
		Deny = 4,

		/// <summary>
		/// Only the resources specified by this permission object can be accessed, even
		/// if the code has been granted permission to access other resources.
		/// keyword: permitonly
		/// </summary>
		PermitOnly = 5,

		/// <summary>
		/// The immediate caller is required to have been granted the specified permission.
		/// keyword: linkcheck
		/// </summary>
		LinkDemand = 6,

		/// <summary>
		/// The derived class inheriting the class or overriding a method is required
		/// to have been granted the specified permission.
		/// keyword: inheritcheck
		/// </summary>
		InheritanceDemand = 7,

		/// <summary>
		/// The request for the minimum permissions required for code to run. This action
		/// can only be used within the scope of the assembly.
		/// keyword: reqmin
		/// </summary>
		RequestMinimum = 8,

		/// <summary>
		/// The request for additional permissions that are optional (not required to run).
		/// This request implicitly refuses all other permissions not specifically
		/// requested. This action can only be used within the scope of the assembly.
		/// keyword: reqopt
		/// </summary>
		RequestOptional = 9,

		/// <summary>
		/// The request that permissions that might be misused will not be granted to
		/// the calling code. This action can only be used within the scope of the assembly.
		/// keyword: reqrefuse
		/// </summary>
		RequestRefuse = 10,

		/// <summary>
		/// Reserved for implementation-specific use.
		/// Persisted grant, set at pre-JIT compilation time by the Ngen.exe utility.
		/// keyword: prejitgrant
		/// </summary>
		PreJitGrant = 11,

		/// <summary>
		/// Reserved for implementation-specific use.
		/// Persisted denial, set at pre-JIT compilation time.
		/// keyword: prejitdeny
		/// </summary>
		PreJitDeny = 12,

		/// <summary>
		/// Check that current assembly has been granted specified permission, throw SecurityException otherwise.
		/// keyword: noncasdemand
		/// </summary>
		NonCasDemand = 13,

		/// <summary>
		/// Check that immediate caller has been granted specified permission, throw SecurityException otherwise.
		/// keyword: noncaslinkdemand
		/// </summary>
		NonCasLinkDemand = 14,

		/// <summary>
		/// keyword: noncasinheritance
		/// </summary>
		NonCasInheritance = 15,
	}
}
