using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class AssemblyTitleAttribute : Attribute
	{
		#region Fields

		private string _title;

		#endregion

		#region Ctors

		public AssemblyTitleAttribute()
		{
		}

		public AssemblyTitleAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		#endregion

		#region Methods

		public override void Build(CustomAttribute customAttribute)
		{
			customAttribute.Constructor =
				new MethodReference(
					".ctor",
					new TypeReference(
						"AssemblyTitleAttribute",
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
					_title,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly)));

			// Named arguments
			customAttribute.NamedArguments.Clear();
		}

		public override void Serialize(IBinaryAccessor accessor)
		{
			accessor.WriteLengthPrefixedString(_title, Encoding.Unicode);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_title = accessor.ReadLengthPrefixedString(Encoding.Unicode);
		}

		private void Load(CustomAttribute customAttribute)
		{
			switch (customAttribute.CtorArguments.Count)
			{
				case 1:
					{
						object value = customAttribute.CtorArguments[0].Value;
						if (value is string)
							_title = (string)value;
					}
					break;
			}
		}

		#endregion

		#region Static

		public static AssemblyTitleAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new AssemblyTitleAttribute(customAttribute);
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

			if (typeRef.Name != "AssemblyTitleAttribute")
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

		public static AssemblyTitleAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new AssemblyTitleAttribute(customAttribute);
				}
			}

			return null;
		}

		public static AssemblyTitleAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<AssemblyTitleAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new AssemblyTitleAttribute(customAttribute));
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
