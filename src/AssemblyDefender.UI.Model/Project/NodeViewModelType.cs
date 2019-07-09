using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model.Project
{
	public enum NodeViewModelType
	{
		None,
		Project,
		Assembly,
		Module,
		Namespace,
		Type,
		Method,
		Field,
		Property,
		Event,
		ResourceFolder,
		Resource,
		Last = Resource,
	}
}
