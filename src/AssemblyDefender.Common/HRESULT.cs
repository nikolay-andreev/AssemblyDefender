using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common
{
	/// <summary>
	/// HRESULT is the data type used to indicate the status of operations in Microsoft's components.
	/// An HRESULT value has 32 bits divided into three fields: a severity code, a facility code, and an error code.
	/// The severity code indicates whether the return value represents information, warning, or error.
	/// The facility code identifies the area of the system responsible for the error. The error code is a
	/// unique number that is assigned to represent the exception. Each exception is mapped to a distinct HRESULT.
	/// When managed code throws an exception, the runtime passes the HRESULT to the COM client. When unmanaged code
	/// returns an error, the HRESULT is converted to an exception, which is then thrown by the runtime.
	/// </summary>
	public static class HRESULT
	{
		#region Error Codes

		/// <summary>
		/// Generic HRESULT for success.
		/// </summary>
		public const int S_OK = 0;

		/// <summary>
		/// HRESULT for false.
		/// </summary>
		public const int S_FALSE = 1;

		/// <summary>
		/// A return value that may indicate an explicit cancellation action or some process that could no longer
		/// proceed after (for instance) both undo and rollback failed.
		/// </summary>
		public const int E_ABORT = -2147467260;

		/// <summary>
		/// A return value that describes a general access denied error.
		/// </summary>
		public const int E_ACCESSDENIED = -2147024891;

		/// <summary>
		/// Error HRESULT for a generic failure.
		/// </summary>
		public const int E_FAIL = -2147467259;

		/// <summary>
		/// A return value that indicates an invalid handle.
		/// </summary>
		public const int E_HANDLE = -2147024890;

		/// <summary>
		/// Error HRESULT for an invalid argument.
		/// </summary>
		public const int E_INVALIDARG = -2147024809;

		/// <summary>
		/// Error HRESULT for the request of a not implemented interface.
		/// </summary>
		public const int E_NOINTERFACE = -2147467262;

		/// <summary>
		/// Error HRESULT for the call to a method that is not implemented.
		/// </summary>
		public const int E_NOTIMPL = -2147467263;

		/// <summary>
		/// Error HRESULT for out of memory.
		/// </summary>
		public const int E_OUTOFMEMORY = -2147024882;

		/// <summary>
		/// A return value that indicates the availability of an asynchronously accessed interface.
		/// </summary>
		public const int E_PENDING = -2147483638;

		/// <summary>
		/// A return value that indicates that an invalid pointer, usually null, was passed as a parameter.
		/// </summary>
		public const int E_POINTER = -2147467261;

		/// <summary>
		/// A return value that indicates that the result of the method call is outside of the error cases the
		/// client code can readily handle.
		/// </summary>
		public const int E_UNEXPECTED = -2147418113;

		#endregion

		/// <summary>
		/// Checks if an HRESULT is an error return code.
		/// </summary>
		/// <returns>true if <paramref name="hr" /> represents an error, otherwise false.
		/// </returns>
		/// <param name="hr">
		/// The HRESULT to test.
		/// </param>
		public static bool Failed(int hr)
		{
			return (hr < 0);
		}

		/// <summary>
		/// Checks if an HRESULT is a success return code.
		/// </summary>
		/// <returns>true if <paramref name="hr" /> represents a success otherwise false.
		/// </returns>
		/// <param name="hr">
		/// The HRESULT to test.
		/// </param>
		public static bool Succeeded(int hr)
		{
			return (hr >= 0);
		}

		/// <summary>
		/// Checks if the parameter is a success or failure HRESULT and throws an exception in case of failure.
		/// </summary>
		/// <returns>
		/// The HRESULT.
		/// </returns>
		/// <param name="hr">
		/// The HRESULT to test.
		/// </param>
		public static int ThrowOnFailure(int hr)
		{
			return ThrowOnFailure(hr, null);
		}

		/// <summary>
		/// Checks if the parameter is a success or failure HRESULT and throws an exception if it is a failure that is not included in the array of well-known failures.
		/// </summary>
		/// <returns>
		/// The HRESULT.
		/// </returns>
		/// <param name="hr">
		/// The HRESULT to test.
		/// </param>
		/// <param name="expectedHRFailure">
		/// If <paramref name="hr" /> is found in this array of expected failures no exception should be thrown.
		/// </param>
		public static int ThrowOnFailure(int hr, params int[] expectedHRFailure)
		{
			if (Failed(hr) && ((expectedHRFailure == null) || (Array.IndexOf<int>(expectedHRFailure, hr) < 0)))
			{
				Marshal.ThrowExceptionForHR(hr);
			}

			return hr;
		}
	}
}
