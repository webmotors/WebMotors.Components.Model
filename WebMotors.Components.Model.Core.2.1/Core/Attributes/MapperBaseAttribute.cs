using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Core.Attributes
{
	public abstract class MapperBaseAttribute : Attribute
	{
		public bool AutomaticGet { get; set; }
		public string Procedures { get; set; }

		public bool IsValid(string procedure = "get")
		{
			List<string> procedures = new List<string>();
			if (!string.IsNullOrWhiteSpace(Procedures))
				procedures = new List<string>(Procedures.ToLower().Split(','));
			return ((procedures.Count == 0 && procedure.Equals("get")) || procedures.Contains(procedure.ToLower()));
		}
	}
}
