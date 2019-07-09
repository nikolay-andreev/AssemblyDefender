using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class AssemblyDescriptionAttribute : Attribute
	{
		#region Fields

		private string _description;

		#endregion

		#region Ctors

		public AssemblyDescriptionAttribute()
		{
		}

		public AssemblyDescriptionAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		#endregion

		#region Methods

		public override void Build(CustomAttribute customAttribute)
		{
			customAttribute.Constructor =
				new MethodReference(
					".ctor",
					new TypeReference(
						"AssemblyDescriptionAttribute",
						"System.Reflection",
						AssemblyReference.GetMscorlib(customAttribute.Assembly)),
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, customAttribute.Assembly),
						new TypeSignature[]
						{
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly),
						},
						0,
						0));

			// Ctor arguments
			var ctorArguments = customAttribute.CtorArguments;
			ctorArguments.Clear();
			ctorArguments.Add(
				new CustomAttributeTypedArgument(
					_description,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly)));

			// Named arguments
			customAttribute.NamedArguments.Clear();
		}

		public override void Serialize(IBinaryAccessor accessor)
		{
			accessor.WriteLengthPrefixedString(_description, Encoding.Unicode);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_description = accessor.ReadLengthPrefixedString(Encoding.Unicode);
		}

		private void Load(CustomAttribute customAttribute)
		{
			switch (customAttribute.CtorArguments.Count)
			{
				case 1:
					{
						object value = customAttribute.CtorArguments[0].Value;
						if (value is string)
							_description = (string)value;
					}
					break;
			}
		}

		#endregion

		#region Static

		public static AssemblyDescriptionAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new AssemblyDescriptionAttribute(customAttribute);
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

			if (typeRef.Name != "AssemblyDescriptionAttribute")
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

		public static AssemblyDescriptionAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new AssemblyDescriptionAttribute(customAttribute);
				}
			}

			return null;
		}

		public static AssemblyDescriptionAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<AssemblyDescriptionAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new AssemblyDescriptionAttribute(customAttribute));
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
