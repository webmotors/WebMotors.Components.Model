using System;
using System.Collections.Generic;
using System.Text;

namespace WebMotors.Components.Model.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class MapperTableAttribute : Attribute
	{
		public MapperTableAttribute()
		{
			Table = string.Empty;
			PK = string.Empty;
			PKIdentity = true;
		}

		public string Table { get; set; }
		public string PK { get; set; }
		public bool PKIdentity { get; set; }
	}
}
