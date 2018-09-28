using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
	public class RegularExpressionAttribute : BaseAttribute
	{
		#region [ +Construtores ]

		public RegularExpressionAttribute(string message, string regularExpression, string method = "", params PersistenceType[] types)
			: base(message, method, types)
		{
			_regularExpression = regularExpression;
		}

		#endregion

		private string _regularExpression = string.Empty;
		public string RegularExpression
		{
			get { return _regularExpression; }
			set { _regularExpression = value; }
		}

		#region [ +Métodos ]

		#region [ valid ]
		internal override void Valid(object value, ValidationResult validationResult)
		{
			Regex reg = new Regex(_regularExpression);
			if (value == null || !reg.IsMatch(value.ToString()))
			{
				validationResult.Append(Message);
			}
		}
		#endregion

		#endregion
	}
}
