using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMotors.Components.Model.Core;

namespace WebMotors.Components.Model.Validation.Attributes
{
	public abstract class BaseAttribute : Attribute
	{
		#region [ +Construtores ]

		public BaseAttribute()
		{

		}

		public BaseAttribute(string message, string method, params PersistenceType[] types)
		{
			_types = types;
			_message = message;
			_method = method;
		}

		#endregion

		#region [ +Campos privados ]

		string _message;
		string _method;
		PersistenceType[] _types;
		Database _database;

		#endregion

		#region [ +Propriedades ]

		#region [ Message ]
		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}
		#endregion

		#region [ Method ]
		public string Method
		{
			get { return _method; }
			set { _method = value; }
		}
		#endregion

		#region [ PersistenceType ]
		public PersistenceType[] Types
		{
			get { return _types; }
			set { _types = value; }
		}
		#endregion

		#region [ Database ]
		public Database Database
		{
			get { return _database; }
			set { _database = value; }
		}
		#endregion

		#endregion

		#region [ +Métodos ]

		internal abstract void Valid(object value, ValidationResult validationResult);

		#region [ ValidType ]
		public bool ValidType(PersistenceType type)
		{
			foreach (PersistenceType t in this._types)
				if (t == type)
					return true;

			return false;
		}
		#endregion

		#endregion
	}
}
