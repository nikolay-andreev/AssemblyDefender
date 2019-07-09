using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class ILBody : ILBlock
	{
		#region Fields

		private int _maxStackSize = MethodBody.DefaultMaxStackSize;
		private bool _initLocals = true;
		private List<TypeSignature> _localVariables = new List<TypeSignature>();

		#endregion

		#region Ctors

		public ILBody()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the maximum number of items on the operand stack when the method is executing.
		/// </summary>
		public int MaxStackSize
		{
			get { return _maxStackSize; }
			set { _maxStackSize = value; }
		}

		/// <summary>
		/// Gets a value indicating whether local variables in the method body are initialized to the
		/// default values for their types.
		/// </summary>
		public bool InitLocals
		{
			get { return _initLocals; }
			set { _initLocals = value; }
		}

		public List<TypeSignature> LocalVariables
		{
			get
			{
				if (_localVariables == null)
				{
					_localVariables = new List<TypeSignature>();
				}

				return _localVariables;
			}
			set { _localVariables = value; }
		}

		public override ILBlockType BlockType
		{
			get { return ILBlockType.Body; }
		}

		#endregion

		#region Methods

		public MethodBody Build()
		{
			var builder = new ILBuilder(this);
			return builder.Build();
		}

		public void CalculateMaxStackSize(MethodDeclaration method)
		{
			var calculator = new StackSizeCalculator(this, method);
			calculator.Calculate();

			_maxStackSize = calculator.MaxStackSize;
		}

		#endregion

		#region Static

		public static ILBody Load(MethodBody methodBody)
		{
			if (methodBody == null)
				throw new ArgumentNullException("methodBody");

			var loader = new ILLoader(methodBody);
			return loader.Load();
		}

		#endregion
	}
}
