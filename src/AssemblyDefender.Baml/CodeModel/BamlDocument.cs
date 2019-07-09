using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlDocument : BamlBlock
	{
		private bool _debugBaml;
		private bool _loadAsync;
		private int _maxAsyncRecords;

		public BamlDocument()
		{
		}

		public BamlDocument(bool debugBaml, bool loadAsync, int maxAsyncRecords)
		{
			_debugBaml = debugBaml;
			_loadAsync = loadAsync;
			_maxAsyncRecords = maxAsyncRecords;
		}

		public bool DebugBaml
		{
			get { return _debugBaml; }
			set { _debugBaml = value; }
		}

		public bool LoadAsync
		{
			get { return _loadAsync; }
			set { _loadAsync = value; }
		}

		public int MaxAsyncRecords
		{
			get { return _maxAsyncRecords; }
			set { _maxAsyncRecords = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.Document; }
		}

		public override string ToString()
		{
			return string.Format("Document: DebugBaml={0}; LoadAsync={1}; MaxAsyncRecords={2}", _debugBaml, _loadAsync, _maxAsyncRecords);
		}
	}
}
