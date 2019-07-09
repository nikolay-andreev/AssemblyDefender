using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class ObfuscationAttribute : Attribute
	{
		#region Fields

		private bool _applyToMembers = true;
		private bool _exclude = true;
		private string _feature = "all";
		private bool _stripAfterObfuscation = true;

		#endregion

		#region Ctors

		public ObfuscationAttribute()
		{
		}

		public ObfuscationAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public bool ApplyToMembers
		{
			get { return _applyToMembers; }
			set { _applyToMembers = value; }
		}

		public bool Exclude
		{
			get { return _exclude; }
			set { _exclude = value; }
		}

		public string Feature
		{
			get { return _feature; }
			set { _feature = value; }
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
						"ObfuscationAttribute",
						"System.Reflection",
						AssemblyReference.GetMscorlib(customAttribute.Assembly)),
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, customAttribute.Assembly),
						ReadOnlyList<TypeSignature>.Empty,
						0,
						0));

			// Named arguments
			var namedArguments = customAttribute.NamedArguments;
			namedArguments.Clear();

			// ApplyToMembers
			namedArguments.Add(
				new CustomAttributeNamedArgument(
					"ApplyToMembers",
					CustomAttributeNamedArgumentType.Property,
					new CustomAttributeTypedArgument(
						_applyToMembers,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly))));

			// Exclude
			namedArguments.Add(
				new CustomAttributeNamedArgument(
					"Exclude",
					CustomAttributeNamedArgumentType.Property,
					new CustomAttributeTypedArgument(
						_exclude,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly))));

			// Feature
			namedArguments.Add(
				new CustomAttributeNamedArgument(
					"Feature",
					CustomAttributeNamedArgumentType.Property,
					new CustomAttributeTypedArgument(
						_feature,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly))));

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
			accessor.Write((bool)_applyToMembers);
			accessor.Write((bool)_exclude);
			accessor.WriteLengthPrefixedString(_feature, Encoding.Unicode);
			accessor.Write((bool)_stripAfterObfuscation);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_applyToMembers = accessor.ReadBoolean();
			_exclude = accessor.ReadBoolean();
			_feature = accessor.ReadLengthPrefixedString(Encoding.Unicode);
			_stripAfterObfuscation = accessor.ReadBoolean();
		}

		private void Load(CustomAttribute customAttribute)
		{
			foreach (var argument in customAttribute.NamedArguments)
			{
				if (argument.Type != CustomAttributeNamedArgumentType.Property)
					continue;

				switch (argument.Name)
				{
					case "ApplyToMembers":
						{
							object value = argument.TypedValue.Value;
							if (value is bool)
								_applyToMembers = (bool)value;
						}
						break;

					case "Exclude":
						{
							object value = argument.TypedValue.Value;
							if (value is bool)
								_exclude = (bool)value;
						}
						break;

					case "Feature":
						{
							object value = argument.TypedValue.Value;
							if (value is string)
								_feature = (string)value;
						}
						break;

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

		public static ObfuscationAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new ObfuscationAttribute(customAttribute);
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

			if (typeRef.Name != "ObfuscationAttribute")
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

		public static ObfuscationAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new ObfuscationAttribute(customAttribute);
				}
			}

			return null;
		}

		public static ObfuscationAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<ObfuscationAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new ObfuscationAttribute(customAttribute));
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

		public static void RemoveMarkedAsStrip(CustomAttributeCollection customAttributes)
		{
			var attribute = new ObfuscationAttribute();

			for (int i = customAttributes.Count - 1; i >= 0; i--)
			{
				if (Match(customAttributes[i]))
				{
					attribute.Load(customAttributes[i]);
					
					if (attribute.StripAfterObfuscation)
					{
						customAttributes.RemoveAt(i);
					}
				}
			}
		}

		#endregion
	}
}
