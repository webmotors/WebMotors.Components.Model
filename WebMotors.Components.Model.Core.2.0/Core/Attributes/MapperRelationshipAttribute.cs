using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class MapperRelationshipAttribute : MapperBaseAttribute
	{
		public MapperRelationshipAttribute()
		{
			AutomaticGet = false;
			Mode = PersistenceMode.Never;
			Procedures = string.Empty;
			InnerJoin = true;
		}

		public Type ClassRealtionship { get; set; }
		public string ForeignKey { get; set; }
		public bool InnerJoin { get; set; }
		public PersistenceMode Mode { get; set; }
	}

	public enum PersistenceMode
	{
		Never,
		OnlyNotNull,
		Before,
		After
	}
}
