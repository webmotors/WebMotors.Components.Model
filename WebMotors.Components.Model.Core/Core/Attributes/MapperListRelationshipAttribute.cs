using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class MapperListRelationshipAttribute : MapperBaseAttribute
	{
		public MapperListRelationshipAttribute()
		{
			AutomaticGet = false;
			AutomaticSave = false;
			Procedures = string.Empty;
		}

		public Type ClassRelationship { get; set; }
		public string ForeignKey { get; set; }
		/// <summary>
		/// Not implemented yet - save child class
		/// </summary>
		public bool AutomaticSave { get; set; }
	}
}
