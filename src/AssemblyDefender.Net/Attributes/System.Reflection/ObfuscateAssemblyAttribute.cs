using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class ObfuscateAssemblyAttribute : Attribute
	{
		#region Fields

		private bool _assemblyIsPrivate;
		private bool _stripAfterObfuscation = true;

		#endregion

		#region Ctors

		public ObfuscateAssemblyAttribute()
		{
		}

		public ObfuscateAssemblyAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public bool AssemblyIsPrivate
		{
			get { return _assemblyIsPrivate; }
			set { _assemblyIsPrivate = value; }
		}

		public bool StripAfterObfuscation
		{
			get { return _stripAfterObfuscation; }
			set { _stripAfterObfuscation = value; }
		}

		#endregion

		#region Methods

		public override void Build(CustomAttribute customAttribute)
		{
			customAttribute.Constructor =
				new MethodReference(
					".ctor",
					new TypeReference(
						"ObfuscateAssemblyAttribute",
						"System.Reflection",
						AssemblyReference.GetMscorlib(customAttribute.Assembly)),
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, customAttribute.Assembly),
						new TypeSignature[]
						{
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly),
						},
						0,
						0));

			// Ctor arguments
			var ctorArguments = customAttribute.CtorArguments;
			ctorArguments.Clear();
			ctorArguments.Add(
				new CustomAttributeTypedArgument(
					_assemblyIsPrivate,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly)));

			// Named arguments
			var namedArguments = customAttribute.NamedArguments;
			namedArguments.Clear();

			// StripAfterObfuscation
			namedArguments.Add(
				new CustomAttributeNamedArgument(
					"StripAfterObfuscation",
					CustomAttributeNamedArgumentType.Property,
					new CustomAttributeTypedArgument(
						_stripAfterObfuscation,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly))));
		}

		public override void Serialize(IBinaryAccessor accessor)
		{
			accessor.Write((bool)_assemblyIsPrivate);
			accessor.Write((bool)_stripAfterObfuscation);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_assemblyIsPrivate = accessor.ReadBoolean();
			_stripAfterObfuscation = accessor.ReadBoolean();
		}

		private void Load(CustomAttribute customAttribute)
		{
			switch (customAttribute.CtorArguments.Count)
			{
				case 2:
					{
						object value;

						value = customAttribute.CtorArguments[0].Value;
						if (value is bool)
							_assemblyIsPrivate = (bool)value;
					}
					break;
			}

			foreach (var argument in customAttribute.NamedArguments)
			{
				if (argument.Type != CustomAttributeNamedArgumentType.Property)
					continue;

				switch (argument.Name)
				{
					case "StripAfterObfuscation":
						{
							object value = argument.TypedValue.Value;
							if (value is bool)
								_stripAfterObfuscation = (bool)value;
						}
						break;
				}
			}
		}

		#endregion

		#region Static

		public static ObfuscateAssemblyAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new ObfuscateAssemblyAttribute(customAttribute);
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

			if (typeRef.Name != "ObfuscateAssemblyAttribute")
				return false;

			if (typeRef.Namespace != "System.Reflection")
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

		public static ObfuscateAssemblyAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new ObfuscateAssemblyAttribute(customAttribute);
				}
			}

			return null;
		}

		public static ObfuscateAssemblyAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<ObfuscateAssemblyAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new ObfuscateAssemblyAttribute(customAttribute));
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
