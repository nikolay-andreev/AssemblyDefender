using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class SuppressMessageAttribute : Attribute
	{
		#region Fields

		private string _category;
		private string _checkId;
		private string _justification;
		private string _messageId;
		private string _scope;
		private string _target;

		#endregion

		#region Ctors

		public SuppressMessageAttribute()
		{
		}

		public SuppressMessageAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public string Category
		{
			get { return _category; }
			set { _category = value; }
		}

		public string CheckId
		{
			get { return _checkId; }
			set { _checkId = value; }
		}

		public string Justification
		{
			get { return _justification; }
			set { _justification = value; }
		}

		public string MessageId
		{
			get { return _messageId; }
			set { _messageId = value; }
		}

		public string Scope
		{
			get { return _scope; }
			set { _scope = value; }
		}

		public string Target
		{
			get { return _target; }
			set { _target = value; }
		}

		#endregion

		#region Methods

		public override void Build(CustomAttribute customAttribute)
		{
			customAttribute.Constructor =
				new MethodReference(
					".ctor",
					new TypeReference(
						"SuppressMessageAttribute",
						"System.Diagnostics.CodeAnalysis",
						AssemblyReference.GetMscorlib(customAttribute.Assembly)),
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, customAttribute.Assembly),
						new TypeSignature[]
						{
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly),
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly),
						},
						0,
						0));

			// Ctor arguments
			var ctorArguments = customAttribute.CtorArguments;
			ctorArguments.Clear();

			// Category
			ctorArguments.Add(
				new CustomAttributeTypedArgument(
					_category,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly)));

			// CheckId
			ctorArguments.Add(
				new CustomAttributeTypedArgument(
					_checkId,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly)));

			// Named arguments
			var namedArguments = customAttribute.NamedArguments;
			namedArguments.Clear();

			// Justification
			if (!string.IsNullOrEmpty(_justification))
			{
				namedArguments.Add(
					new CustomAttributeNamedArgument(
						"Justification",
						CustomAttributeNamedArgumentType.Property,
						new CustomAttributeTypedArgument(
							_justification,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly))));
			}

			// MessageId
			if (!string.IsNullOrEmpty(_messageId))
			{
				namedArguments.Add(
					new CustomAttributeNamedArgument(
						"MessageId",
						CustomAttributeNamedArgumentType.Property,
						new CustomAttributeTypedArgument(
							_messageId,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly))));
			}

			// Scope
			if (!string.IsNullOrEmpty(_scope))
			{
				namedArguments.Add(
					new CustomAttributeNamedArgument(
						"Scope",
						CustomAttributeNamedArgumentType.Property,
						new CustomAttributeTypedArgument(
							_scope,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly))));
			}

			// Target
			if (!string.IsNullOrEmpty(_target))
			{
				namedArguments.Add(
					new CustomAttributeNamedArgument(
						"Target",
						CustomAttributeNamedArgumentType.Property,
						new CustomAttributeTypedArgument(
							_target,
							TypeReference.GetPrimitiveType(PrimitiveTypeCode.String, customAttribute.Assembly))));
			}
		}

		public override void Serialize(IBinaryAccessor accessor)
		{
			accessor.WriteLengthPrefixedString(_category, Encoding.Unicode);
			accessor.WriteLengthPrefixedString(_checkId, Encoding.Unicode);
			accessor.WriteLengthPrefixedString(_justification, Encoding.Unicode);
			accessor.WriteLengthPrefixedString(_messageId, Encoding.Unicode);
			accessor.WriteLengthPrefixedString(_scope, Encoding.Unicode);
			accessor.WriteLengthPrefixedString(_target, Encoding.Unicode);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_category = accessor.ReadLengthPrefixedString(Encoding.Unicode);
			_checkId = accessor.ReadLengthPrefixedString(Encoding.Unicode);
			_justification = accessor.ReadLengthPrefixedString(Encoding.Unicode);
			_messageId = accessor.ReadLengthPrefixedString(Encoding.Unicode);
			_scope = accessor.ReadLengthPrefixedString(Encoding.Unicode);
			_target = accessor.ReadLengthPrefixedString(Encoding.Unicode);
		}

		private void Load(CustomAttribute customAttribute)
		{
			switch (customAttribute.CtorArguments.Count)
			{
				case 2:
					{
						object value;

						value = customAttribute.CtorArguments[0].Value;
						if (value is string)
							_category = (string)value;

						value = customAttribute.CtorArguments[1].Value;
						if (value is string)
							_checkId = (string)value;
					}
					break;
			}

			foreach (var argument in customAttribute.NamedArguments)
			{
				if (argument.Type != CustomAttributeNamedArgumentType.Property)
					continue;

				switch (argument.Name)
				{
					case "Justification":
						{
							object value = argument.TypedValue.Value;
							if (value is string)
								_justification = (string)value;
						}
						break;

					case "MessageId":
						{
							object value = argument.TypedValue.Value;
							if (value is string)
								_messageId = (string)value;
						}
						break;

					case "Scope":
						{
							object value = argument.TypedValue.Value;
							if (value is string)
								_scope = (string)value;
						}
						break;

					case "Target":
						{
							object value = argument.TypedValue.Value;
							if (value is string)
								_target = (string)value;
						}
						break;
				}
			}
		}

		#endregion

		#region Static

		public static SuppressMessageAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new SuppressMessageAttribute(customAttribute);
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

			if (typeRef.Name != "SuppressMessageAttribute")
				return false;

			if (typeRef.Namespace != "System.Diagnostics.CodeAnalysis")
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

		public static SuppressMessageAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new SuppressMessageAttribute(customAttribute);
				}
			}

			return null;
		}

		public static SuppressMessageAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<SuppressMessageAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new SuppressMessageAttribute(customAttribute));
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
