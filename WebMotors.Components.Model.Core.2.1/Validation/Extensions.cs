using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebMotors.Components.Model.Core;
using WebMotors.Components.Model.Validation.Attributes;

namespace WebMotors.Components.Model.Validation
{
	public static class Extensions
	{
		#region [ Extensoes internas ]
		internal static object GetDefaultValue(this object value)
		{
			if (value != null && value.GetType().GetTypeInfo().IsValueType)
				return Activator.CreateInstance(value.GetType());
			return null;
		}
		#endregion

		#region [ Extensoes publicas ]
		internal static ValidationResult IsValid<T>(this T entity, PersistenceType type, Database database, string method)
		{
			ValidationResult result = new ValidationResult();

			Type entityType = entity.GetType();
			PropertyInfo[] propriedades = entityType.GetProperties();

			foreach (PropertyInfo p in propriedades)
			{
				var attributes = p.GetCustomAttributes(false);
				object value = p.GetValue(entity, null);
				ValidItem(value, type, database, attributes, result, method);
			}

			var classAttr = entity.GetType().GetTypeInfo().GetCustomAttributes(false);
			ValidItem(entity, type, database, classAttr, result, method);

			return result;
		}

		private static void ValidItem(object value, PersistenceType type, Database database, object[] attributes, ValidationResult result, string method)
		{
			foreach (object attribute in attributes)
			{
				if (attribute is BaseAttribute && ((BaseAttribute)attribute).ValidType(type) && (string.IsNullOrWhiteSpace(((BaseAttribute)attribute).Method) || ((BaseAttribute)attribute).Method.Split(',').Contains(method)))
				{
					if (database != null)
						((BaseAttribute)attribute).Database = database;

					((BaseAttribute)attribute).Valid(value, result);
				}
			}
		}
		#endregion
	}
}
