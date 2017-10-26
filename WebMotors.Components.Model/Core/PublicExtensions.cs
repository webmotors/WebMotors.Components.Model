using System;
using System.ComponentModel;
using System.Data.Common;

namespace WebMotors.Components.Model.Core
{
	public static class PublicExtensions
	{
		public static bool IsNullableType(Type _type)
		{
			return (_type.IsGenericType && _type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
		}

		public static T GetReaderValue<T>(this DbDataReader _dbDataReader, string _columnName)
		{
			if (!string.IsNullOrWhiteSpace(_columnName))
			{
				try
				{
					object _value = _dbDataReader[_columnName];

					if (DBNull.Value != _value)
					{
						Type _type = typeof(T);
						if (_type.IsEnum)
						{
							if (_value is char || _value is string)
								return GetEnumFromChar<T>(_value);
							return (T)Enum.Parse(_type, _value.ToString());
						}
						else
						{
							if (IsNullableType(_type))
								return (T)Convert.ChangeType(_value, new NullableConverter(_type).UnderlyingType);
							return (T)Convert.ChangeType(_value, _type);
						}
					}
				}
				catch { }
			}

			return default(T);
		}

		private static T GetEnumFromChar<T>(object _value)
		{
			try
			{
				sbyte v = (sbyte)char.Parse(_value.ToString());
				return (T)Enum.Parse(typeof(T), v.ToString());
			}
			catch { }
			return default(T);
		}
	}
}
