using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class MapperFieldAttribute : MapperBaseAttribute
	{
		#region [ +Construtores ]
		public MapperFieldAttribute()
		{
			Procedures = string.Empty;
			Field = string.Empty;
			AutomaticGet = true;
		}
		#endregion

		public string Field { get; set; }
		public DbType Type { get; set; }
		public int MaxLength { get; set; }
	}
}
