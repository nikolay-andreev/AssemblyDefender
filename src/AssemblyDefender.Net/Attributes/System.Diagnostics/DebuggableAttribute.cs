using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class DebuggableAttribute : Attribute
	{
		#region Fields

		private int _debuggingModes;

		#endregion

		#region Ctors

		public DebuggableAttribute()
		{
		}

		public DebuggableAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public int DebuggingModes
		{
			get { return _debuggingModes; }
			set { _debuggingModes = value; }
		}

		#endregion

		#region Methods

		public override void Build(CustomAttribute customAttribute)
		{
			customAttribute.Constructor =
				new MethodReference(
					".ctor",
					new TypeReference(
						"DebuggableAttribute",
						"System.Diagnostics",
						AssemblyReference.GetMscorlib(customAttribute.Assembly)),
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, customAttribute.Assembly),
						new TypeSignature[]
						{
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, customAttribute.Assembly),
						},
						0,
						0));

			// Ctor arguments
			var ctorArguments = customAttribute.CtorArguments;
			ctorArguments.Clear();
			ctorArguments.Add(
				new CustomAttributeTypedArgument(
					_debuggingModes,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, customAttribute.Assembly)));

			// Named arguments
			customAttribute.NamedArguments.Clear();
		}

		public override void Serialize(IBinaryAccessor accessor)
		{
			accessor.Write7BitEncodedInt(_debuggingModes);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_debuggingModes = accessor.Read7BitEncodedInt();
		}

		private void Load(CustomAttribute customAttribute)
		{
			switch (customAttribute.CtorArguments.Count)
			{
				case 1:
					{
						object value = customAttribute.CtorArguments[0].Value;
						if (value is int)
							_debuggingModes = (int)value;
					}
					break;
			}
		}

		#endregion

		#region Static

		public static DebuggableAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new DebuggableAttribute(customAttribute);
			}

			return null;
		}

		public static bool Match(CustomAttribute customAttribute)
		{
			if (customAttribute.Constructor == null)
				return false;

			var typeRef = customAttribute.Constructor.Owner as TypeReference;
			if (typeRef == null)
				return false;

			if (typeRef.Name != "DebuggableAttribute")
				return false;

			if (typeRef.Namespace != "System.Diagnostics")
				return false;

			if (typeRef.Owner != null)
			{
				var assemblyRef = typeRef.Owner as AssemblyReference;
				if (assemblyRef == null)
					return false;

				if (assemblyRef.Name != "mscorlib")
					return false;
			}
			else
			{
				if (customAttribute.Assembly.Name != "mscorlib")
					return false;
			}

			return true;
		}

		public static DebuggableAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new DebuggableAttribute(customAttribute);
				}
			}

			return null;
		}

		public static DebuggableAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<DebuggableAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new DebuggableAttribute(customAttribute));
				}
			}

			return list.ToArray();
		}

		public static void Clear(CustomAttributeCollection customAttributes)
		{
			for (int i = customAttributes.Count - 1; i >= 0; i--)
			{
				if (Match(customAttributes[i]))
				{
					customAttributes.RemoveAt(i);
				}
			}
		}

		#endregion
	}
}
