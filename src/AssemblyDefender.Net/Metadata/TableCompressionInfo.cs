using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Represents compression information for existing metadata.
	/// </summary>
	public class TableCompressionInfo
	{
		/// <summary>
		/// Gets compression info for string heap pointer size.
		/// </summary>
		public bool StringHeapOffsetSize4;

		/// <summary>
		/// Gets compression info for guid heap pointer size.
		/// </summary>
		public bool GuidHeapOffsetSize4;

		/// <summary>
		/// Gets compression info for blob heap pointer size.
		/// </summary>
		public bool BlobHeapOffsetSize4;

		/// <summary>
		/// Gets compression info for table row index size.
		/// </summary>
		public bool[] TableRowIndexSize4 = new bool[MetadataConstants.TableCount];

		/// <summary>
		/// Gets compression info for coded token data size.
		/// </summary>
		public bool[] CodedTokenDataSize4 = new bool[MetadataConstants.CodedTokenCount];

		public static TableCompressionInfo Create(MetadataScope metadata)
		{
			var info = new TableCompressionInfo();

			var tables = metadata.Tables;

			int[] rowCounts = new int[MetadataConstants.TableCount];
			for (int i = 0; i < MetadataConstants.TableCount; i++)
			{
				rowCounts[i] = tables[i].Count;
			}

			// Heap offset sizes.
			LoadHeapOffsetSizes(info, metadata);

			// Table row index sizes.
			LoadTableIndexSizes(info, metadata, rowCounts);

			// Coded token data sizes.
			LoadCodedTokenDataSizes(info, metadata, rowCounts);

			return info;
		}

		public static TableCompressionInfo Create(MetadataScope metadata, int[] rowCounts, byte heapFlags)
		{
			var info = new TableCompressionInfo();

			var tables = metadata.Tables;

			// Heap offset sizes.
			info.StringHeapOffsetSize4 =
				((heapFlags & HeapOffsetFlags.StringHeap4) == HeapOffsetFlags.StringHeap4);

			info.GuidHeapOffsetSize4 =
				((heapFlags & HeapOffsetFlags.GuidHeap4) == HeapOffsetFlags.GuidHeap4);

			info.BlobHeapOffsetSize4 =
				((heapFlags & HeapOffsetFlags.BlobHeap4) == HeapOffsetFlags.BlobHeap4);

			// Table row index sizes.
			LoadTableIndexSizes(info, metadata, rowCounts);

			// Coded token data sizes.
			LoadCodedTokenDataSizes(info, metadata, rowCounts);

			return info;
		}

		private static void LoadHeapOffsetSizes(TableCompressionInfo info, MetadataScope metadata)
		{
			info.StringHeapOffsetSize4 = (metadata.Strings.Length >= (1 << 16));
			info.GuidHeapOffsetSize4 = (metadata.Guids.Length >= (1 << 16));
			info.BlobHeapOffsetSize4 = (metadata.Blobs.Length >= (1 << 16));
		}

		private static void LoadTableIndexSizes(TableCompressionInfo info, MetadataScope metadata, int[] rowCounts)
		{
			for (int i = 0; i < MetadataConstants.TableCount; i++)
			{
				info.TableRowIndexSize4[i] = (rowCounts[i] >= (1 << 16));
			}
		}

		private static void LoadCodedTokenDataSizes(TableCompressionInfo info, MetadataScope metadata, int[] rowCounts)
		{
			for (int i = 0; i < MetadataConstants.CodedTokenCount; i++)
			{
				int maxRowCount = 0;
				int codedTokenType = i + CodedTokenType.TypeDefOrRef;
				var token = CodedTokenInfo.Get(codedTokenType);

				for (int j = 0; j < token.TokenTypes.Length; j++)
				{
					int tableType = MetadataToken.GetTableTypeByTokenType(token.TokenTypes[j]);

					int rowCount = rowCounts[tableType];
					if (maxRowCount < rowCount)
						maxRowCount = rowCount;
				}

				info.CodedTokenDataSize4[i] = (maxRowCount >= (1 << (16 - token.Tag)));
			}
		}
	}
}
