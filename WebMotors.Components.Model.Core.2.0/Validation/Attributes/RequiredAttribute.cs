using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class RequiredAttribute : BaseAttribute
	{
		#region [ +Construtores ]

		public RequiredAttribute(string message, string method = "", params PersistenceType[] types)
			: base(message, method, types)
		{
		}

		#endregion

		#region [ +Métodos ]

		#region [ valid ]
		internal override void Valid(object value, ValidationResult validationResult)
		{
			if (value.GetDefaultValue().Equals(value))
			{
				validationResult.Append(Message);
			}
		}
		#endregion

		#endregion
	}
}
