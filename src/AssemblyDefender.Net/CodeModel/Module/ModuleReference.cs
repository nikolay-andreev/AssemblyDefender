using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ModuleReference : Signature, IModuleSignature
	{
		#region Fields

		private string _name;

		#endregion

		#region Ctors

		private ModuleReference()
		{
		}

		public ModuleReference(string name)
		{
			_name = name.NullIfEmpty();
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Module; }
		}

		#endregion

		#region Methods

		public IModule Resolve(IModule context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(this, context, throwOnFailure);
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		#endregion

		#region Static

		internal static ModuleReference LoadDef(Module module)
		{
			var image = module.Image;

			var moduleRef = image.ModuleDefSignature;
			if (moduleRef == null)
				return moduleRef;

			ModuleRow row;
			image.GetModule(1, out row);

			moduleRef = new ModuleReference();
			moduleRef._name = image.GetString(row.Name);

			module.AddSignature(ref moduleRef);
			image.ModuleDefSignature = moduleRef;

			return moduleRef;
		}

		internal static ModuleReference LoadRef(Module module, int rid)
		{
			var image = module.Image;

			var moduleRef = image.ModuleRefSignatures[rid - 1];
			if (moduleRef != null)
				return moduleRef;

			ModuleRefRow row;
			image.GetModuleRef(rid, out row);

			moduleRef = new ModuleReference();
			moduleRef._name = image.GetString(row.Name);

			module.AddSignature(ref moduleRef);
			image.ModuleRefSignatures[rid - 1] = moduleRef;

			return moduleRef;
		}

		internal static Signature LoadFile(Module module, int rid)
		{
			var image = module.Image;

			FileRow row;
			image.GetFile(rid, out row);

			var moduleRef = new ModuleReference();
			moduleRef._name = image.GetString(row.Name);

			return moduleRef;
		}

		#endregion
	}
}
