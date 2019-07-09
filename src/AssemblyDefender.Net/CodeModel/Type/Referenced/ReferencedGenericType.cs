using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedGenericType : ReferencedType
	{
		private IType _declaringType;
		private IType _baseType;
		private IReadOnlyList<IType> _genericArguments;
		private IReadOnlyList<IGenericParameter> _genericParameters;
		private IReadOnlyList<IType> _interfaces;
		private IReadOnlyList<IMethod> _methods;
		private IReadOnlyList<IField> _fields;
		private IReadOnlyList<IProperty> _properties;
		private IReadOnlyList<IEvent> _events;

		internal ReferencedGenericType(IType declaringType, IReadOnlyList<IType> genericArguments)
			: base(declaringType)
		{
			_declaringType = declaringType;
			_genericArguments = genericArguments;
		}

		public override string Name
		{
			get
			{
				string name = _declaringType.Name;
				if (string.IsNullOrEmpty(name))
					return null;

				var builder = new StringBuilder();
				builder.Append(name);

				builder.Append("<");

				for (int i = 0; i < _genericArguments.Count; i++)
				{
					builder.Append(_genericArguments[i].ToString());
				}

				builder.Append(">");

				return builder.ToString();
			}
		}

		public override string Namespace
		{
			get { return _declaringType.Namespace; }
		}

		public override bool HasGenericContext
		{
			get { return true; }
		}

		public override bool IsInterface
		{
			get { return _declaringType.IsInterface; }
		}

		public override bool IsAbstract
		{
			get { return _declaringType.IsAbstract; }
		}

		public override bool IsSealed
		{
			get { return _declaringType.IsSealed; }
		}

		public override int? PackingSize
		{
			get { return _declaringType.PackingSize; }
		}

		public override int? ClassSize
		{
			get { return _declaringType.ClassSize; }
		}

		public override TypeVisibilityFlags Visibility
		{
			get { return _declaringType.Visibility; }
		}

		public override TypeLayoutFlags Layout
		{
			get { return _declaringType.Layout; }
		}

		public override TypeCharSetFlags CharSet
		{
			get { return _declaringType.CharSet; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.GenericType; }
		}

		public override IType DeclaringType
		{
			get { return _declaringType; }
		}

		public override IType BaseType
		{
			get
			{
				if (_baseType == null)
				{
					var baseTypeSig = ((ITypeBase)_declaringType).BaseType;
					if (baseTypeSig != null)
					{
						_baseType = _assemblyManager.Resolve(baseTypeSig, this, true);
					}
				}

				return _baseType;
			}
		}

		public override IType EnclosingType
		{
			get { return _declaringType.EnclosingType; }
		}

		public override IReadOnlyList<IType> GenericArguments
		{
			get { return _genericArguments; }
		}

		public override IReadOnlyList<IGenericParameter> GenericParameters
		{
			get
			{
				if (_genericParameters == null)
				{
					LoadGenericParameters();
				}

				return _genericParameters;
			}
		}

		public override IReadOnlyList<IType> Interfaces
		{
			get
			{
				if (_interfaces == null)
				{
					LoadInterfaces();
				}

				return _interfaces;
			}
		}

		public override IReadOnlyList<IMethod> Methods
		{
			get
			{
				if (_methods == null)
				{
					LoadMethods();
				}

				return _methods;
			}
		}

		public override IReadOnlyList<IField> Fields
		{
			get
			{
				if (_fields == null)
				{
					LoadFields();
				}

				return _fields;
			}
		}

		public override IReadOnlyList<IProperty> Properties
		{
			get
			{
				if (_properties == null)
				{
					LoadProperties();
				}

				return _properties;
			}
		}

		public override IReadOnlyList<IEvent> Events
		{
			get
			{
				if (_events == null)
				{
					LoadEvents();
				}

				return _events;
			}
		}

		public override IReadOnlyList<IType> NestedTypes
		{
			get { return _declaringType.NestedTypes; }
		}

		public override ISignature ResolutionScope
		{
			get { return _declaringType.ResolutionScope; }
		}

		public override ISignature Owner
		{
			get { return _declaringType.Owner; }
		}

		public override IType GetGenericArgument(bool isMethod, int position)
		{
			if (!isMethod)
			{
				if (_genericArguments.Count > position)
				{
					return _genericArguments[position];
				}
			}

			return null;
		}

		private void LoadGenericParameters()
		{
			var declaringGenericParameters = _declaringType.GenericParameters;
			var genericParameters = new IGenericParameter[declaringGenericParameters.Count];
			for (int i = 0; i < genericParameters.Length; i++)
			{
				genericParameters[i] = new ReferencedGenericParameter(declaringGenericParameters[i], this);
			}

			_genericParameters = ReadOnlyList<IGenericParameter>.Create(genericParameters);
		}

		private void LoadInterfaces()
		{
			_interfaces = _assemblyManager.Resolve(((ITypeBase)_declaringType).Interfaces, this, true);
		}

		private void LoadMethods()
		{
			var declaringMethods = _declaringType.Methods;
			var methods = new IMethod[declaringMethods.Count];
			for (int i = 0; i < methods.Length; i++)
			{
				methods[i] = new ReferencedMethod(declaringMethods[i], this, ReadOnlyList<IType>.Empty);
			}

			_methods = ReadOnlyList<IMethod>.Create(methods);
		}

		private void LoadFields()
		{
			var declaringFields = _declaringType.Fields;
			var fields = new IField[declaringFields.Count];
			for (int i = 0; i < fields.Length; i++)
			{
				fields[i] = new ReferencedField(declaringFields[i], this);
			}

			_fields = ReadOnlyList<IField>.Create(fields);
		}

		private void LoadProperties()
		{
			var declaringProperties = _declaringType.Properties;
			var properties = new IProperty[declaringProperties.Count];
			for (int i = 0; i < properties.Length; i++)
			{
				properties[i] = new ReferencedProperty(declaringProperties[i], this);
			}

			_properties = ReadOnlyList<IProperty>.Create(properties);
		}

		private void LoadEvents()
		{
			var declaringEvents = _declaringType.Events;
			var events = new IEvent[declaringEvents.Count];
			for (int i = 0; i < events.Length; i++)
			{
				events[i] = new ReferencedEvent(declaringEvents[i], this);
			}

			_events = ReadOnlyList<IEvent>.Create(events);
		}
	}
}
