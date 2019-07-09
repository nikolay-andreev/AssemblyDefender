using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	/// <summary>
	/// Add explicit overrides to methods.
	/// </summary>
	public class ExplicitMethodOverrideBuilder
	{
		#region Fields

		private BuildAssembly _assembly;
		private MemberNameGenerator _nameGenerator;
		private BuildState _state;

		#endregion

		#region Ctors

		public ExplicitMethodOverrideBuilder(BuildAssembly assembly, MemberNameGenerator nameGenerator)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (nameGenerator == null)
				throw new ArgumentNullException("nameGenerator");

			_assembly = assembly;
			_nameGenerator = nameGenerator;
			_state = new BuildState();
		}

		#endregion

		#region Method

		public void Build()
		{
			Collect();
			Change();
		}

		#endregion

		#region Collect

		public void Collect()
		{
			foreach (var module in _assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Collect(module);
			}
		}

		private void Collect(IModule module)
		{
			foreach (IType type in module.Types)
			{
				Collect(type);
			}
		}

		private void Collect(IType type)
		{
			CollectType(type);

			foreach (IType nestedType in type.NestedTypes)
			{
				Collect(nestedType);
			}
		}

		private void CollectType(IType type)
		{
			if (type.IsInterface)
				return;

			var slots = new MethodSlotList(type);

			// Collect interfaces
			foreach (var interfaceType in type.Interfaces)
			{
				CollectInterface(slots, interfaceType);
			}

			// Collect slots
			foreach (var slot in slots)
			{
				CollectSlot(slot);
			}
		}

		private void CollectInterface(MethodSlotList slots, IType interfaceType)
		{
			var type = slots.Type;

			// Methods
			foreach (var interfaceBuildMethod in interfaceType.Methods)
			{
				var targetSlotMethod = slots.GetMethod(interfaceBuildMethod, true);

				if (targetSlotMethod.Depth == 0)
				{
					// Root type overrides interface.
					var targetBuildMethod = targetSlotMethod.Method;
					var targetMethodState = _state.GetMethod(targetBuildMethod);
					var interfaceMethodState = _state.GetMethod(interfaceBuildMethod);

					if (NeedsInterfaceOverride(interfaceMethodState, targetMethodState))
					{
						targetMethodState.Overrides.Add(interfaceBuildMethod);
					}
				}
				else
				{
					// Base type overrides interface.
					var typeState = _state.GetType(type);

					var targetBuildMethod = targetSlotMethod.Method;

					// If not overriden by root type, build proxy method and call base method.
					if (typeState.Overrides.Contains(interfaceBuildMethod))
						continue;

					// If base method could not be called from this type, skip proxy method.
					// If assembly is valid base method is always callable.
					if (!CanCallMethod(type, targetBuildMethod))
						continue;

					// Add proxy method.
					var proxy =
						new ProxyBuildMethod()
						{
							OverridenMethod = interfaceBuildMethod,
							CalledMethod = targetBuildMethod,
						};

					typeState.ProxyMethods.Add(proxy);
					typeState.Overrides.Add(interfaceBuildMethod);
				}
			}

			// Children
			foreach (var childInterfaceType in interfaceType.Interfaces)
			{
				CollectInterface(slots, childInterfaceType);
			}
		}

		private bool CanCallMethod(IType sourceType, IMethod targetMethod)
		{
			switch (targetMethod.Visibility)
			{
				case MethodVisibilityFlags.Public:
					return true;

				case MethodVisibilityFlags.Private:
					return false;

				case MethodVisibilityFlags.Family:
					return true;

				case MethodVisibilityFlags.Assembly:
				case MethodVisibilityFlags.FamAndAssem:
					return targetMethod.Assembly == sourceType.Assembly;

				default:
					return false;
			}
		}

		private void CollectSlot(MethodSlot slot)
		{
			var mainSlotMethod = slot.MainMethod;
			if (mainSlotMethod.Depth > 0)
				return;

			if (!mainSlotMethod.Method.IsVirtual)
				return;

			if (mainSlotMethod.Method.CodeType == MethodCodeTypeFlags.Runtime)
				return;

			var mainMethodState = _state.GetMethod(mainSlotMethod.Method);

			// Mark new slot.
			foreach (var slotMethod in slot)
			{
				MarkNewSlot(slotMethod);
			}

			// Collect methods.
			foreach (var slotMethod in slot)
			{
				CollectSlot(mainSlotMethod, mainMethodState, slotMethod, true);
			}
		}

		private void CollectSlot(MethodSlotEntry mainSlotMethod, MethodState mainMethodState, MethodSlotEntry slotMethod, bool isNewSlot)
		{
			if (mainSlotMethod == slotMethod)
			{
				isNewSlot = mainMethodState.IsNewSlot;
			}
			else
			{
				var methodState = _state.GetMethod(slotMethod.Method);
				var method = slotMethod.Method;

				if (isNewSlot)
				{
					// Add override to main method in slot.
					mainMethodState.Overrides.Add(method);
				}
				else
				{
					// Remove override method.
					mainMethodState.Overrides.Remove(method);
				}

				if (slotMethod.Depth == 0)
				{
					// Remove overrides of root overriden methods.
					methodState.Overrides.Clear();
				}

				isNewSlot = methodState.IsNewSlot;
			}

			// Build base methods.
			if (slotMethod.BaseMethod != null)
			{
				CollectSlot(mainSlotMethod, mainMethodState, slotMethod.BaseMethod, isNewSlot);
			}
		}

		private void MarkNewSlot(MethodSlotEntry slotMethod)
		{
			MethodState superMethodState = null;
			while (slotMethod != null)
			{
				var methodState = _state.GetMethod(slotMethod.Method);

				if (superMethodState != null && NeedsOverride(superMethodState, methodState))
				{
					superMethodState.IsNewSlot = true;
				}

				superMethodState = methodState;
				slotMethod = slotMethod.BaseMethod;
			}
		}

		private bool NeedsOverride(MethodState superMethodState, MethodState baseMethodState)
		{
			if (baseMethodState.IsRename)
			{
				if (superMethodState.IsRename)
				{
					return superMethodState.NewName != baseMethodState.NewName;
				}

				return true;
			}
			else
			{
				return superMethodState.IsRename;
			}
		}

		private bool NeedsInterfaceOverride(MethodState interfaceMethodState, MethodState targetMethodState)
		{
			if (targetMethodState.IsRename)
			{
				if (interfaceMethodState.IsRename)
				{
					if (targetMethodState.NewName == interfaceMethodState.NewName)
						return false;
				}
			}
			else
			{
				if (!interfaceMethodState.IsRename)
					return false;
			}

			return true;
		}

		#endregion

		#region Change

		private void Change()
		{
			foreach (var module in _assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				Change(module);
			}
		}

		private void Change(IModule module)
		{
			foreach (var type in module.Types)
			{
				Change(type);
			}
		}

		private void Change(IType type)
		{
			TypeState typeState;
			if (_state.Types.TryGetValue(type, out typeState))
			{
				Change(typeState);
			}

			foreach (var method in type.Methods)
			{
				Change(method);
			}

			foreach (var nestedType in type.NestedTypes)
			{
				Change(nestedType);
			}
		}

		private void Change(IMethod method)
		{
			MethodState methodState;
			if (_state.Methods.TryGetValue(method, out methodState))
			{
				Change(methodState);
			}
		}

		private void Change(TypeState typeState)
		{
			foreach (var proxyMethod in typeState.ProxyMethods)
			{
				ChangeProxyMethod((BuildType)typeState.Type, proxyMethod);
			}
		}

		private void ChangeProxyMethod(BuildType type, ProxyBuildMethod proxyMethod)
		{
			var overridenMethod = proxyMethod.OverridenMethod;
			var calledMethod = proxyMethod.CalledMethod;

			var method = type.Methods.Add();
			method.Name = _nameGenerator.GenerateUniqueString();
			method.Visibility = MethodVisibilityFlags.Private;
			method.HasThis = true;
			method.IsHideBySig = true;
			method.IsVirtual = true;
			method.IsNewSlot = true;
			method.IsFinal = true;
			method.CallConv = overridenMethod.CallConv;

			// Return type
			{
				var returnType = method.ReturnType;
				returnType.Type = overridenMethod.ReturnType.ToSignature(type.Module);
			}

			// Parameters
			foreach (var overridenMethodParameter in overridenMethod.Parameters)
			{
				var parameter = method.Parameters.Add();
				parameter.Name = overridenMethodParameter.Name;
				parameter.IsIn = overridenMethodParameter.IsIn;
				parameter.IsOut = overridenMethodParameter.IsOut;
				parameter.IsOptional = overridenMethodParameter.IsOptional;
				parameter.IsLcid = overridenMethodParameter.IsLcid;
				parameter.Type = overridenMethodParameter.Type.ToSignature(type.Module);
			}

			// GenericParameters
			foreach (var interfaceGenericParameter in overridenMethod.GenericParameters)
			{
				var genericParameter = method.GenericParameters.Add();
				genericParameter.Name = interfaceGenericParameter.Name;
				genericParameter.Variance = interfaceGenericParameter.Variance;
				genericParameter.DefaultConstructorConstraint = interfaceGenericParameter.DefaultConstructorConstraint;
				genericParameter.ReferenceTypeConstraint = interfaceGenericParameter.ReferenceTypeConstraint;
				genericParameter.ValueTypeConstraint = interfaceGenericParameter.ValueTypeConstraint;

				// Constraints
				foreach (var constraintType in interfaceGenericParameter.Constraints)
				{
					genericParameter.Constraints.Add(constraintType.ToSignature(type.Module));
				}
			}

			// Body
			{
				var methodBody = new MethodBody();

				// Instructions
				var instructions = methodBody.Instructions;

				methodBody.MaxStackSize = method.Parameters.Count + 2; // this + parameters + return

				// Load this
				instructions.Add(Instruction.GetLdarg(0));

				// Load parameters
				for (int i = 0; i < method.Parameters.Count; i++)
				{
					instructions.Add(Instruction.GetLdarg(i + 1));
				}

				var calledMethodSig = calledMethod.ToSignature(type.Module);
				if (calledMethod.GenericParameters.Count > 0)
				{
					var genericArguments = new TypeSignature[calledMethod.GenericParameters.Count];
					for (int i = 0; i < genericArguments.Length; i++)
					{
						genericArguments[i] = new GenericParameterType(true, i);
					}

					calledMethodSig = new GenericMethodReference((MethodReference)calledMethodSig, genericArguments);
				}

				// Call method
				instructions.Add(new Instruction(
					calledMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
					calledMethodSig));

				instructions.Add(new Instruction(OpCodes.Ret));

				methodBody.Build(method);
			}

			// Add override.
			method.Overrides.Add(overridenMethod.ToReference(type.Module));
		}

		private void Change(MethodState methodState)
		{
			var method = (BuildMethod)methodState.Method;

			// New slot
			method.IsNewSlot = methodState.IsNewSlot;

			// Overrides
			var overrides = methodState.Overrides;
			method.Overrides.Clear();

			foreach (var overridenMethod in overrides)
			{
				method.Overrides.Add(overridenMethod.ToReference(method.Module));
			}
		}

		#endregion

		#region Static

		public static void Build(BuildAssembly assembly, MemberNameGenerator nameGenerator)
		{
			var builder = new ExplicitMethodOverrideBuilder(assembly, nameGenerator);
			builder.Build();
		}

		#endregion

		#region Nested types

		private class BuildState
		{
			#region Fields

			private Dictionary<IType, TypeState> _types = new Dictionary<IType, TypeState>();
			private Dictionary<IMethod, MethodState> _methods = new Dictionary<IMethod, MethodState>();

			#endregion

			#region Ctors

			internal BuildState()
			{
			}

			#endregion

			#region Properties

			internal Dictionary<IType, TypeState> Types
			{
				get { return _types; }
			}

			internal Dictionary<IMethod, MethodState> Methods
			{
				get { return _methods; }
			}

			#endregion

			#region Methods

			internal TypeState GetType(IType type)
			{
				type = type.DeclaringType;

				TypeState state;
				if (!_types.TryGetValue(type, out state))
				{
					state = new TypeState(type);
					_types.Add(type, state);
				}

				return state;
			}

			internal MethodState GetMethod(IMethod method)
			{
				method = method.DeclaringMethod;

				MethodState state;
				if (!_methods.TryGetValue(method, out state))
				{
					state = new MethodState(method);
					_methods.Add(method, state);
				}

				return state;
			}

			#endregion
		}

		private class TypeState
		{
			#region Fields

			private IType _type;
			private HashSet<IMethod> _overrides;
			private HashSet<ProxyBuildMethod> _proxyMethods;

			#endregion

			#region Ctors

			internal TypeState(IType type)
			{
				_type = type;
			}

			#endregion

			#region Properties

			internal IType Type
			{
				get { return _type; }
			}

			internal HashSet<IMethod> Overrides
			{
				get
				{
					if (_overrides == null)
					{
						LoadOverrides();
					}

					return _overrides;
				}
			}

			internal HashSet<ProxyBuildMethod> ProxyMethods
			{
				get
				{
					if (_proxyMethods == null)
					{
						_proxyMethods = new HashSet<ProxyBuildMethod>();
					}

					return _proxyMethods;
				}
			}

			#endregion

			#region Methods

			private void LoadOverrides()
			{
				_overrides = new HashSet<IMethod>();

				foreach (var method in _type.Methods)
				{
					foreach (var overrideBuildMethod in method.Overrides)
					{
						_overrides.Add(overrideBuildMethod);
					}
				}
			}

			#endregion
		}

		private class MethodState
		{
			#region Fields

			private bool _isNewSlot;
			private bool _isRename;
			private bool _buildMethodLoaded;
			private string _newName;
			private IMethod _method;
			private HashSet<IMethod> _overrides;

			#endregion

			#region Ctors

			internal MethodState(IMethod method)
			{
				_method = method;
				_isNewSlot = _method.IsNewSlot;
			}

			#endregion

			#region Properties

			internal bool IsNewSlot
			{
				get { return _isNewSlot; }
				set { _isNewSlot = value; }
			}

			internal bool IsRename
			{
				get
				{
					if (!_buildMethodLoaded)
					{
						LoadBuildMethod();
					}

					return _isRename;
				}
			}

			internal string NewName
			{
				get
				{
					if (!_buildMethodLoaded)
					{
						LoadBuildMethod();
					}

					return _newName;
				}
			}

			internal IMethod Method
			{
				get { return _method; }
			}

			internal HashSet<IMethod> Overrides
			{
				get
				{
					if (_overrides == null)
					{
						LoadOverrides();
					}

					return _overrides;
				}
			}

			#endregion

			#region Methods

			private void LoadOverrides()
			{
				_overrides = new HashSet<IMethod>();

				foreach (var overrideBuildMethod in _method.Overrides)
				{
					_overrides.Add(overrideBuildMethod);
				}
			}

			private void LoadBuildMethod()
			{
				var method = _method as BuildMethod;
				if (method != null)
				{
					_isRename = method.Rename;
					_newName = method.NewName;
				}

				_buildMethodLoaded = true;
			}

			#endregion
		}

		private class ProxyBuildMethod
		{
			/// <summary>
			/// Method overrided by proxy.
			/// </summary>
			internal IMethod OverridenMethod;

			/// <summary>
			/// Base method to callvirt from proxy.
			/// </summary>
			internal IMethod CalledMethod;
		}

		#endregion
	}
}
