using System;

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
			var defaultValue = value.GetDefaultValue();
			if
				(
					(defaultValue == null && value == null) ||
					(value != null && value.GetType().Equals(string.Empty.GetType()) && string.IsNullOrWhiteSpace(value.ToString())) ||
					(defaultValue != null && defaultValue.Equals(value))
				)
			{
				validationResult.Append(Message);
			}
		}
		#endregion

		#endregion
	}
}
