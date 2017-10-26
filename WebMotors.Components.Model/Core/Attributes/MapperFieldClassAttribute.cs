using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class MapperFieldClassAttribute : MapperBaseAttribute
	{
		#region [ +Construtores ]
		public MapperFieldClassAttribute()
		{
			Procedures = string.Empty;
			Field = string.Empty;
		}
		#endregion

		public string Field { get; set; }
		public DbType Type { get; set; }
		public object Value { get; set; }
	}
}
