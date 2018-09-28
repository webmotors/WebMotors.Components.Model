using Newtonsoft.Json;
using System;
using System.Reflection;

namespace WebMotors.Components.Model.Core
{
	internal static class Extensions
	{
		#region [ ICustomAttributeProvider ]
		public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider) where T : Attribute
		{
			return GetCustomAttributes<T>(provider, true);
		}

		public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			T[] attributes = provider.GetCustomAttributes(typeof(T), inherit) as T[];
			if (attributes == null)
			{   // WORKAROUND: Due to a bug in the code for retrieving attributes
				// from a dynamic generated parameter, GetCustomAttributes can return
				// an instance of an object[] instead of T[], and hence the cast above
				// will return null.

				return new T[0];
			}

			return attributes;
		}
		#endregion

		#region [ BaseEntity ]
		public static string DRNameChar(this BaseEntity obj, string valor)
		{
			return string.Format("{0}_{1}", valor, Convert.ToChar(Convert.ToByte(obj.Alias)));
		}
		public static char CharAlias(this BaseEntity obj)
		{
			return Convert.ToChar(Convert.ToByte(obj.Alias));
		}
		public static char CharAliasForeinKey(this NoGetEntity obj)
		{
			return Convert.ToChar(Convert.ToByte(obj.AliasForeinKey));
		}
		#endregion

		public static object GetPropValue(this object src, string propName)
		{
			return src.GetType().GetProperty(propName).GetValue(src, null);
		}

		public static T CopyObject<T>(this T item)
		{
			if (item != null)
				return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(item));
			else
				return default(T);
		}

		public static string FormatStr(this string obj, params object[] args)
		{
			return string.Format(obj, args);
		}
	}
}
