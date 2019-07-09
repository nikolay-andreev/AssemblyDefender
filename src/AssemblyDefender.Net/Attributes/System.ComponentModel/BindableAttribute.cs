using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;
using System.ComponentModel;

namespace AssemblyDefender.Net.CustomAttributes
{
	public class BindableAttribute : Attribute
	{
		#region Fields

		private bool _bindable;
		private BindingDirection? _direction;

		#endregion

		#region Ctors

		public BindableAttribute()
		{
		}

		public BindableAttribute(CustomAttribute customAttribute)
		{
			if (customAttribute == null)
				throw new ArgumentNullException("customAttribute");

			Load(customAttribute);
		}

		#endregion

		#region Properties

		public bool Bindable
		{
			get { return _bindable; }
			set { _bindable = value; }
		}

		public BindingDirection? Direction
		{
			get { return _direction; }
			set { _direction = value; }
		}

		#endregion

		#region Methods

		public override void Build(CustomAttribute customAttribute)
		{
			var arguments = new List<TypeSignature>(2);

			// Ctor arguments
			var ctorArguments = customAttribute.CtorArguments;
			ctorArguments.Clear();

			// Bindable
			arguments.Add(TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly));

			ctorArguments.Add(
				new CustomAttributeTypedArgument(
					_bindable,
					TypeReference.GetPrimitiveType(PrimitiveTypeCode.Boolean, customAttribute.Assembly)));

			// Direction
			if (_direction.HasValue)
			{
				arguments.Add(new TypeReference("BindingDirection", "System.ComponentModel", AssemblyReference.GetSystem(customAttribute.Assembly), true));

				ctorArguments.Add(
					new CustomAttributeTypedArgument(
						(int)_direction,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Int32, customAttribute.Assembly)));
			}

			// Named arguments
			customAttribute.NamedArguments.Clear();

			// Constructor
			customAttribute.Constructor =
				new MethodReference(
					".ctor",
					new TypeReference(
						"BindableAttribute",
						"System.ComponentModel",
						AssemblyReference.GetMscorlib(customAttribute.Assembly)),
					new CallSite(
						true,
						false,
						MethodCallingConvention.Default,
						TypeReference.GetPrimitiveType(PrimitiveTypeCode.Void, customAttribute.Assembly),
						ReadOnlyList<TypeSignature>.Create(arguments),
						0,
						0));
		}

		public override void Serialize(IBinaryAccessor accessor)
		{
			accessor.Write((bool)_bindable);
			accessor.Write7BitEncodedInt((int)_direction);
		}

		public override void Deserialize(IBinaryAccessor accessor)
		{
			_bindable = accessor.ReadBoolean();
			_direction = (BindingDirection)accessor.Read7BitEncodedInt();
		}

		private void Load(CustomAttribute customAttribute)
		{
			int ctorArgCount = customAttribute.CtorArguments.Count;

			if (ctorArgCount > 0)
			{
				object value = customAttribute.CtorArguments[0].Value;
				if (value != null)
				{
					if (value is bool)
					{
						_bindable = (bool)value;
					}
					else if (value is int)
					{
						_bindable = ((BindableSupport)value) != BindableSupport.No;
					}
				}
			}

			if (ctorArgCount > 1)
			{
				object value = customAttribute.CtorArguments[1].Value;
				if (value != null)
				{
					if (value is int)
					{
						_direction = (BindingDirection)value;
					}
				}
			}
		}

		#endregion

		#region Static

		public static BindableAttribute Get(CustomAttribute customAttribute)
		{
			if (Match(customAttribute))
			{
				return new BindableAttribute(customAttribute);
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

			if (typeRef.Name != "BindableAttribute")
				return false;

			if (typeRef.Namespace != "System.ComponentModel")
				return false;

			if (typeRef.Owner != null)
			{
				var assemblyRef = typeRef.Owner as AssemblyReference;
				if (assemblyRef == null)
					return false;

				if (assemblyRef.Name != "System")
					return false;
			}
			else
			{
				if (customAttribute.Assembly.Name != "System")
					return false;
			}

			return true;
		}

		public static BindableAttribute FindFirst(CustomAttributeCollection customAttributes)
		{
			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					return new BindableAttribute(customAttribute);
				}
			}

			return null;
		}

		public static BindableAttribute[] FindAll(CustomAttributeCollection customAttributes)
		{
			var list = new List<BindableAttribute>();

			foreach (var customAttribute in customAttributes)
			{
				if (Match(customAttribute))
				{
					list.Add(new BindableAttribute(customAttribute));
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
