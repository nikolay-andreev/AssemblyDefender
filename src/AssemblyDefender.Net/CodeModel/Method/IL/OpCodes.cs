using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	public static class OpCodes
	{
		#region Primitive

		/// <summary>
		/// Fills space if opcodes are patched. No meaningful operation is performed
		/// although a processing cycle can be consumed.
		/// </summary>
		public static readonly OpCode Nop = new OpCode(
			"nop", 0xFF, 0x0,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Signals the Common Language Infrastructure (CLI) to inform the debugger that
		/// a break point has been tripped.
		/// </summary>
		public static readonly OpCode Break = new OpCode(
			"break", 0xFF, 0x1,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Break,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Copies the current topmost value on the evaluation stack, and then pushes
		/// the copy onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Dup = new OpCode(
			"dup", 0xFF, 0x25,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push1_push1, 1);

		/// <summary>
		/// Removes the value currently on top of the evaluation stack.
		/// </summary>
		public static readonly OpCode Pop = new OpCode(
			"pop", 0xFF, 0x26,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Copies a specified number bytes from a source address to a destination address.
		/// </summary>
		public static readonly OpCode Cpblk = new OpCode(
			"cpblk", 0xFE, 0x17,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi_popi, StackBehavior.Push0, -3);

		/// <summary>
		/// Initializes a specified block of memory at a specific address to a given
		/// size and initial value.
		/// </summary>
		public static readonly OpCode Initblk = new OpCode(
			"initblk", 0xFE, 0x18,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi_popi, StackBehavior.Push0, -3);

		/// <summary>
		/// Allocates a certain number of bytes from the local dynamic memory pool and
		/// pushes the address (a transient pointer, type *) of the first allocated byte
		/// onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Localloc = new OpCode(
			"localloc", 0xFE, 0xF,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Returns an unmanaged pointer to the argument list of the current method.
		/// </summary>
		public static readonly OpCode Arglist = new OpCode(
			"arglist", 0xFE, 0x0,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Throws System.ArithmeticException if value is not a finite number.
		/// </summary>
		public static readonly OpCode Ckfinite = new OpCode(
			"ckfinite", 0xFF, 0xC3,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushr8, 0);

		/// <summary>
		/// Pushes a null reference (type O) onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldnull = new OpCode(
			"ldnull", 0xFF, 0x14,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushref, 1);

		/// <summary>
		/// Converts a metadata token to its runtime representation, pushing it onto
		/// the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldtoken = new OpCode(
			"ldtoken", 0xFF, 0xD0,
			OpCodeType.Primitive, OperandType.InlineTok, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the size, in bytes, of a supplied value type onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Sizeof = new OpCode(
			"sizeof", 0xFE, 0x1C,
			OpCodeType.Primitive, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes an object reference to a new zero-based, one-dimensional array whose
		/// elements are of a specific type onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Newarr = new OpCode(
			"newarr", 0xFF, 0x8D,
			OpCodeType.Primitive, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushref, 0);

		/// <summary>
		/// Pushes the number of elements of a zero-based, one-dimensional array onto
		/// the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldlen = new OpCode(
			"ldlen", 0xFF, 0x8E,
			OpCodeType.Primitive, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Pushi, 0);

		#endregion

		#region Prefix

		/// <summary>
		/// Constrains the type on which a virtual method call is made.
		/// </summary>
		public static readonly OpCode ConstrainedPrefix = new OpCode(
			"constrained.", 0xFE, 0x16,
			OpCodeType.Prefix, OperandType.InlineType, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Indicates that the subsequent instruction need not perform the specified fault check when it is
		/// executed. The byte that follows the instruction code indicates which checks can optionally be skipped.
		/// </summary>
		public static readonly OpCode NoPrefix = new OpCode(
			"no.", 0xFE, 0x19,
			OpCodeType.Prefix, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Specifies that the subsequent array address operation performs no type check
		/// at run time, and that it returns a managed pointer whose mutability is restricted.
		/// </summary>
		public static readonly OpCode ReadonlyPrefix = new OpCode(
			"readonly.", 0xFE, 0x1E,
			OpCodeType.Prefix, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Performs a postfixed method call instruction such that the current method's
		/// stack frame is removed before the actual call instruction is executed.
		/// </summary>
		public static readonly OpCode TailPrefix = new OpCode(
			"tail.", 0xFE, 0x14,
			OpCodeType.Prefix, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Indicates that an address currently atop the evaluation stack might not be
		/// aligned to the natural size of the immediately following ldind, stind, ldfld,
		/// stfld, ldobj, stobj, initblk, or cpblk instruction.
		/// </summary>
		public static readonly OpCode UnalignedPrefix = new OpCode(
			"unaligned.", 0xFE, 0x12,
			OpCodeType.Prefix, OperandType.ShortInlineI, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Specifies that an address currently atop the evaluation stack might be volatile,
		/// and the results of reading that location cannot be cached or that multiple
		/// stores to that location cannot be suppressed.
		/// </summary>
		public static readonly OpCode VolatilePrefix = new OpCode(
			"volatile.", 0xFE, 0x13,
			OpCodeType.Prefix, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		#endregion

		#region Reserved

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix1 = new OpCode(
			"prefix1", 0xFF, 0xFE,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix2 = new OpCode(
			"prefix2", 0xFF, 0xFD,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix3 = new OpCode(
			"prefix3", 0xFF, 0xFC,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix4 = new OpCode(
			"prefix4", 0xFF, 0xFB,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix5 = new OpCode(
			"prefix5", 0xFF, 0xFA,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix6 = new OpCode(
			"prefix6", 0xFF, 0xF9,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefix7 = new OpCode(
			"prefix7", 0xFF, 0xF8,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// This is a reserved instruction.
		/// </summary>
		public static readonly OpCode Prefixref = new OpCode(
			"prefixref", 0xFF, 0xFF,
			OpCodeType.Reserved, OperandType.InlineNone, FlowControl.Meta,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		#endregion

		#region Object

		/// <summary>
		/// Copies the value type object pointed to by an address to the top of the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Ldobj = new OpCode(
			"ldobj", 0xFF, 0x71,
			OpCodeType.Object, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Push1, 0);

		/// <summary>
		/// Copies a value of a specified type from the evaluation stack into a supplied
		/// memory address.
		/// </summary>
		public static readonly OpCode Stobj = new OpCode(
			"stobj", 0xFF, 0x81,
			OpCodeType.Object, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popi_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Copies the value type located at the address of an object (type &, * or native
		/// int) to the address of the destination object (type &, * or native int).
		/// </summary>
		public static readonly OpCode Cpobj = new OpCode(
			"cpobj", 0xFF, 0x70,
			OpCodeType.Object, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popi_popi, StackBehavior.Push0, -2);

		/// <summary>
		/// Initializes each field of the value type at a specified address to a null
		/// reference or a 0 of the appropriate primitive type.
		/// </summary>
		public static readonly OpCode Initobj = new OpCode(
			"initobj", 0xFE, 0x15,
			OpCodeType.Object, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		/// <summary>
		/// Tests whether an object reference (type O) is an instance of a particular
		/// class.
		/// </summary>
		public static readonly OpCode Isinst = new OpCode(
			"isinst", 0xFF, 0x75,
			OpCodeType.Object, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Pushi, 0);

		/// <summary>
		/// Attempts to cast an object passed by reference to the specified class.
		/// </summary>
		public static readonly OpCode Castclass = new OpCode(
			"castclass", 0xFF, 0x74,
			OpCodeType.Object, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Pushref, 0);

		/// <summary>
		/// Pushes a new object reference to a string literal stored in the metadata.
		/// </summary>
		public static readonly OpCode Ldstr = new OpCode(
			"ldstr", 0xFF, 0x72,
			OpCodeType.Object, OperandType.InlineString, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushref, 1);

		#endregion

		#region TypedRef

		/// <summary>
		/// Pushes a typed reference to an instance of a specific type onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Mkrefany = new OpCode(
			"mkrefany", 0xFF, 0xC6,
			OpCodeType.TypedRef, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Push1, 0);

		/// <summary>
		/// Retrieves the type token embedded in a typed reference.
		/// </summary>
		public static readonly OpCode Refanytype = new OpCode(
			"refanytype", 0xFE, 0x1D,
			OpCodeType.TypedRef, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Retrieves the address (type &) embedded in a typed reference.
		/// </summary>
		public static readonly OpCode Refanyval = new OpCode(
			"refanyval", 0xFF, 0xC2,
			OpCodeType.TypedRef, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		#endregion

		#region Call

		/// <summary>
		/// Calls the method indicated by the passed method descriptor.
		/// </summary>
		public static readonly OpCode Call = new OpCode(
			"call", 0xFF, 0x28,
			OpCodeType.Call, OperandType.InlineMethod, FlowControl.Call,
			StackBehavior.Varpop, StackBehavior.Varpush, 0);

		/// <summary>
		/// Calls a late-bound method on an object, pushing the return value onto the
		/// evaluation stack.
		/// </summary>
		public static readonly OpCode Callvirt = new OpCode(
			"callvirt", 0xFF, 0x6F,
			OpCodeType.Call, OperandType.InlineMethod, FlowControl.Call,
			StackBehavior.Varpop, StackBehavior.Varpush, 0);

		/// <summary>
		/// Calls the method indicated on the evaluation stack (as a pointer to an entry
		/// point) with arguments described by a calling convention.
		/// </summary>
		public static readonly OpCode Calli = new OpCode(
			"calli", 0xFF, 0x29,
			OpCodeType.Call, OperandType.InlineSig, FlowControl.Call,
			StackBehavior.Varpop, StackBehavior.Varpush, 0);

		/// <summary>
		/// Exits current method and jumps to specified method.
		/// </summary>
		public static readonly OpCode Jmp = new OpCode(
			"jmp", 0xFF, 0x27,
			OpCodeType.Call, OperandType.InlineMethod, FlowControl.Call,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Creates a new object or a new instance of a value type, pushing an object
		/// reference (type O) onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Newobj = new OpCode(
			"newobj", 0xFF, 0x73,
			OpCodeType.Call, OperandType.InlineMethod, FlowControl.Call,
			StackBehavior.Varpop, StackBehavior.Pushref, 1);

		#endregion

		#region Branch

		/// <summary>
		/// Unconditionally transfers control to a target instruction.
		/// </summary>
		public static readonly OpCode Br = new OpCode(
			"br", 0xFF, 0x38,
			OpCodeType.Branch, OperandType.InlineBrTarget, FlowControl.Branch,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Unconditionally transfers control to a target instruction (short form).
		/// </summary>
		public static readonly OpCode Br_S = new OpCode(
			"br.s", 0xFF, 0x2B,
			OpCodeType.Branch, OperandType.ShortInlineBrTarget, FlowControl.Branch,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Exits a protected region of code, unconditionally transferring control to
		/// a specific target instruction.
		/// </summary>
		public static readonly OpCode Leave = new OpCode(
			"leave", 0xFF, 0xDD,
			OpCodeType.Branch, OperandType.InlineBrTarget, FlowControl.Branch,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Exits a protected region of code, unconditionally transferring control to
		/// a target instruction (short form).
		/// </summary>
		public static readonly OpCode Leave_S = new OpCode(
			"leave.s", 0xFF, 0xDE,
			OpCodeType.Branch, OperandType.ShortInlineBrTarget, FlowControl.Branch,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Throws the exception object currently on the evaluation stack.
		/// </summary>
		public static readonly OpCode Throw = new OpCode(
			"throw", 0xFF, 0x7A,
			OpCodeType.Branch, OperandType.InlineNone, FlowControl.Throw,
			StackBehavior.Popref, StackBehavior.Push0, -1);

		/// <summary>
		/// Rethrows the current exception.
		/// </summary>
		public static readonly OpCode Rethrow = new OpCode(
			"rethrow", 0xFE, 0x1A,
			OpCodeType.Branch, OperandType.InlineNone, FlowControl.Throw,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Returns from the current method, pushing a return value (if present) from
		/// the callee's evaluation stack onto the caller's evaluation stack.
		/// </summary>
		public static readonly OpCode Ret = new OpCode(
			"ret", 0xFF, 0x2A,
			OpCodeType.Branch, OperandType.InlineNone, FlowControl.Return,
			StackBehavior.Varpop, StackBehavior.Push0, 0);

		/// <summary>
		/// Transfers control from the fault or finally clause of an exception block
		/// back to the Common Language Infrastructure (CLI) exception handler.
		/// </summary>
		public static readonly OpCode Endfinally = new OpCode(
			"endfinally", 0xFF, 0xDC,
			OpCodeType.Branch, OperandType.InlineNone, FlowControl.Return,
			StackBehavior.Pop0, StackBehavior.Push0, 0);

		/// <summary>
		/// Transfers control from the filter clause of an exception back to the Common
		/// Language Infrastructure (CLI) exception handler.
		/// </summary>
		public static readonly OpCode Endfilter = new OpCode(
			"endfilter", 0xFE, 0x11,
			OpCodeType.Branch, OperandType.InlineNone, FlowControl.Return,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		#endregion

		#region CondBranch

		/// <summary>
		/// Transfers control to a target instruction if value is false, a null reference
		/// (Nothing in Visual Basic), or zero.
		/// </summary>
		public static readonly OpCode Brfalse = new OpCode(
			"brfalse", 0xFF, 0x39,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		/// <summary>
		/// Transfers control to a target instruction if value is true, not null, or
		/// non-zero.
		/// </summary>
		public static readonly OpCode Brtrue = new OpCode(
			"brtrue", 0xFF, 0x3A,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		/// <summary>
		/// Implements a jump table.
		/// </summary>
		public static readonly OpCode Switch = new OpCode(
			"switch", 0xFF, 0x45,
			OpCodeType.CondBranch, OperandType.InlineSwitch, FlowControl.Cond_Branch,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		/// <summary>
		/// Transfers control to a target instruction if two values are equal.
		/// </summary>
		public static readonly OpCode Beq = new OpCode(
			"beq", 0xFF, 0x3B,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if two values are
		/// equal.
		/// </summary>
		public static readonly OpCode Beq_S = new OpCode(
			"beq.s", 0xFF, 0x2E,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is greater than
		/// or equal to the second value.
		/// </summary>
		public static readonly OpCode Bge = new OpCode(
			"bge", 0xFF, 0x3C,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is greater than or equal to the second value.
		/// </summary>
		public static readonly OpCode Bge_S = new OpCode(
			"bge.s", 0xFF, 0x2F,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is greater than
		/// the second value, when comparing unsigned integer values or unordered float
		/// values.
		/// </summary>
		public static readonly OpCode Bge_Un = new OpCode(
			"bge.un", 0xFF, 0x41,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is greater than the second value, when comparing unsigned integer values
		/// or unordered float values.
		/// </summary>
		public static readonly OpCode Bge_Un_S = new OpCode(
			"bge.un.s", 0xFF, 0x34,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is greater than
		/// the second value.
		/// </summary>
		public static readonly OpCode Bgt = new OpCode(
			"bgt", 0xFF, 0x3D,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is greater than the second value.
		/// </summary>
		public static readonly OpCode Bgt_S = new OpCode(
			"bgt.s", 0xFF, 0x30,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is greater than
		/// the second value, when comparing unsigned integer values or unordered float
		/// values.
		/// </summary>
		public static readonly OpCode Bgt_Un = new OpCode(
			"bgt.un", 0xFF, 0x42,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is greater than the second value, when comparing unsigned integer values
		/// or unordered float values.
		/// </summary>
		public static readonly OpCode Bgt_Un_S = new OpCode(
			"bgt.un.s", 0xFF, 0x35,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is less than
		/// or equal to the second value.
		/// </summary>
		public static readonly OpCode Ble = new OpCode(
			"ble", 0xFF, 0x3E,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is less than or equal to the second value.
		/// </summary>
		public static readonly OpCode Ble_S = new OpCode(
			"ble.s", 0xFF, 0x31,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is less than
		/// or equal to the second value, when comparing unsigned integer values or unordered
		/// float values.
		/// </summary>
		public static readonly OpCode Ble_Un = new OpCode(
			"ble.un", 0xFF, 0x43,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is less than or equal to the second value, when comparing unsigned integer
		/// values or unordered float values.
		/// </summary>
		public static readonly OpCode Ble_Un_S = new OpCode(
			"ble.un.s", 0xFF, 0x36,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is less than
		/// the second value.
		/// </summary>
		public static readonly OpCode Blt = new OpCode(
			"blt", 0xFF, 0x3F,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is less than the second value.
		/// </summary>
		public static readonly OpCode Blt_S = new OpCode(
			"blt.s", 0xFF, 0x32,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if the first value is less than
		/// the second value, when comparing unsigned integer values or unordered float
		/// values.
		/// </summary>
		public static readonly OpCode Blt_Un = new OpCode(
			"blt.un", 0xFF, 0x44,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) if the first value
		/// is less than the second value, when comparing unsigned integer values or
		/// unordered float values.
		/// </summary>
		public static readonly OpCode Blt_Un_S = new OpCode(
			"blt.un.s", 0xFF, 0x37,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction when two unsigned integer values
		/// or unordered float values are not equal.
		/// </summary>
		public static readonly OpCode Bne_Un = new OpCode(
			"bne.un", 0xFF, 0x40,
			OpCodeType.CondBranch, OperandType.InlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction (short form) when two unsigned
		/// integer values or unordered float values are not equal.
		/// </summary>
		public static readonly OpCode Bne_Un_S = new OpCode(
			"bne.un.s", 0xFF, 0x33,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Pop1_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Transfers control to a target instruction if value is false, a null reference,
		/// or zero.
		/// </summary>
		public static readonly OpCode Brfalse_S = new OpCode(
			"brfalse.s", 0xFF, 0x2C,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		/// <summary>
		/// Transfers control to a target instruction (short form) if value is true,
		/// not null, or non-zero.
		/// </summary>
		public static readonly OpCode Brtrue_S = new OpCode(
			"brtrue.s", 0xFF, 0x2D,
			OpCodeType.CondBranch, OperandType.ShortInlineBrTarget, FlowControl.Cond_Branch,
			StackBehavior.Popi, StackBehavior.Push0, -1);

		#endregion

		#region Math

		/// <summary>
		/// Adds two values and pushes the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Add = new OpCode(
			"add", 0xFF, 0x58,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Adds two integers, performs an overflow check, and pushes the result onto
		/// the evaluation stack.
		/// </summary>
		public static readonly OpCode Add_Ovf = new OpCode(
			"add.ovf", 0xFF, 0xD6,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Adds two unsigned integer values, performs an overflow check, and pushes
		/// the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Add_Ovf_Un = new OpCode(
			"add.ovf.un", 0xFF, 0xD7,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Computes the bitwise AND of two values and pushes the result onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode And = new OpCode(
			"and", 0xFF, 0x5F,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Subtracts one value from another and pushes the result onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Sub = new OpCode(
			"sub", 0xFF, 0x59,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Subtracts one integer value from another, performs an overflow check, and
		/// pushes the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Sub_Ovf = new OpCode(
			"sub.ovf", 0xFF, 0xDA,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Subtracts one unsigned integer value from another, performs an overflow check,
		/// and pushes the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Sub_Ovf_Un = new OpCode(
			"sub.ovf.un", 0xFF, 0xDB,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Multiplies two values and pushes the result on the evaluation stack.
		/// </summary>
		public static readonly OpCode Mul = new OpCode(
			"mul", 0xFF, 0x5A,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Multiplies two integer values, performs an overflow check, and pushes the
		/// result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Mul_Ovf = new OpCode(
			"mul.ovf", 0xFF, 0xD8,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Multiplies two unsigned integer values, performs an overflow check, and pushes
		/// the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Mul_Ovf_Un = new OpCode(
			"mul.ovf.un", 0xFF, 0xD9,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Divides two values and pushes the result as a floating-point (type F) or
		/// quotient (type int32) onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Div = new OpCode(
			"div", 0xFF, 0x5B,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Divides two unsigned integer values and pushes the result (int32) onto the
		/// evaluation stack.
		/// </summary>
		public static readonly OpCode Div_Un = new OpCode(
			"div.un", 0xFF, 0x5C,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Negates a value and pushes the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Neg = new OpCode(
			"neg", 0xFF, 0x65,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push1, 0);

		/// <summary>
		/// Computes the bitwise complement of the integer value on top of the stack
		/// and pushes the result onto the evaluation stack as the same type.
		/// </summary>
		public static readonly OpCode Not = new OpCode(
			"not", 0xFF, 0x66,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push1, 0);

		/// <summary>
		/// Compute the bitwise complement of the two integer values on top of the stack
		/// and pushes the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Or = new OpCode(
			"or", 0xFF, 0x60,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Divides two values and pushes the remainder onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Rem = new OpCode(
			"rem", 0xFF, 0x5D,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Divides two unsigned values and pushes the remainder onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Rem_Un = new OpCode(
			"rem.un", 0xFF, 0x5E,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Shifts an integer value to the left (in zeroes) by a specified number of
		/// bits, pushing the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Shl = new OpCode(
			"shl", 0xFF, 0x62,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Shifts an integer value (in sign) to the right by a specified number of bits,
		/// pushing the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Shr = new OpCode(
			"shr", 0xFF, 0x63,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Shifts an unsigned integer value (in zeroes) to the right by a specified
		/// number of bits, pushing the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Shr_Un = new OpCode(
			"shr.un", 0xFF, 0x64,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		/// <summary>
		/// Computes the bitwise XOR of the top two values on the evaluation stack, pushing
		/// the result onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Xor = new OpCode(
			"xor", 0xFF, 0x61,
			OpCodeType.Math, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Push1, -1);

		#endregion

		#region Boxing

		/// <summary>
		/// Converts a value type to an object reference (type O).
		/// </summary>
		public static readonly OpCode Box = new OpCode(
			"box", 0xFF, 0x8C,
			OpCodeType.Box, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushref, 0);

		/// <summary>
		/// Converts the boxed representation of a value type to its unboxed form.
		/// </summary>
		public static readonly OpCode Unbox = new OpCode(
			"unbox", 0xFF, 0x79,
			OpCodeType.Box, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the boxed representation of a type specified in the instruction
		/// to its unboxed form.
		/// </summary>
		public static readonly OpCode Unbox_Any = new OpCode(
			"unbox.any", 0xFF, 0xA5,
			OpCodeType.Box, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Push1, 0);

		#endregion

		#region Convert

		/// <summary>
		/// Converts the value on top of the evaluation stack to native int.
		/// </summary>
		public static readonly OpCode Conv_I = new OpCode(
			"conv.i", 0xFF, 0xD3,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to int8, then extends (pads)
		/// it to int32.
		/// </summary>
		public static readonly OpCode Conv_I1 = new OpCode(
			"conv.i1", 0xFF, 0x67,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to int16, then extends
		/// (pads) it to int32.
		/// </summary>
		public static readonly OpCode Conv_I2 = new OpCode(
			"conv.i2", 0xFF, 0x68,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to int32.
		/// </summary>
		public static readonly OpCode Conv_I4 = new OpCode(
			"conv.i4", 0xFF, 0x69,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to int64.
		/// </summary>
		public static readonly OpCode Conv_I8 = new OpCode(
			"conv.i8", 0xFF, 0x6A,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi8, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to signed native
		/// int, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I = new OpCode(
			"conv.ovf.i", 0xFF, 0xD4,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to signed native
		/// int, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I_Un = new OpCode(
			"conv.ovf.i.un", 0xFF, 0x8A,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to signed int8 and
		/// extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I1 = new OpCode(
			"conv.ovf.i1", 0xFF, 0xB3,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to signed int8
		/// and extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I1_Un = new OpCode(
			"conv.ovf.i1.un", 0xFF, 0x82,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to signed int16
		/// and extending it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I2 = new OpCode(
			"conv.ovf.i2", 0xFF, 0xB5,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to signed int16
		/// and extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I2_Un = new OpCode(
			"conv.ovf.i2.un", 0xFF, 0x83,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to signed int32,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I4 = new OpCode(
			"conv.ovf.i4", 0xFF, 0xB7,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to signed int32,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I4_Un = new OpCode(
			"conv.ovf.i4.un", 0xFF, 0x84,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to signed int64,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I8 = new OpCode(
			"conv.ovf.i8", 0xFF, 0xB9,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi8, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to signed int64,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_I8_Un = new OpCode(
			"conv.ovf.i8.un", 0xFF, 0x85,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi8, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to unsigned native
		/// int, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U = new OpCode(
			"conv.ovf.u", 0xFF, 0xD5,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to unsigned native
		/// int, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U_Un = new OpCode(
			"conv.ovf.u.un", 0xFF, 0x8B,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to unsigned int8
		/// and extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U1 = new OpCode(
			"conv.ovf.u1", 0xFF, 0xB4,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to unsigned int8
		/// and extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U1_Un = new OpCode(
			"conv.ovf.u1.un", 0xFF, 0x86,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to unsigned int16
		/// and extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U2 = new OpCode(
			"conv.ovf.u2", 0xFF, 0xB6,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to unsigned int16
		/// and extends it to int32, throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U2_Un = new OpCode(
			"conv.ovf.u2.un", 0xFF, 0x87,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to unsigned int32,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U4 = new OpCode(
			"conv.ovf.u4", 0xFF, 0xB8,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to unsigned int32,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U4_Un = new OpCode(
			"conv.ovf.u4.un", 0xFF, 0x88,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the signed value on top of the evaluation stack to unsigned int64,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U8 = new OpCode(
			"conv.ovf.u8", 0xFF, 0xBA,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi8, 0);

		/// <summary>
		/// Converts the unsigned value on top of the evaluation stack to unsigned int64,
		/// throwing System.OverflowException on overflow.
		/// </summary>
		public static readonly OpCode Conv_Ovf_U8_Un = new OpCode(
			"conv.ovf.u8.un", 0xFF, 0x89,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi8, 0);

		/// <summary>
		/// Converts the unsigned integer value on top of the evaluation stack to float32.
		/// </summary>
		public static readonly OpCode Conv_R_Un = new OpCode(
			"conv.r.un", 0xFF, 0x76,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushr8, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to float32.
		/// </summary>
		public static readonly OpCode Conv_R4 = new OpCode(
			"conv.r4", 0xFF, 0x6B,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushr4, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to float64.
		/// </summary>
		public static readonly OpCode Conv_R8 = new OpCode(
			"conv.r8", 0xFF, 0x6C,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushr8, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to unsigned native int,
		/// and extends it to native int.
		/// </summary>
		public static readonly OpCode Conv_U = new OpCode(
			"conv.u", 0xFF, 0xE0,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to unsigned int8, and extends
		/// it to int32.
		/// </summary>
		public static readonly OpCode Conv_U1 = new OpCode(
			"conv.u1", 0xFF, 0xD2,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to unsigned int16, and
		/// extends it to int32.
		/// </summary>
		public static readonly OpCode Conv_U2 = new OpCode(
			"conv.u2", 0xFF, 0xD1,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to unsigned int32, and
		/// extends it to int32.
		/// </summary>
		public static readonly OpCode Conv_U4 = new OpCode(
			"conv.u4", 0xFF, 0x6D,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi, 0);

		/// <summary>
		/// Converts the value on top of the evaluation stack to unsigned int64, and
		/// extends it to int64.
		/// </summary>
		public static readonly OpCode Conv_U8 = new OpCode(
			"conv.u8", 0xFF, 0x6E,
			OpCodeType.Convert, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Pushi8, 0);

		#endregion

		#region Compare

		/// <summary>
		/// Compares two values. If they are equal, the integer value 1 (int32) is pushed
		/// onto the evaluation stack; otherwise 0 (int32) is pushed onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Ceq = new OpCode(
			"ceq", 0xFE, 0x1,
			OpCodeType.Compare, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Pushi, -1);

		/// <summary>
		/// Compares two values. If the first value is greater than the second, the integer
		/// value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32)
		/// is pushed onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Cgt = new OpCode(
			"cgt", 0xFE, 0x2,
			OpCodeType.Compare, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Pushi, -1);

		/// <summary>
		/// Compares two unsigned or unordered values. If the first value is greater
		/// than the second, the integer value 1 (int32) is pushed onto the evaluation
		/// stack; otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Cgt_Un = new OpCode(
			"cgt.un", 0xFE, 0x3,
			OpCodeType.Compare, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Pushi, -1);

		/// <summary>
		/// Compares two values. If the first value is less than the second, the integer
		/// value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32)
		/// is pushed onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Clt = new OpCode(
			"clt", 0xFE, 0x4,
			OpCodeType.Compare, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Pushi, -1);

		/// <summary>
		/// Compares the unsigned or unordered values value1 and value2. If value1 is
		/// less than value2, then the integer value 1 (int32) is pushed onto the evaluation
		/// stack; otherwise 0 (int32) is pushed onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Clt_Un = new OpCode(
			"clt.un", 0xFE, 0x5,
			OpCodeType.Compare, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1_pop1, StackBehavior.Pushi, -1);

		#endregion

		#region Ldarg

		/// <summary>
		/// Loads an argument (referenced by a specified index value) onto the stack.
		/// </summary>
		public static readonly OpCode Ldarg = new OpCode(
			"ldarg", 0xFE, 0x9,
			OpCodeType.Ldarg, OperandType.InlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Load an argument address onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarga = new OpCode(
			"ldarga", 0xFE, 0xA,
			OpCodeType.Ldarg, OperandType.InlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Loads the argument at index 0 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarg_0 = new OpCode(
			"ldarg.0", 0xFF, 0x2,
			OpCodeType.Ldarg, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the argument at index 1 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarg_1 = new OpCode(
			"ldarg.1", 0xFF, 0x3,
			OpCodeType.Ldarg, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the argument at index 2 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarg_2 = new OpCode(
			"ldarg.2", 0xFF, 0x4,
			OpCodeType.Ldarg, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the argument at index 3 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarg_3 = new OpCode(
			"ldarg.3", 0xFF, 0x5,
			OpCodeType.Ldarg, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the argument (referenced by a specified short form index) onto the
		/// evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarg_S = new OpCode(
			"ldarg.s", 0xFF, 0xE,
			OpCodeType.Ldarg, OperandType.ShortInlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Load an argument address, in short form, onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldarga_S = new OpCode(
			"ldarga.s", 0xFF, 0xF,
			OpCodeType.Ldarg, OperandType.ShortInlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		#endregion

		#region Starg

		/// <summary>
		/// Stores the value on top of the evaluation stack in the argument slot at a
		/// specified index.
		/// </summary>
		public static readonly OpCode Starg = new OpCode(
			"starg", 0xFE, 0xB,
			OpCodeType.Starg, OperandType.InlineVar, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Stores the value on top of the evaluation stack in the argument slot at a
		/// specified index, short form.
		/// </summary>
		public static readonly OpCode Starg_S = new OpCode(
			"starg.s", 0xFF, 0x10,
			OpCodeType.Starg, OperandType.ShortInlineVar, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		#endregion

		#region Ldloc

		/// <summary>
		/// Loads the local variable at a specific index onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldloc = new OpCode(
			"ldloc", 0xFE, 0xC,
			OpCodeType.Ldloc, OperandType.InlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the address of the local variable at a specific index onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Ldloca = new OpCode(
			"ldloca", 0xFE, 0xD,
			OpCodeType.Ldloc, OperandType.InlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Loads the local variable at index 0 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldloc_0 = new OpCode(
			"ldloc.0", 0xFF, 0x6,
			OpCodeType.Ldloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the local variable at index 1 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldloc_1 = new OpCode(
			"ldloc.1", 0xFF, 0x7,
			OpCodeType.Ldloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the local variable at index 2 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldloc_2 = new OpCode(
			"ldloc.2", 0xFF, 0x8,
			OpCodeType.Ldloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the local variable at index 3 onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldloc_3 = new OpCode(
			"ldloc.3", 0xFF, 0x9,
			OpCodeType.Ldloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the local variable at a specific index onto the evaluation stack, short
		/// form.
		/// </summary>
		public static readonly OpCode Ldloc_S = new OpCode(
			"ldloc.s", 0xFF, 0x11,
			OpCodeType.Ldloc, OperandType.ShortInlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Loads the address of the local variable at a specific index onto the evaluation
		/// stack, short form.
		/// </summary>
		public static readonly OpCode Ldloca_S = new OpCode(
			"ldloca.s", 0xFF, 0x12,
			OpCodeType.Ldloc, OperandType.ShortInlineVar, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		#endregion

		#region Stloc

		/// <summary>
		/// Pops the current value from the top of the evaluation stack and stores it
		/// in a the local variable list at a specified index.
		/// </summary>
		public static readonly OpCode Stloc = new OpCode(
			"stloc", 0xFE, 0xE,
			OpCodeType.Stloc, OperandType.InlineVar, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Pops the current value from the top of the evaluation stack and stores it
		/// in a the local variable list at index 0.
		/// </summary>
		public static readonly OpCode Stloc_0 = new OpCode(
			"stloc.0", 0xFF, 0xA,
			OpCodeType.Stloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Pops the current value from the top of the evaluation stack and stores it
		/// in a the local variable list at index 1.
		/// </summary>
		public static readonly OpCode Stloc_1 = new OpCode(
			"stloc.1", 0xFF, 0xB,
			OpCodeType.Stloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Pops the current value from the top of the evaluation stack and stores it
		/// in a the local variable list at index 2.
		/// </summary>
		public static readonly OpCode Stloc_2 = new OpCode(
			"stloc.2", 0xFF, 0xC,
			OpCodeType.Stloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Pops the current value from the top of the evaluation stack and stores it
		/// in a the local variable list at index 3.
		/// </summary>
		public static readonly OpCode Stloc_3 = new OpCode(
			"stloc.3", 0xFF, 0xD,
			OpCodeType.Stloc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		/// <summary>
		/// Pops the current value from the top of the evaluation stack and stores it
		/// in a the local variable list at index (short form).
		/// </summary>
		public static readonly OpCode Stloc_S = new OpCode(
			"stloc.s", 0xFF, 0x13,
			OpCodeType.Stloc, OperandType.ShortInlineVar, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		#endregion

		#region Ldc

		/// <summary>
		/// Pushes a supplied value of type int32 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4 = new OpCode(
			"ldc.i4", 0xFF, 0x20,
			OpCodeType.Ldc, OperandType.InlineI, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes a supplied value of type int64 onto the evaluation stack as an int64.
		/// </summary>
		public static readonly OpCode Ldc_I8 = new OpCode(
			"ldc.i8", 0xFF, 0x21,
			OpCodeType.Ldc, OperandType.InlineI8, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi8, 1);

		/// <summary>
		/// Pushes a supplied value of type float32 onto the evaluation stack as type
		/// F (float).
		/// </summary>
		public static readonly OpCode Ldc_R4 = new OpCode(
			"ldc.r4", 0xFF, 0x22,
			OpCodeType.Ldc, OperandType.ShortInlineR, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushr4, 1);

		/// <summary>
		/// Pushes a supplied value of type float64 onto the evaluation stack as type
		/// F (float).
		/// </summary>
		public static readonly OpCode Ldc_R8 = new OpCode(
			"ldc.r8", 0xFF, 0x23,
			OpCodeType.Ldc, OperandType.InlineR, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushr8, 1);

		/// <summary>
		/// Pushes the integer value of 0 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_0 = new OpCode(
			"ldc.i4.0", 0xFF, 0x16,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 1 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_1 = new OpCode(
			"ldc.i4.1", 0xFF, 0x17,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 2 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_2 = new OpCode(
			"ldc.i4.2", 0xFF, 0x18,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 3 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_3 = new OpCode(
			"ldc.i4.3", 0xFF, 0x19,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 4 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_4 = new OpCode(
			"ldc.i4.4", 0xFF, 0x1A,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 5 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_5 = new OpCode(
			"ldc.i4.5", 0xFF, 0x1B,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 6 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_6 = new OpCode(
			"ldc.i4.6", 0xFF, 0x1C,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 7 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_7 = new OpCode(
			"ldc.i4.7", 0xFF, 0x1D,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of 8 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_8 = new OpCode(
			"ldc.i4.8", 0xFF, 0x1E,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the integer value of -1 onto the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldc_I4_M1 = new OpCode(
			"ldc.i4.m1", 0xFF, 0x15,
			OpCodeType.Ldc, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes the supplied int8 value onto the evaluation stack as an int32, short form.
		/// </summary>
		public static readonly OpCode Ldc_I4_S = new OpCode(
			"ldc.i4.s", 0xFF, 0x1F,
			OpCodeType.Ldc, OperandType.ShortInlineI, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		#endregion

		#region Ldind

		/// <summary>
		/// Loads a value of type native int as a native int onto the evaluation stack
		/// indirectly.
		/// </summary>
		public static readonly OpCode Ldind_I = new OpCode(
			"ldind.i", 0xFF, 0x4D,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Loads a value of type int8 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		public static readonly OpCode Ldind_I1 = new OpCode(
			"ldind.i1", 0xFF, 0x46,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Loads a value of type int16 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		public static readonly OpCode Ldind_I2 = new OpCode(
			"ldind.i2", 0xFF, 0x48,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Loads a value of type int32 as an int32 onto the evaluation stack indirectly.
		/// </summary>
		public static readonly OpCode Ldind_I4 = new OpCode(
			"ldind.i4", 0xFF, 0x4A,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Loads a value of type int64 as an int64 onto the evaluation stack indirectly.
		/// </summary>
		public static readonly OpCode Ldind_I8 = new OpCode(
			"ldind.i8", 0xFF, 0x4C,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi8, 0);

		/// <summary>
		/// Loads a value of type float32 as a type F (float) onto the evaluation stack
		/// indirectly.
		/// </summary>
		public static readonly OpCode Ldind_R4 = new OpCode(
			"ldind.r4", 0xFF, 0x4E,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushr4, 0);

		/// <summary>
		/// Loads a value of type float64 as a type F (float) onto the evaluation stack
		/// indirectly.
		/// </summary>
		public static readonly OpCode Ldind_R8 = new OpCode(
			"ldind.r8", 0xFF, 0x4F,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushr8, 0);

		/// <summary>
		/// Loads an object reference as a type O (object reference) onto the evaluation
		/// stack indirectly.
		/// </summary>
		public static readonly OpCode Ldind_Ref = new OpCode(
			"ldind.ref", 0xFF, 0x50,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushref, 0);

		/// <summary>
		/// Loads a value of type unsigned int8 as an int32 onto the evaluation stack
		/// indirectly.
		/// </summary>
		public static readonly OpCode Ldind_U1 = new OpCode(
			"ldind.u1", 0xFF, 0x47,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Loads a value of type unsigned int16 as an int32 onto the evaluation stack
		/// indirectly.
		/// </summary>
		public static readonly OpCode Ldind_U2 = new OpCode(
			"ldind.u2", 0xFF, 0x49,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		/// <summary>
		/// Loads a value of type unsigned int32 as an int32 onto the evaluation stack
		/// indirectly.
		/// </summary>
		public static readonly OpCode Ldind_U4 = new OpCode(
			"ldind.u4", 0xFF, 0x4B,
			OpCodeType.Ldind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi, StackBehavior.Pushi, 0);

		#endregion

		#region Stind

		/// <summary>
		/// Stores a value of type native int at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_I = new OpCode(
			"stind.i", 0xFF, 0xDF,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a value of type int8 at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_I1 = new OpCode(
			"stind.i1", 0xFF, 0x52,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a value of type int16 at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_I2 = new OpCode(
			"stind.i2", 0xFF, 0x53,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a value of type int32 at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_I4 = new OpCode(
			"stind.i4", 0xFF, 0x54,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a value of type int64 at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_I8 = new OpCode(
			"stind.i8", 0xFF, 0x55,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi8, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a value of type float32 at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_R4 = new OpCode(
			"stind.r4", 0xFF, 0x56,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popr4, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a value of type float64 at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_R8 = new OpCode(
			"stind.r8", 0xFF, 0x57,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popr8, StackBehavior.Push0, -2);

		/// <summary>
		/// Stores a object reference value at a supplied address.
		/// </summary>
		public static readonly OpCode Stind_Ref = new OpCode(
			"stind.ref", 0xFF, 0x51,
			OpCodeType.Stind, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popi_popi, StackBehavior.Push0, -2);

		#endregion

		#region Ldelem

		/// <summary>
		/// Loads the element at a specified array index onto the top of the evaluation
		/// stack as the type specified in the instruction.
		/// </summary>
		public static readonly OpCode Ldelem = new OpCode(
			"ldelem", 0xFF, 0xA3,
			OpCodeType.Ldelem, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Push1, -1);

		/// <summary>
		/// Loads the element with type native int at a specified array index onto the
		/// top of the evaluation stack as a native int.
		/// </summary>
		public static readonly OpCode Ldelem_I = new OpCode(
			"ldelem.i", 0xFF, 0x97,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the element with type int8 at a specified array index onto the top
		/// of the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldelem_I1 = new OpCode(
			"ldelem.i1", 0xFF, 0x90,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the element with type int16 at a specified array index onto the top
		/// of the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldelem_I2 = new OpCode(
			"ldelem.i2", 0xFF, 0x92,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the element with type int32 at a specified array index onto the top
		/// of the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldelem_I4 = new OpCode(
			"ldelem.i4", 0xFF, 0x94,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the element with type int64 at a specified array index onto the top
		/// of the evaluation stack as an int64.
		/// </summary>
		public static readonly OpCode Ldelem_I8 = new OpCode(
			"ldelem.i8", 0xFF, 0x96,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi8, -1);

		/// <summary>
		/// Loads the element with type float32 at a specified array index onto the top
		/// of the evaluation stack as type F (float).
		/// </summary>
		public static readonly OpCode Ldelem_R4 = new OpCode(
			"ldelem.r4", 0xFF, 0x98,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushr4, -1);

		/// <summary>
		/// Loads the element with type float64 at a specified array index onto the top
		/// of the evaluation stack as type F (float).
		/// </summary>
		public static readonly OpCode Ldelem_R8 = new OpCode(
			"ldelem.r8", 0xFF, 0x99,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushr8, -1);

		/// <summary>
		/// Loads the element containing an object reference at a specified array index
		/// onto the top of the evaluation stack as type O (object reference).
		/// </summary>
		public static readonly OpCode Ldelem_Ref = new OpCode(
			"ldelem.ref", 0xFF, 0x9A,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushref, -1);

		/// <summary>
		/// Loads the element with type unsigned int8 at a specified array index onto
		/// the top of the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldelem_U1 = new OpCode(
			"ldelem.u1", 0xFF, 0x91,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the element with type unsigned int16 at a specified array index onto
		/// the top of the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldelem_U2 = new OpCode(
			"ldelem.u2", 0xFF, 0x93,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the element with type unsigned int32 at a specified array index onto
		/// the top of the evaluation stack as an int32.
		/// </summary>
		public static readonly OpCode Ldelem_U4 = new OpCode(
			"ldelem.u4", 0xFF, 0x95,
			OpCodeType.Ldelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		/// <summary>
		/// Loads the address of the array element at a specified array index onto the
		/// top of the evaluation stack as type & (managed pointer).
		/// </summary>
		public static readonly OpCode Ldelema = new OpCode(
			"ldelema", 0xFF, 0x8F,
			OpCodeType.Ldelem, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref_popi, StackBehavior.Pushi, -1);

		#endregion

		#region Stelem

		/// <summary>
		/// Replaces the array element at a given index with the value on the evaluation
		/// stack, whose type is specified in the instruction.
		/// </summary>
		public static readonly OpCode Stelem = new OpCode(
			"stelem", 0xFF, 0xA4,
			OpCodeType.Stelem, OperandType.InlineType, FlowControl.Next,
			StackBehavior.Popref_popi_pop1, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the native int value on
		/// the evaluation stack.
		/// </summary>
		public static readonly OpCode Stelem_I = new OpCode(
			"stelem.i", 0xFF, 0x9B,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popi, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the int8 value on the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Stelem_I1 = new OpCode(
			"stelem.i1", 0xFF, 0x9C,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popi, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the int16 value on the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Stelem_I2 = new OpCode(
			"stelem.i2", 0xFF, 0x9D,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popi, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the int32 value on the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Stelem_I4 = new OpCode(
			"stelem.i4", 0xFF, 0x9E,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popi, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the int64 value on the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Stelem_I8 = new OpCode(
			"stelem.i8", 0xFF, 0x9F,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popi8, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the float32 value on the
		/// evaluation stack.
		/// </summary>
		public static readonly OpCode Stelem_R4 = new OpCode(
			"stelem.r4", 0xFF, 0xA0,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popr4, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the float64 value on the
		/// evaluation stack.
		/// </summary>
		public static readonly OpCode Stelem_R8 = new OpCode(
			"stelem.r8", 0xFF, 0xA1,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popr8, StackBehavior.Push0, -3);

		/// <summary>
		/// Replaces the array element at a given index with the object ref value (type
		/// O) on the evaluation stack.
		/// </summary>
		public static readonly OpCode Stelem_Ref = new OpCode(
			"stelem.ref", 0xFF, 0xA2,
			OpCodeType.Stelem, OperandType.InlineNone, FlowControl.Next,
			StackBehavior.Popref_popi_popref, StackBehavior.Push0, -3);

		#endregion

		#region Ldfld

		/// <summary>
		/// Finds the value of a field in the object whose reference is currently on
		/// the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldfld = new OpCode(
			"ldfld", 0xFF, 0x7B,
			OpCodeType.Ldfld, OperandType.InlineField, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Push1, 0);

		/// <summary>
		/// Finds the address of a field in the object whose reference is currently on
		/// the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldflda = new OpCode(
			"ldflda", 0xFF, 0x7C,
			OpCodeType.Ldfld, OperandType.InlineField, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Pushi, 0);

		/// <summary>
		/// Pushes the value of a static field onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldsfld = new OpCode(
			"ldsfld", 0xFF, 0x7E,
			OpCodeType.Ldfld, OperandType.InlineField, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Push1, 1);

		/// <summary>
		/// Pushes the address of a static field onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldsflda = new OpCode(
			"ldsflda", 0xFF, 0x7F,
			OpCodeType.Ldfld, OperandType.InlineField, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		#endregion

		#region Stfld

		/// <summary>
		/// Replaces the value stored in the field of an object reference or pointer
		/// with a new value.
		/// </summary>
		public static readonly OpCode Stfld = new OpCode(
			"stfld", 0xFF, 0x7D,
			OpCodeType.Stfld, OperandType.InlineField, FlowControl.Next,
			StackBehavior.Popref_pop1, StackBehavior.Push0, -2);

		/// <summary>
		/// Replaces the value of a static field with a value from the evaluation stack.
		/// </summary>
		public static readonly OpCode Stsfld = new OpCode(
			"stsfld", 0xFF, 0x80,
			OpCodeType.Stfld, OperandType.InlineField, FlowControl.Next,
			StackBehavior.Pop1, StackBehavior.Push0, -1);

		#endregion

		#region Ldftn

		/// <summary>
		/// Pushes an unmanaged pointer (type native int) to the native code implementing
		/// a specific method onto the evaluation stack.
		/// </summary>
		public static readonly OpCode Ldftn = new OpCode(
			"ldftn", 0xFE, 0x6,
			OpCodeType.Ldftn, OperandType.InlineMethod, FlowControl.Next,
			StackBehavior.Pop0, StackBehavior.Pushi, 1);

		/// <summary>
		/// Pushes an unmanaged pointer (type native int) to the native code implementing
		/// a particular virtual method associated with a specified object onto the evaluation
		/// stack.
		/// </summary>
		public static readonly OpCode Ldvirtftn = new OpCode(
			"ldvirtftn", 0xFE, 0x7,
			OpCodeType.Ldftn, OperandType.InlineMethod, FlowControl.Next,
			StackBehavior.Popref, StackBehavior.Pushi, 0);

		#endregion

		#region Array

		public static readonly OpCode[] OpCodeArray = new OpCode[]
		{
			// One byte opcodes
			Nop, // 0 (0x0)
			Break, // 1 (0x1)
			Ldarg_0, // 2 (0x2)
			Ldarg_1, // 3 (0x3)
			Ldarg_2, // 4 (0x4)
			Ldarg_3, // 5 (0x5)
			Ldloc_0, // 6 (0x6)
			Ldloc_1, // 7 (0x7)
			Ldloc_2, // 8 (0x8)
			Ldloc_3, // 9 (0x9)
			Stloc_0, // 10 (0xA)
			Stloc_1, // 11 (0xB)
			Stloc_2, // 12 (0xC)
			Stloc_3, // 13 (0xD)
			Ldarg_S, // 14 (0xE)
			Ldarga_S, // 15 (0xF)
			Starg_S, // 16 (0x10)
			Ldloc_S, // 17 (0x11)
			Ldloca_S, // 18 (0x12)
			Stloc_S, // 19 (0x13)
			Ldnull, // 20 (0x14)
			Ldc_I4_M1, // 21 (0x15)
			Ldc_I4_0, // 22 (0x16)
			Ldc_I4_1, // 23 (0x17)
			Ldc_I4_2, // 24 (0x18)
			Ldc_I4_3, // 25 (0x19)
			Ldc_I4_4, // 26 (0x1A)
			Ldc_I4_5, // 27 (0x1B)
			Ldc_I4_6, // 28 (0x1C)
			Ldc_I4_7, // 29 (0x1D)
			Ldc_I4_8, // 30 (0x1E)
			Ldc_I4_S, // 31 (0x1F)
			Ldc_I4, // 32 (0x20)
			Ldc_I8, // 33 (0x21)
			Ldc_R4, // 34 (0x22)
			Ldc_R8, // 35 (0x23)
			null, // 36 (0x24)
			Dup, // 37 (0x25)
			Pop, // 38 (0x26)
			Jmp, // 39 (0x27)
			Call, // 40 (0x28)
			Calli, // 41 (0x29)
			Ret, // 42 (0x2A)
			Br_S, // 43 (0x2B)
			Brfalse_S, // 44 (0x2C)
			Brtrue_S, // 45 (0x2D)
			Beq_S, // 46 (0x2E)
			Bge_S, // 47 (0x2F)
			Bgt_S, // 48 (0x30)
			Ble_S, // 49 (0x31)
			Blt_S, // 50 (0x32)
			Bne_Un_S, // 51 (0x33)
			Bge_Un_S, // 52 (0x34)
			Bgt_Un_S, // 53 (0x35)
			Ble_Un_S, // 54 (0x36)
			Blt_Un_S, // 55 (0x37)
			Br, // 56 (0x38)
			Brfalse, // 57 (0x39)
			Brtrue, // 58 (0x3A)
			Beq, // 59 (0x3B)
			Bge, // 60 (0x3C)
			Bgt, // 61 (0x3D)
			Ble, // 62 (0x3E)
			Blt, // 63 (0x3F)
			Bne_Un, // 64 (0x40)
			Bge_Un, // 65 (0x41)
			Bgt_Un, // 66 (0x42)
			Ble_Un, // 67 (0x43)
			Blt_Un, // 68 (0x44)
			Switch, // 69 (0x45)
			Ldind_I1, // 70 (0x46)
			Ldind_U1, // 71 (0x47)
			Ldind_I2, // 72 (0x48)
			Ldind_U2, // 73 (0x49)
			Ldind_I4, // 74 (0x4A)
			Ldind_U4, // 75 (0x4B)
			Ldind_I8, // 76 (0x4C)
			Ldind_I, // 77 (0x4D)
			Ldind_R4, // 78 (0x4E)
			Ldind_R8, // 79 (0x4F)
			Ldind_Ref, // 80 (0x50)
			Stind_Ref, // 81 (0x51)
			Stind_I1, // 82 (0x52)
			Stind_I2, // 83 (0x53)
			Stind_I4, // 84 (0x54)
			Stind_I8, // 85 (0x55)
			Stind_R4, // 86 (0x56)
			Stind_R8, // 87 (0x57)
			Add, // 88 (0x58)
			Sub, // 89 (0x59)
			Mul, // 90 (0x5A)
			Div, // 91 (0x5B)
			Div_Un, // 92 (0x5C)
			Rem, // 93 (0x5D)
			Rem_Un, // 94 (0x5E)
			And, // 95 (0x5F)
			Or, // 96 (0x60)
			Xor, // 97 (0x61)
			Shl, // 98 (0x62)
			Shr, // 99 (0x63)
			Shr_Un, // 100 (0x64)
			Neg, // 101 (0x65)
			Not, // 102 (0x66)
			Conv_I1, // 103 (0x67)
			Conv_I2, // 104 (0x68)
			Conv_I4, // 105 (0x69)
			Conv_I8, // 106 (0x6A)
			Conv_R4, // 107 (0x6B)
			Conv_R8, // 108 (0x6C)
			Conv_U4, // 109 (0x6D)
			Conv_U8, // 110 (0x6E)
			Callvirt, // 111 (0x6F)
			Cpobj, // 112 (0x70)
			Ldobj, // 113 (0x71)
			Ldstr, // 114 (0x72)
			Newobj, // 115 (0x73)
			Castclass, // 116 (0x74)
			Isinst, // 117 (0x75)
			Conv_R_Un, // 118 (0x76)
			null, // 119 (0x77)
			null, // 120 (0x78)
			Unbox, // 121 (0x79)
			Throw, // 122 (0x7A)
			Ldfld, // 123 (0x7B)
			Ldflda, // 124 (0x7C)
			Stfld, // 125 (0x7D)
			Ldsfld, // 126 (0x7E)
			Ldsflda, // 127 (0x7F)
			Stsfld, // 128 (0x80)
			Stobj, // 129 (0x81)
			Conv_Ovf_I1_Un, // 130 (0x82)
			Conv_Ovf_I2_Un, // 131 (0x83)
			Conv_Ovf_I4_Un, // 132 (0x84)
			Conv_Ovf_I8_Un, // 133 (0x85)
			Conv_Ovf_U1_Un, // 134 (0x86)
			Conv_Ovf_U2_Un, // 135 (0x87)
			Conv_Ovf_U4_Un, // 136 (0x88)
			Conv_Ovf_U8_Un, // 137 (0x89)
			Conv_Ovf_I_Un, // 138 (0x8A)
			Conv_Ovf_U_Un, // 139 (0x8B)
			Box, // 140 (0x8C)
			Newarr, // 141 (0x8D)
			Ldlen, // 142 (0x8E)
			Ldelema, // 143 (0x8F)
			Ldelem_I1, // 144 (0x90)
			Ldelem_U1, // 145 (0x91)
			Ldelem_I2, // 146 (0x92)
			Ldelem_U2, // 147 (0x93)
			Ldelem_I4, // 148 (0x94)
			Ldelem_U4, // 149 (0x95)
			Ldelem_I8, // 150 (0x96)
			Ldelem_I, // 151 (0x97)
			Ldelem_R4, // 152 (0x98)
			Ldelem_R8, // 153 (0x99)
			Ldelem_Ref, // 154 (0x9A)
			Stelem_I, // 155 (0x9B)
			Stelem_I1, // 156 (0x9C)
			Stelem_I2, // 157 (0x9D)
			Stelem_I4, // 158 (0x9E)
			Stelem_I8, // 159 (0x9F)
			Stelem_R4, // 160 (0xA0)
			Stelem_R8, // 161 (0xA1)
			Stelem_Ref, // 162 (0xA2)
			Ldelem, // 163 (0xA3)
			Stelem, // 164 (0xA4)
			Unbox_Any, // 165 (0xA5)
			null, // 166 (0xA6)
			null, // 167 (0xA7)
			null, // 168 (0xA8)
			null, // 169 (0xA9)
			null, // 170 (0xAA)
			null, // 171 (0xAB)
			null, // 172 (0xAC)
			null, // 173 (0xAD)
			null, // 174 (0xAE)
			null, // 175 (0xAF)
			null, // 176 (0xB0)
			null, // 177 (0xB1)
			null, // 178 (0xB2)
			Conv_Ovf_I1, // 179 (0xB3)
			Conv_Ovf_U1, // 180 (0xB4)
			Conv_Ovf_I2, // 181 (0xB5)
			Conv_Ovf_U2, // 182 (0xB6)
			Conv_Ovf_I4, // 183 (0xB7)
			Conv_Ovf_U4, // 184 (0xB8)
			Conv_Ovf_I8, // 185 (0xB9)
			Conv_Ovf_U8, // 186 (0xBA)
			null, // 187 (0xBB)
			null, // 188 (0xBC)
			null, // 189 (0xBD)
			null, // 190 (0xBE)
			null, // 191 (0xBF)
			null, // 192 (0xC0)
			null, // 193 (0xC1)
			Refanyval, // 194 (0xC2)
			Ckfinite, // 195 (0xC3)
			null, // 196 (0xC4)
			null, // 197 (0xC5)
			Mkrefany, // 198 (0xC6)
			null, // 199 (0xC7)
			null, // 200 (0xC8)
			null, // 201 (0xC9)
			null, // 202 (0xCA)
			null, // 203 (0xCB)
			null, // 204 (0xCC)
			null, // 205 (0xCD)
			null, // 206 (0xCE)
			null, // 207 (0xCF)
			Ldtoken, // 208 (0xD0)
			Conv_U2, // 209 (0xD1)
			Conv_U1, // 210 (0xD2)
			Conv_I, // 211 (0xD3)
			Conv_Ovf_I, // 212 (0xD4)
			Conv_Ovf_U, // 213 (0xD5)
			Add_Ovf, // 214 (0xD6)
			Add_Ovf_Un, // 215 (0xD7)
			Mul_Ovf, // 216 (0xD8)
			Mul_Ovf_Un, // 217 (0xD9)
			Sub_Ovf, // 218 (0xDA)
			Sub_Ovf_Un, // 219 (0xDB)
			Endfinally, // 220 (0xDC)
			Leave, // 221 (0xDD)
			Leave_S, // 222 (0xDE)
			Stind_I, // 223 (0xDF)
			Conv_U, // 224 (0xE0)
			null, // 225 (0xE1)
			null, // 226 (0xE2)
			null, // 227 (0xE3)
			null, // 228 (0xE4)
			null, // 229 (0xE5)
			null, // 230 (0xE6)
			null, // 231 (0xE7)
			null, // 232 (0xE8)
			null, // 233 (0xE9)
			null, // 234 (0xEA)
			null, // 235 (0xEB)
			null, // 236 (0xEC)
			null, // 237 (0xED)
			null, // 238 (0xEE)
			null, // 239 (0xEF)
			null, // 240 (0xF0)
			null, // 241 (0xF1)
			null, // 242 (0xF2)
			null, // 243 (0xF3)
			null, // 244 (0xF4)
			null, // 245 (0xF5)
			null, // 246 (0xF6)
			null, // 247 (0xF7)
			Prefix7, // 248 (0xF8)
			Prefix6, // 249 (0xF9)
			Prefix5, // 250 (0xFA)
			Prefix4, // 251 (0xFB)
			Prefix3, // 252 (0xFC)
			Prefix2, // 253 (0xFD)
			Prefix1, // 254 (0xFE)
			Prefixref, // 255 (0xFF)

			// Two byte opcodes
			Arglist, // 0 (0x0)
			Ceq, // 1 (0x1)
			Cgt, // 2 (0x2)
			Cgt_Un, // 3 (0x3)
			Clt, // 4 (0x4)
			Clt_Un, // 5 (0x5)
			Ldftn, // 6 (0x6)
			Ldvirtftn, // 7 (0x7)
			null, // 8 (0x8)
			Ldarg, // 9 (0x9)
			Ldarga, // 10 (0xA)
			Starg, // 11 (0xB)
			Ldloc, // 12 (0xC)
			Ldloca, // 13 (0xD)
			Stloc, // 14 (0xE)
			Localloc, // 15 (0xF)
			null, // 16 (0x10)
			Endfilter, // 17 (0x11)
			UnalignedPrefix, // 18 (0x12)
			VolatilePrefix, // 19 (0x13)
			TailPrefix, // 20 (0x14)
			Initobj, // 21 (0x15)
			ConstrainedPrefix, // 22 (0x16)
			Cpblk, // 23 (0x17)
			Initblk, // 24 (0x18)
			NoPrefix, // 25 (0x19)
			Rethrow, // 26 (0x1A)
			null, // 27 (0x1B)
			Sizeof, // 28 (0x1C)
			Refanytype, // 29 (0x1D)
			ReadonlyPrefix, // 30 (0x1E)
		};

		#endregion
	}
}
