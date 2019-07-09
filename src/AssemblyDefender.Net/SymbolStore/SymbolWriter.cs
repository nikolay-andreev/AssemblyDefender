using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Net.SymbolStore.UnmanagedApi;

namespace AssemblyDefender.Net.SymbolStore
{
	[ComVisible(true)]
	public class SymbolWriter : ISymbolWriter
	{
		#region Fields

		private ISymUnmanagedWriter _unmanaged;

		#endregion

		#region Ctors

		public SymbolWriter(ISymUnmanagedWriter unmanaged)
		{
			if (unmanaged == null)
				throw new ArgumentNullException("unmanaged");

			_unmanaged = unmanaged;
		}

		#endregion

		#region Properties

		public ISymUnmanagedWriter Unmanaged
		{
			get { return _unmanaged; }
		}

		#endregion

		#region ISymbolWriter Members

		public void Close()
		{
			HRESULT.ThrowOnFailure(_unmanaged.Close());
		}

		public void CloseMethod()
		{
			HRESULT.ThrowOnFailure(_unmanaged.CloseMethod());
		}

		public void CloseNamespace()
		{
			HRESULT.ThrowOnFailure(_unmanaged.CloseNamespace());
		}

		public void CloseScope(int endOffset)
		{
			HRESULT.ThrowOnFailure(_unmanaged.CloseScope(endOffset));
		}

		public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			ISymUnmanagedDocumentWriter unmanagedDocWriter;
			HRESULT.ThrowOnFailure(_unmanaged.DefineDocument(
				url, language, languageVendor,
				documentType, out unmanagedDocWriter));

			return new SymbolDocumentWriter(unmanagedDocWriter);
		}

		public void DefineField(SymbolToken parent, string name, System.Reflection.FieldAttributes attributes,
			byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
			HRESULT.ThrowOnFailure(_unmanaged.DefineField(
				parent.GetToken(), name, (int)attributes,
				signature.Length, signature, (int)addrKind, addr1, addr2, addr3));
		}

		public void DefineGlobalVariable(string name, System.Reflection.FieldAttributes attributes,
			byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
			HRESULT.ThrowOnFailure(_unmanaged.DefineGlobalVariable(
				name, (int)attributes, signature.Length, signature,
				(int)addrKind, addr1, addr2, addr3));
		}

		public void DefineLocalVariable(string name, System.Reflection.FieldAttributes attributes, byte[] signature,
			SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset)
		{
			HRESULT.ThrowOnFailure(_unmanaged.DefineLocalVariable(
				name, (int)attributes, signature.Length, signature,
				(int)addrKind, addr1, addr2, addr3, startOffset, endOffset));
		}

		public void DefineParameter(string name, System.Reflection.ParameterAttributes attributes, int sequence,
			SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
			HRESULT.ThrowOnFailure(_unmanaged.DefineParameter(name,
				(int)attributes, sequence, (int)addrKind, addr1, addr2, addr3));
		}

		public void DefineSequencePoints(ISymbolDocumentWriter document, int[] offsets, int[] lines,
			int[] columns, int[] endLines, int[] endColumns)
		{
			var symDoc = document as SymbolDocumentWriter;
			if (symDoc == null)
			{
				throw new InvalidOperationException();
			}

			HRESULT.ThrowOnFailure(_unmanaged.DefineSequencePoints(
				symDoc.Unmanaged, offsets.Length, offsets,
				lines, columns, endLines, endColumns));
		}

		public void Initialize(IntPtr emitter, string filename, bool fFullBuild)
		{
			object objEmitter = Marshal.GetObjectForIUnknown(emitter);
			HRESULT.ThrowOnFailure(_unmanaged.Initialize(objEmitter, filename, null, fFullBuild));
		}

		public void OpenMethod(SymbolToken method)
		{
			HRESULT.ThrowOnFailure(_unmanaged.OpenMethod(method.GetToken()));
		}

		public void OpenNamespace(string name)
		{
			HRESULT.ThrowOnFailure(_unmanaged.OpenNamespace(name));
		}

		public int OpenScope(int startOffset)
		{
			int value;
			HRESULT.ThrowOnFailure(_unmanaged.OpenScope(startOffset, out value));

			return value;
		}

		public void SetMethodSourceRange(ISymbolDocumentWriter startDoc, int startLine, int startColumn, ISymbolDocumentWriter endDoc, int endLine, int endColumn)
		{
			var symStartDoc = startDoc as SymbolDocumentWriter;
			if (symStartDoc == null)
			{
				throw new InvalidOperationException();
			}

			var symEndDoc = endDoc as SymbolDocumentWriter;
			if (symEndDoc == null)
			{
				throw new InvalidOperationException();
			}

			HRESULT.ThrowOnFailure(_unmanaged.SetMethodSourceRange(
				symStartDoc.Unmanaged, startLine, startColumn,
				symEndDoc.Unmanaged, endLine, endColumn));
		}

		public void SetScopeRange(int scopeID, int startOffset, int endOffset)
		{
			HRESULT.ThrowOnFailure(_unmanaged.SetScopeRange(scopeID, startOffset, endOffset));
		}

		public void SetSymAttribute(SymbolToken parent, string name, byte[] data)
		{
			HRESULT.ThrowOnFailure(_unmanaged.SetSymAttribute(
				parent.GetToken(), name, data.Length, data));
		}

		public void SetUnderlyingWriter(IntPtr underlyingWriter)
		{
			_unmanaged = (ISymUnmanagedWriter)Marshal.GetObjectForIUnknown(underlyingWriter);
		}

		public void SetUserEntryPoint(SymbolToken entryMethod)
		{
			HRESULT.ThrowOnFailure(_unmanaged.SetUserEntryPoint(entryMethod.GetToken()));
		}

		public void UsingNamespace(string fullName)
		{
			HRESULT.ThrowOnFailure(_unmanaged.UsingNamespace(fullName));
		}

		#endregion
	}
}
