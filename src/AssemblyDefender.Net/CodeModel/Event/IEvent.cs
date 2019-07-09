using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public interface IEvent : ICodeNode, IEventSignature, IEventBase
	{
		new IType EventType
		{
			get;
		}

		new IType Owner
		{
			get;
		}

		new IMethod AddMethod
		{
			get;
		}

		new IMethod RemoveMethod
		{
			get;
		}

		new IMethod InvokeMethod
		{
			get;
		}

		IEvent DeclaringEvent
		{
			get;
		}
	}
}
