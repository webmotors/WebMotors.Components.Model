using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebMotors.Components.Model.Core
{
	#region [ BaseEntity ]
	[Serializable]
	internal class NoGetEntity : BaseEntity
	{
		public NoGetEntity()
		{
			Fields = new Dictionary<string, PropertyInfo>();
		}
		public int AliasForeinKey;
		public string ForeinKey;
	}

	[Serializable]
	internal class GetEntity : BaseEntity
	{
		public GetEntity()
		{
			Fields = new Dictionary<string, PropertyInfo>();
			Inner = new List<NoGetEntity>();
			Left = new List<NoGetEntity>();
			ListEntity = new List<PropertyInfo>();
			EntityFill = false;
		}
		public List<NoGetEntity> Inner;
		public List<NoGetEntity> Left;
		public List<PropertyInfo> ListEntity;
		public bool EntityFill;

		#region [ getSQLString ]
		public string getSQLString(Database database, string fieldFilter = "")
		{
			var sqlFieldsString = sqlFields();
			var sqlTablesString = sqlTables(this, database);
			if (string.IsNullOrWhiteSpace(fieldFilter))
				return string.Format("SELECT {0} FROM {1} WHERE A.{2} = @PK", sqlFieldsString, sqlTablesString, PK);
			else
				return string.Format("SELECT {0} FROM {1} WHERE A.{2} = @{2}", sqlFieldsString, sqlTablesString, fieldFilter);
		}

		private string sqlFields()
		{
			List<string> fields = new List<string>();
			sqlFields(this, fields);
			foreach (var entity in Inner)
				sqlFields(entity, fields);
			foreach (var entity in Left)
				sqlFields(entity, fields);
			return string.Join(", ", fields);
		}

		private void sqlFields(BaseEntity entity, List<string> fields)
		{
			fields.Add(string.Format("{0}.{1} AS '{2}'", entity.CharAlias(), entity.PK, entity.DRNameChar(entity.PK)));
			foreach (var field in entity.Fields)
				fields.Add(string.Format("{0}.{1} AS '{2}'", entity.CharAlias(), field.Key, entity.DRNameChar(field.Key)));
		}

		private string sqlTables(GetEntity getEntity, Database database)
		{
			var result = string.Format("{0} {1}{2}", getEntity.Table, getEntity.CharAlias(), database.getNOLOCKString);
			foreach (var entity in getEntity.Inner)
				result = sqlTables(entity, result, "INNER", database);
			foreach (var entity in getEntity.Left)
				result = sqlTables(entity, result, "LEFT", database);
			return result;
		}

		private string sqlTables(NoGetEntity entity, string result, string joinType, Database database)
		{
			return string.Format("{0} {1} JOIN {2} {3}{4} ON {5}.{6} = {3}.{7}",
					result,
					joinType,
					entity.Table,
					entity.CharAlias(),
					database.getNOLOCKString,
					entity.CharAliasForeinKey(),
					entity.ForeinKey,
					entity.PK
			);
		}
		#endregion
	}

	[Serializable]
	internal class BaseEntity
	{
		public string PK;
		public PropertyInfo PKPropertyInfo;
		public object Instance;
		public int Alias;
		public string Table;
		public Dictionary<string, PropertyInfo> Fields;
	}
	#endregion
}
