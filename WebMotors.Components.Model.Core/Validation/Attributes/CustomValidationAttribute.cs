using System;
using System.Reflection;

namespace WebMotors.Components.Model.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public class CustomValidationAttribute : BaseAttribute
	{
		#region [ +Construtores ]

		public CustomValidationAttribute(string message, string methodName, string method = "", params PersistenceType[] types)
			: base(message, method, types)
		{
			_methodName = methodName;
		}

		#endregion

		private string _methodName = string.Empty;
		public string MethodName
		{
			get { return _methodName; }
			set { _methodName = value; }
		}

		#region [ +Métodos ]

		#region [ valid ]
		internal override void Valid(object value, ValidationResult validationResult)
		{
			if (value == null)
			{
				validationResult.Append(Message);
				return;
			}

			var methods = value.GetType().GetTypeInfo().GetMethods();
			foreach (var methodInfo in methods)
				if (methodInfo.Name == MethodName && methodInfo.ReturnType == typeof(ValidationResult))
				{
					object[] arguments = new object[1];
					arguments[0] = Database;
					validationResult.Append((ValidationResult)methodInfo.Invoke(value, arguments));
					return;
				}

			validationResult.Append("MethodName Not Implemented");
		}
		#endregion

		#endregion
	}
}
