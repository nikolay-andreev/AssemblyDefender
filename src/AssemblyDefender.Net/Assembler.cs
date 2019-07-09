using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class Assembler : PEBuilder
	{
		public Assembler(Module module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			var cliFlags = CorFlags.ILOnly;
			DebugTable debugTable = null;

			var image = module.Image;
			if (image != null)
			{
				var pe = image.PE;
				if (pe != null)
				{
					Is32Bits = pe.Is32Bits;
					Characteristics = pe.Characteristics;
					DllCharacteristics = pe.DllCharacteristics;
					Machine = pe.Machine;
					ImageBase = pe.ImageBase;
					SectionAlignment = pe.SectionAlignment;
					FileAlignment = pe.FileAlignment;
					Subsystem = pe.Subsystem;
					SizeOfStackReserve = pe.SizeOfStackReserve;
					cliFlags = image.CorHeader.Flags;
					debugTable = DebugTable.TryLoad(pe);
				}
			}

			Tasks.Add(
				new ModuleBuilder()
				{
					FieldCLIDataSectionName = PESectionNames.Text,
					FieldCLIDataBlobPriority = 4000,
					MethodBodySectionName = PESectionNames.Text,
					MethodBodyBlobPriority = 5000,
					ManagedResourceSectionName = PESectionNames.Text,
					ManagedResourceBlobPriority = 9000,
					FieldDataSectionName = PESectionNames.SData,
					FieldDataBlobPriority = 4000,
					Module = module,
				},
				1000);

			Tasks.Add(
				new MetadataBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 8000,
				},
				2000);

			Tasks.Add(
				new ExportMethodBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 6000,
				},
				3000);

			Tasks.Add(
				new VTableFixupBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 12000,
					DataSectionName = PESectionNames.SData,
					DataBlobPriority = 1000,
				},
				4000);

			Tasks.Add(
				new CorMainStubBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 13000,
				},
				5000);

			Tasks.Add(
				new StrongNameSignatureBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 3000,
				},
				6000);

			Tasks.Add(
				new CorHeaderBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 2000,
					Flags = cliFlags,
				},
				7000);

			Tasks.Add(
				new ResourceBuilder()
				{
					SectionName = PESectionNames.Rsrc,
					BlobPriority = 1000,
					Table = module.UnmanagedResources,
				},
				9000);

			Tasks.Add(
				new DebugBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 7000,
					Table = debugTable,
				},
				10000);

			Tasks.Add(
				new TLSBuilder()
				{
					SectionName = PESectionNames.SData,
					BlobPriority = 3000,
				},
				11000);

			Tasks.Add(
				new ImportBuilder()
				{
					SectionName = PESectionNames.Text,
					BlobPriority = 10000,
					IATSectionName = PESectionNames.Text,
					IATBlobPriority = 1000,
				},
				12000);

			Tasks.Add(
				new ExportBuilder()
				{
					SectionName = PESectionNames.SData,
					BlobPriority = 2000,
				},
				13000);

			Tasks.Add(
				new BaseRelocationBuilder()
				{
					SectionName = PESectionNames.Reloc,
					BlobPriority = 1000,
				},
				14000);
		}

		public void InitDefaultDLL()
		{
			Is32Bits = true;
			Characteristics =
				ImageCharacteristics.EXECUTABLE_IMAGE |
				ImageCharacteristics.MACHINE_32BIT |
				ImageCharacteristics.DLL;
			DllCharacteristics =
				DllCharacteristics.DYNAMIC_BASE |
				DllCharacteristics.NX_COMPAT |
				DllCharacteristics.NO_SEH |
				DllCharacteristics.TERMINAL_SERVER_AWARE;
			Machine = MachineType.I386;
			ImageBase = 0x400000;
			SectionAlignment = 0x2000;
			FileAlignment = 0x200;
			Subsystem = SubsystemType.WINDOWS_CUI;
			SizeOfStackReserve = 0x100000;

			var cliBuilder = Tasks.Get<CorHeaderBuilder>();
			if (cliBuilder != null)
			{
				cliBuilder.Flags = CorFlags.ILOnly;
			}
		}
	}
}
