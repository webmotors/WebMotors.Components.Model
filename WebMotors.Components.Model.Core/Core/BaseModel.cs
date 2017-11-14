using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using WebMotors.Components.Model.Core.Attributes;
using WebMotors.Components.Model.Validation;

namespace WebMotors.Components.Model.Core
{
	public abstract class BaseModel<TPk>
		where TPk : IComparable<TPk>
	{
		#region [ Constructor ]
		public BaseModel()
		{
		}
		public BaseModel(TPk pk)
		{
			PK = pk;
		} 
		#endregion

		#region [ PK ]
		[JsonIgnore]
		public TPk PK
		{
			get;
			internal set;
		}
		#endregion

		#region [ ValidationResult ]
		private ValidationResult validationResult;
		protected string UpdatePKDefaultError = "PK must be informed to execute update.";
		protected string DeletePKDefaultError = "PK must be informed to execute delete.";
		[JsonIgnore]
		public bool HasError
		{
			get
			{
				return validationResult != null && validationResult.HasError;
			}
		}
		[JsonIgnore]
		public List<string> Errors
		{
			get
			{
				if (validationResult != null)
					return validationResult.Errors;
				return new List<string>();
			}
		}

		protected void AppendError(ValidationResult _validationResult)
		{
			if (validationResult == null)
				validationResult = _validationResult;
			else
				validationResult.Append(_validationResult);
		}
		#endregion

		#region [ Protected Methods ]

		#region [ Static ]
		protected static T Get<T>(string connection, TPk pk, string procedure = "get")
			where T : BaseModel<TPk>
		{
			var getEntity = getEntity<T>(string.Empty, procedure);

			using (Database database = new Database(connection))
			{
				var sqlString = getEntity.getSQLString(database);
				ArrayList pParameters = SQLHelperEdited.AddParameter(database, "@Pk", pk);
				using (DbDataReader dr = SQLHelperEdited.ExecuteReader(database, sqlString, pParameters, CommandType.Text))
				{
					while (dr.Read())
					{
						setEntity(getEntity, dr);
					}
				}
			}

			GetListRelations<T>(getEntity, connection, procedure);

			if (getEntity.EntityFill) return (T)getEntity.Instance;
			return default(T);
		}

		protected static List<T> Find<T>(string connectionString, string sql, CommandType commandType, Dictionary<string, object> parameters, string procedure = null)
			where T : BaseModel<TPk>
		{
			using (Database database = new Database(connectionString))
			{
				ArrayList pParameters = new ArrayList();
				foreach (var item in parameters)
					SQLHelperEdited.AddParameter(database, ref pParameters, item.Key, item.Value);

				return select<T>(database, sql, pParameters, commandType);
			}
		}

		protected static PagingInfo<T> FindPaging<T>(string connectionString, string sql, CommandType commandType, Dictionary<string, object> parametros, int page, int pageSize, string orderBy = null, string procedure = null)
			where T : BaseModel<TPk>
		{
			using (Database database = new Database(connectionString))
			{
				ArrayList pParameters = new ArrayList();
				foreach (var item in parametros)
					SQLHelperEdited.AddParameter(database, ref pParameters, item.Key, item.Value);

				return selectPaging<T>(database, sql, pParameters, commandType, page, pageSize, orderBy, procedure);
			}
		}

		protected static Database CreateDatabase(string connection)
		{
			return new Database(connection);
		}

		public string getSQLString(string connection, string procedure = "get")
		{
			var result = string.Empty;
			var getEntity = getEntityType(procedure);
			using (Database database = new Database(connection, false))
			{
				result = getEntity.getSQLString(database);
				foreach (var p in getEntity.ListEntity)
				{
					var atributeListRelation = p.GetCustomAttributes<MapperListRelationshipAttribute>(false);

					foreach (var attribute in atributeListRelation)
					{
						if (attribute.AutomaticGet)
						{
							var getEntityListRelation = getEntityType(attribute.ClassRelationship, procedure, attribute.ForeignKey);
							result = string.Concat(result, Environment.NewLine, getEntityListRelation.getSQLString(database, attribute.ForeignKey));
						}
					}
				}
			}
			return result;
		}

		public dynamic getSQLSaveString(string connection, string procName = "Insert")
		{
			using (Database database = new Database(connection, false))
			{
				var table = string.Empty;
				bool pkIdentity = true;
				var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
				foreach (var atributo in atributos)
				{
					table = atributo.Table;
					pkIdentity = atributo.PKIdentity;
				}
				var oDictionary = GetEntityParametersDictionary(this, procName);
				ArrayList pParameters = null;
				var sql = getInsertSQL(database, table, oDictionary, out pParameters, pkIdentity);
				return new { SQL = sql, Parameters = pParameters };
			}
		}

		public dynamic getSQLUpdateString(string connection, string procName = "Update")
		{
			using (Database database = new Database(connection, false))
			{
				var table = string.Empty;
				var pk = string.Empty;
				var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
				foreach (var atributo in atributos)
				{
					table = atributo.Table;
					pk = atributo.PK;
				}
				var oDictionary = GetEntityParametersDictionary(this, procName);

				var oDictionaryWhere = new Dictionary<string, object>();
				oDictionaryWhere.Add(pk, PK);

				ArrayList pParameters = null;
				var sql = getUpdateSQL(database, table, oDictionary, oDictionaryWhere, out pParameters);
				return new { SQL = sql, Parametros = pParameters };
			}
		}

		public dynamic getSQLDeleteString(string connection, string procName = "Delete")
		{
			using (Database database = new Database(connection, false))
			{
				var table = string.Empty;
				var pk = string.Empty;
				var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
				foreach (var atributo in atributos)
				{
					table = atributo.Table;
					pk = atributo.PK;
				}
				var oDictionaryWhere = new Dictionary<string, object>();
				oDictionaryWhere.Add(pk, PK);
				ArrayList pParameters = null;
				var sql = getDeleteSQL(database, table, oDictionaryWhere, out pParameters);
				return new { SQL = sql, Parametros = pParameters };
			}
		}
		#endregion

		#region [ Persistence ]
		protected ValidationResult Delete(Database database, string procName = "Delete", bool ignoreValidation = false)
		{
			if (this.PK.CompareTo(default(TPk)) == 0)
			{
				validationResult = new ValidationResult();
				validationResult.Append(DeletePKDefaultError);
				return validationResult;
			}

			validationResult = this.IsValid(PersistenceType.Delete, database, procName);
			if (validationResult.HasError && !ignoreValidation)
				return validationResult;

			var table = string.Empty;
			string pk = string.Empty;
			var attributes = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var attribute in attributes)
			{
				table = attribute.Table;
				pk = attribute.PK;
			}

			var pParametersWhere = new Dictionary<string, object>();
			pParametersWhere.Add(pk, this.PK);
			var result = delete(database, table, pParametersWhere);
			if (result == 1)
				this.PK = default(TPk);
			else
				throw new InvalidOperationException("record not found in the database");
			return validationResult;
		}

		protected ValidationResult SaveOrUpdate(Database database, string procName = "", bool ignoreValidation = false)
		{
			PersistenceType persistenceType = PersistenceType.Insert;
			if (this.PK.CompareTo(default(TPk)) != 0)
			{
				persistenceType = PersistenceType.Update;
				procName = string.IsNullOrWhiteSpace(procName) ? "Update" : procName;
			}
			else
				procName = string.IsNullOrWhiteSpace(procName) ? "Insert" : procName;

			validationResult = this.IsValid(persistenceType, database, procName);
			if (validationResult.HasError && !ignoreValidation)
				return validationResult;

			if (persistenceType == PersistenceType.Update)
			{
				if (this.PK.CompareTo(default(TPk)) == 0)
				{
					validationResult = new ValidationResult();
					validationResult.Append(UpdatePKDefaultError);
					return validationResult;
				}
				this.Update(database, procName);
			}
			else
				this.Insert(database, procName);
			return validationResult;
		}

		protected ValidationResult Save(Database database, string procName = "", bool ignoreValidation = false)
		{
			PersistenceType persistenceType = PersistenceType.Insert;
			procName = string.IsNullOrWhiteSpace(procName) ? "Insert" : procName;

			validationResult = this.IsValid(persistenceType, database, procName);
			if (validationResult.HasError && !ignoreValidation)
				return validationResult;

			this.Insert(database, procName);
			return validationResult;
		}

		protected ValidationResult Update(Database database, string procName = "", bool ignoreValidation = false)
		{
			if (this.PK.CompareTo(default(TPk)) == 0)
			{
				validationResult = new ValidationResult();
				validationResult.Append(UpdatePKDefaultError);
			}
			else
			{
				PersistenceType persistenceType = PersistenceType.Update;
				procName = string.IsNullOrWhiteSpace(procName) ? "Update" : procName;

				validationResult = this.IsValid(persistenceType, database, procName);
				if (validationResult.HasError && !ignoreValidation)
					return validationResult;

				this.Update(database, procName);
			}
			return validationResult;
		}
		#endregion

		#endregion

		#region [ Private Methods ]

		#region [ Persistence ]
		private void Insert(Database database, string procName = "Insert")
		{
			if (database.isElasticSearch) { SaveElasticSearch(database, procName); return; }
			var table = string.Empty;
			var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			bool pkIdentity = true;
			foreach (var atributo in atributos)
			{
				table = atributo.Table;
				pkIdentity = atributo.PKIdentity;
			}
			var pParameters = GetEntityParametersDictionary(this, procName);
			var pkValue = insert(database, table, pParameters, pkIdentity);
			if (pkValue.CompareTo(default(TPk)) != 0)
				PK = (TPk)Convert.ChangeType(pkValue, typeof(TPk));
		}

		private void Update(Database database, string procName = "Update")
		{
			if (database.isElasticSearch) { SaveElasticSearch(database, procName); return; }
			var table = string.Empty;
			string pk = string.Empty;
			var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var atributo in atributos)
			{
				table = atributo.Table;
				pk = atributo.PK;
			}
			var pParametersCampos = GetEntityParametersDictionary(this, procName);
			if (pParametersCampos.ContainsKey(pk))
				pParametersCampos.Remove(pk);

			var pParametersWhere = new Dictionary<string, object>();
			pParametersWhere.Add(pk, PK);

			update(database, table, pParametersCampos, pParametersWhere);
		}

		private void SaveElasticSearch(Database database, string procName)
		{
			var table = string.Empty;
			var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var atributo in atributos)
			{
				table = atributo.Table;
			}
			var command = database.CreateCommand();
			var method = command.GetType().GetMethod("InsertOrUpdate");
			var obj = GetEntityParametersDictionary(this, procName);
			method.MakeGenericMethod(new Type[] { obj.GetType(), typeof(TPk) }).Invoke(command, new object[] { this.PK, obj, table });
		}

		private TPk insert(Database database, string pNameTable, Dictionary<string, object> oDictionary, bool pkIdentity)
		{
			ArrayList pParameters = null;
			var sql = getInsertSQL(database, pNameTable, oDictionary, out pParameters, pkIdentity);
			if (pkIdentity)
				return (TPk)Convert.ChangeType(SQLHelperEdited.ExecuteScalar(database, sql, pParameters, CommandType.Text), typeof(TPk));
			else
				SQLHelperEdited.ExecuteNonQuery(database, sql, pParameters, CommandType.Text);
			return default(TPk);
		}

		private string getInsertSQL(Database database, string pNameTable, Dictionary<string, object> oDictionary, out ArrayList pParameters, bool pkIdentity)
		{
			StringBuilder oStringBuilder = new StringBuilder();
			pParameters = new ArrayList();
			foreach (var item in oDictionary)
				SQLHelperEdited.AddParameter(database, ref pParameters, string.Format("@C{0}", item.Key), item.Value);
			oStringBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2}); {3}", pNameTable, BuildInsert(oDictionary), BuildInsertValues(oDictionary), pkIdentity ? database.ScopeIdentity : string.Empty);
			return oStringBuilder.ToString();
		}

		private void update(Database database, string pNameTable, Dictionary<string, object> oDictionary, Dictionary<string, object> oDictionaryWhere)
		{
			ArrayList pParameters = new ArrayList();
			var sql = getUpdateSQL(database, pNameTable, oDictionary, oDictionaryWhere, out pParameters);
			SQLHelperEdited.ExecuteNonQuery(database, sql, pParameters, CommandType.Text);
		}

		private string getUpdateSQL(Database database, string pNameTable, Dictionary<string, object> oDictionary, Dictionary<string, object> oDictionaryWhere, out ArrayList pParameters)
		{
			StringBuilder oStringBuilder = new StringBuilder();
			pParameters = new ArrayList();
			foreach (var item in oDictionary)
				SQLHelperEdited.AddParameter(database, ref pParameters, string.Format("@C{0}", item.Key), item.Value);

			foreach (var item in oDictionaryWhere)
				SQLHelperEdited.AddParameter(database, ref pParameters, string.Format("@W{0}", item.Key), item.Value);

			oStringBuilder.AppendFormat("UPDATE {0} SET {1} WHERE {2}", pNameTable, BuildUpdate(oDictionary), BuildWhere(oDictionaryWhere));
			return oStringBuilder.ToString();
		}

		private int delete(Database database, string pNameTable, Dictionary<string, object> oDictionaryWhere)
		{
			if (database.isElasticSearch) { return DeleteElasticSearch(database); }
			ArrayList pParameters = new ArrayList();
			var sql = getDeleteSQL(database, pNameTable, oDictionaryWhere, out pParameters);
			return SQLHelperEdited.ExecuteNonQuery(database, sql, pParameters, CommandType.Text);
		}

		private int DeleteElasticSearch(Database database)
		{
			var table = string.Empty;
			var atributos = this.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var atributo in atributos)
			{
				table = atributo.Table;
			}
			var command = database.CreateCommand();
			var method = command.GetType().GetMethod("Delete");
			method.MakeGenericMethod(new Type[] { typeof(TPk) }).Invoke(command, new object[] { this.PK, table });
			return 1;
		}

		private string getDeleteSQL(Database database, string pNameTable, Dictionary<string, object> oDictionaryWhere, out ArrayList pParameters)
		{
			StringBuilder oStringBuilder = new StringBuilder();

			pParameters = new ArrayList();
			foreach (var item in oDictionaryWhere)
				SQLHelperEdited.AddParameter(database, ref pParameters, string.Format("@W{0}", item.Key), item.Value);

			oStringBuilder.AppendFormat("DELETE FROM {0} WHERE {1}", pNameTable, BuildWhere(oDictionaryWhere));
			return oStringBuilder.ToString();
		}
		#endregion

		#region [ Query DB ]
		private static List<T> select<T>(Database database, string sql, ArrayList pParameters, CommandType commandType, string procedure = null)
		{
			List<T> retorno = new List<T>();
			using (DbDataReader dr = SQLHelperEdited.ExecuteReader(database, sql, pParameters, commandType))
			{
				while (dr.Read())
				{
					retorno.Add(FillEntity<T>(dr, default(T), procedure));
				}
			}
			return retorno;
		}

		private static PagingInfo<T> selectPaging<T>(Database database, string sql, ArrayList pParameters, CommandType commandType, int page, int pageSize, string orderBy, string procedure)
		{
			List<T> retorno = new List<T>();
			int pageCount = 0;
			int totalRecords = 0;
			using (DbDataReader dr = SQLHelperEdited.ExecuteReader(database, database.SqlStringPaging(sql, page, pageSize, orderBy), pParameters, commandType))
			{
				while (dr.Read())
				{
					retorno.Add(FillEntity<T>(dr, default(T), procedure));
					totalRecords = database.InWhileDR(dr);
				}
				totalRecords = database.AfterWhileDR(dr, totalRecords);
			}
			pageCount = Convert.ToInt32((Math.Floor(Convert.ToDecimal(totalRecords) / pageSize)));
			if ((Convert.ToDecimal(totalRecords) % pageSize) > 0)
				pageCount++;
			return new PagingInfo<T>(retorno, pageCount, totalRecords);
		}
		#endregion

		#region [ Build Insert ]
		private static string BuildInsert(Dictionary<string, object> oDictionary)
		{
			return BuildFields(oDictionary, false, false, false);
		}

		private static string BuildInsertValues(Dictionary<string, object> oDictionary)
		{
			return BuildFields(oDictionary, true, false, false);
		}

		private static string BuildUpdate(Dictionary<string, object> oDictionary)
		{
			return BuildFields(oDictionary, false, true, false);
		}

		private static string BuildWhere(Dictionary<string, object> oDictionary)
		{
			return BuildFields(oDictionary, false, true, true);
		}

		private static string BuildFields(Dictionary<string, object> oDictionary, bool oUseArroba, bool oIsUpdate, bool oIsWhere)
		{
			string result = string.Empty;
			foreach (KeyValuePair<string, object> oKeyValuePair in oDictionary)
				result += (oIsUpdate) ? string.Format("{0}{1} = @{2}{1}, ", ((oIsWhere) ? "AND " : string.Empty), oKeyValuePair.Key, ((oIsWhere) ? "W" : "C")) : string.Format("{0}{1}, ", ((oUseArroba) ? "@C" : string.Empty), oKeyValuePair.Key);
			return (oIsWhere) ? result.Remove(0, 4).Replace(",", "") : result.Remove(result.Length - 2, 2);
		}
		#endregion

		#region [ Entity ]

		#region [ Get ]
		private static void GetListRelations<T>(GetEntity getEntity, string connection, string procedure = "Get")
			where T : BaseModel<TPk>
		{
			foreach (var p in getEntity.ListEntity)
			{
				var atributeListRelation = p.GetCustomAttributes<MapperListRelationshipAttribute>(false);

				foreach (var attribute in atributeListRelation)
				{
					if (attribute.AutomaticGet)
					{
						IListEntity listEntity = (IListEntity)Activator.CreateInstance(p.PropertyType);
						listEntity.Set();
						p.SetValue(getEntity.Instance, listEntity);
						var getEntityListRelation = getEntityType(attribute.ClassRelationship, procedure, attribute.ForeignKey);

						using (Database database = new Database(connection))
						{
							var sqlString = getEntityListRelation.getSQLString(database, attribute.ForeignKey);
							ArrayList pParameters = SQLHelperEdited.AddParameter(database, attribute.ForeignKey, ((T)getEntity.Instance).PK);
							using (DbDataReader dr = SQLHelperEdited.ExecuteReader(database, sqlString, pParameters, CommandType.Text))
							{
								while (dr.Read())
								{
									var item = getEntityListRelation.CopyObject();
									setEntity(item, dr);
									listEntity.Add(item.Instance);
									setForeinKeyEntity(item.Instance, getEntity.Instance, attribute.ForeignKey);
								}
							}
						}
						listEntity.EndFill();
					}
				}
			}
		}

		private static void setForeinKeyEntity(object child, object parent, string foreinKey)
		{
			bool propertySet = false;
			foreach (PropertyInfo p in child.GetType().GetProperties())
			{
				var attributeRelation = p.GetCustomAttributes<MapperRelationshipAttribute>(false);

				foreach (var attribute in attributeRelation)
				{
					if (attribute.ForeignKey == foreinKey && parent.GetType() == attribute.ClassRealtionship)
					{
						p.SetValue(child, parent);
						propertySet = true;
						break;
					}
				}

				if (propertySet) break;
			}
		}
		#endregion

		#region [ setEntity ]
		private static void setEntity(GetEntity getEntity, DbDataReader dr)
		{
			setEntityProp(getEntity, dr);
			foreach (var entity in getEntity.Inner)
				setEntityProp(entity, dr);
			foreach (var entity in getEntity.Left)
				setEntityProp(entity, dr);
			getEntity.EntityFill = true;
		}

		private static void setEntityProp(BaseEntity entity, DbDataReader dr)
		{
			setEntityPropValue(dr, entity.PKPropertyInfo, entity.Instance, entity.DRNameChar(entity.PK));
			foreach (var field in entity.Fields)
			{
				setEntityPropValue(dr, field.Value, entity.Instance, entity.DRNameChar(field.Key));
			}
		}

		private static void setEntityPropValue(DbDataReader dr, PropertyInfo p, object instance, string drName)
		{
			object valorDr = dr[drName];
			setEntityPropValue(p, instance, valorDr);
		}

		private static void setEntityPropValue(PropertyInfo p, object instance, object value)
		{
			if (value != DBNull.Value)
			{
				if (!p.PropertyType.GetTypeInfo().IsEnum)
				{
					object valorConvertido = null;
					if (p.PropertyType.GetTypeInfo().IsGenericType && p.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
						valorConvertido = Convert.ChangeType(value, Nullable.GetUnderlyingType(p.PropertyType));
					else
						valorConvertido = Convert.ChangeType(value, p.PropertyType);
					p.SetValue(instance, valorConvertido, null);
				}
				else
				{
					Type objType = p.PropertyType;
					if (value is char || value is string)
						p.SetValue(instance, GetEnumFromChar(value, objType), null);
					else
						p.SetValue(instance, Enum.Parse(p.PropertyType, value.ToString()), null);
				}
			}
		}
		#endregion

		#region [ getEntity ]
		private static GetEntity getEntity<T>(string foreinKey, string procedure)
		{
			return getEntityType(typeof(T), procedure);
		}

		private GetEntity getEntityType(string procedure = "get")
		{
			return getEntityType(this.GetType(), procedure);
		}

		private static GetEntity getEntityType(Type type, string procedure, string foreinKey = "")
		{
			int alias = 65; // First table will have Alias A
			GetEntity getEntity = new GetEntity { Alias = alias };

			getEntity.Instance = Activator.CreateInstance(type);
			getEntityBaseAttributes(getEntity, getEntity.Instance);

			foreach (PropertyInfo p in getEntity.Instance.GetType().GetProperties())
			{
				getFields(getEntity, p, procedure);
				getRelations(getEntity, p, getEntity.Instance, false, ref alias, foreinKey, procedure);
				getListRelations(getEntity, p, procedure);
			}

			return getEntity;
		}

		private static void getLeft(GetEntity getEntity, NoGetEntity noGetEntity, ref int alias, string procedure)
		{
			getEntity.Left.Add(noGetEntity);
			getEntityBaseAttributes(noGetEntity, noGetEntity.Instance);

			foreach (PropertyInfo p in noGetEntity.Instance.GetType().GetProperties())
			{
				getFields(noGetEntity, p, procedure);
				getRelations(getEntity, p, noGetEntity.Instance, true, ref alias, string.Empty, procedure);
			}
		}

		private static void getInner(GetEntity getEntity, NoGetEntity noGetEntity, ref int alias, string procedure)
		{
			getEntity.Inner.Add(noGetEntity);
			getEntityBaseAttributes(noGetEntity, noGetEntity.Instance);

			foreach (PropertyInfo p in noGetEntity.Instance.GetType().GetProperties())
			{
				getFields(noGetEntity, p, procedure);
				getRelations(getEntity, p, noGetEntity.Instance, false, ref alias, string.Empty, procedure);
			}
		}

		private static void getEntityBaseAttributes(BaseEntity entity, object instance)
		{
			var atributos = instance.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var atributo in atributos)
			{
				entity.Table = atributo.Table;
				entity.PK = atributo.PK;
			}
		}

		private static void getFields(BaseEntity entity, PropertyInfo p, string procedure)
		{
			if (p.Name.ToLower() == "pk")
				entity.PKPropertyInfo = p;
			else
			{
				var atributeProp = p.GetCustomAttributes<MapperFieldAttribute>(false);

				foreach (var attribute in atributeProp)
				{
					if (attribute.IsValid(procedure))
						entity.Fields.Add(attribute.Field, p);
				}
			}
		}

		private static void getRelations(GetEntity getEntity, PropertyInfo p, object instance, bool left, ref int alias, string foreinKey = "", string procedure = "")
		{
			var attributeRelation = p.GetCustomAttributes<MapperRelationshipAttribute>(false);

			foreach (var attribute in attributeRelation)
			{
				if (attribute.IsValid(procedure) && attribute.AutomaticGet && (!left || !attribute.InnerJoin) && (string.IsNullOrWhiteSpace(foreinKey) || attribute.ForeignKey != foreinKey))
				{
					alias++;
					NoGetEntity noGetEntity = new NoGetEntity { Alias = alias, ForeinKey = attribute.ForeignKey, AliasForeinKey = getEntity.Alias };
					noGetEntity.Instance = Activator.CreateInstance(attribute.ClassRealtionship);
					if (attribute.InnerJoin && !left)
						getInner(getEntity, noGetEntity, ref alias, procedure);
					else if (!attribute.InnerJoin)
						getLeft(getEntity, noGetEntity, ref alias, procedure);
					p.SetValue(instance, noGetEntity.Instance, null);
				}
			}
		}

		private static void getListRelations(GetEntity getEntity, PropertyInfo p, string procedure)
		{
			var atributeListRelation = p.GetCustomAttributes<MapperListRelationshipAttribute>(false);

			foreach (var attribute in atributeListRelation)
			{
				if (attribute.IsValid(procedure) && attribute.AutomaticGet)
					getEntity.ListEntity.Add(p);
			}
		}
		#endregion

		#region [ FillEntity ]
		public void FillEntity(Dictionary<string, object> fields, string procedure = null)
		{
			Dictionary<string, object> _fields = new Dictionary<string, object>();
			if (fields != null)
				foreach (var item in fields)
					if (item.Key != null)
						_fields.Add(item.Key.ToLower(), item.Value);

			PropertyInfo[] properties = this.GetType().GetProperties();

			foreach (PropertyInfo p in properties)
			{
				var attributes = p.GetCustomAttributes(typeof(MapperFieldAttribute), false);
				foreach (object attribute in attributes)
				{
					if (attribute is MapperFieldAttribute)
					{
						List<string> procedures = new List<string>(((MapperFieldAttribute)attribute).Procedures.ToLower().Split(','));
						if (string.IsNullOrWhiteSpace(procedure) || procedures.Contains(procedure.ToLower()))
						{
							if (_fields.ContainsKey(((MapperFieldAttribute)attribute).Field.ToLower()))
								setEntityPropValue(p, this, _fields[((MapperFieldAttribute)attribute).Field.ToLower()]);
						}
					}
				}
			}
		}

		protected static T FillEntity<T>(DbDataReader dr, T instance = default(T), string procedure = null)
		{
			if (instance == null)
				instance = (T)Activator.CreateInstance(typeof(T), false);

			Dictionary<string, string> objCampos = new Dictionary<string, string>();

			int numCampos = dr.FieldCount;
			for (int i = 0; i < numCampos; i++)
				objCampos.Add(dr.GetName(i).ToLower(), dr.GetName(i).ToLower());

			SetFieldDataReader<T>(dr, (T)instance, objCampos, procedure);
			return (T)instance;
		}

		private static void SetFieldDataReader<T>(DbDataReader dr, T instance, Dictionary<string, string> fields, string procedure)
		{
			var entityFields = new Dictionary<string, string>();
			var classAttr = instance.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var atributo in classAttr)
			{
				if (!string.IsNullOrWhiteSpace(atributo.PK) && fields.ContainsKey(atributo.PK.ToLower()))
					entityFields.Add("pk", atributo.PK.ToLower());
			}

			PropertyInfo[] properties = instance.GetType().GetProperties();

			foreach (PropertyInfo p in properties)
			{
				var attributes = p.GetCustomAttributes(typeof(MapperFieldAttribute), false);
				foreach (object attribute in attributes)
				{
					if (attribute is MapperFieldAttribute)
					{
						List<string> procedures = new List<string>(((MapperFieldAttribute)attribute).Procedures.ToLower().Split(','));
						if (string.IsNullOrWhiteSpace(procedure) || procedures.Contains(procedure.ToLower()))
						{
							if (fields.ContainsKey(((MapperFieldAttribute)attribute).Field.ToLower()))
								entityFields.Add(p.Name.ToLower(), ((MapperFieldAttribute)attribute).Field.ToLower());
						}
					}
				}

				attributes = p.GetCustomAttributes(typeof(MapperRelationshipAttribute), false);
				foreach (object attribute in attributes)
				{
					var instancia2 = Activator.CreateInstance(((MapperRelationshipAttribute)attribute).ClassRealtionship);
					SetFieldDataReader(dr, instancia2, fields, procedure);
					p.SetValue(instance, instancia2, null);
					break;
				}
			}

			SetValueFieldDataReader<T>(dr, instance, entityFields, procedure, properties);
		}

		private static void SetValueFieldDataReader<T>(DbDataReader dr, object instance, Dictionary<string, string> fields, string procedure, PropertyInfo[] propriedades)
		{
			foreach (PropertyInfo p in propriedades)
			{
				if (fields.ContainsKey(p.Name.ToLower()))
					setEntityPropValue(dr, p, instance, fields[p.Name.ToLower()]);
			}
		}

		private static object GetEnumFromChar(object valor, Type type)
		{
			try
			{
				sbyte v = (sbyte)char.Parse(valor.ToString());
				return Enum.Parse(type, v.ToString());
			}
			catch { }
			return null;
		}

		private static char GetCharFromEnum(object instancia)
		{
			try
			{
				object value = instancia.GetType().GetField(instancia.ToString()).GetRawConstantValue();
				return (char)sbyte.Parse(value.ToString());
			}
			catch { }
			return Char.MinValue;
		}

		private static Dictionary<string, object> GetEntityParametersDictionary<T>(T entidade, string procedure)
			where T : BaseModel<TPk>
		{
			Dictionary<string, object> result = new Dictionary<string, object>();

			var attributes = entidade.GetType().GetTypeInfo().GetCustomAttributes<MapperFieldClassAttribute>(false);
			foreach (var attribute in attributes)
			{
				if (attribute.IsValid(procedure))
				{
					result.Add(attribute.Field, attribute.Value);
				}
			}

			foreach (PropertyInfo p in entidade.GetType().GetProperties())
			{
				var atributosProp = p.GetCustomAttributes<MapperFieldAttribute>(false);

				foreach (var attribute in atributosProp)
				{
					if (attribute.IsValid(procedure))
					{
						object valor = p.GetValue(entidade, null);
						if (p.PropertyType.GetTypeInfo().IsEnum)
						{
							if (attribute.Type == DbType.SByte)
								valor = GetCharFromEnum(valor);
							else
								valor = Enum.Parse(p.PropertyType, valor.ToString());
						}
						result.Add(attribute.Field, valor);
					}
				}

				var relatAttributes = p.GetCustomAttributes<MapperRelationshipAttribute>(false);
				foreach (var attribute in relatAttributes)
				{
					if (attribute.IsValid(procedure))
					{
						result.Add(attribute.ForeignKey, p.GetValue(entidade, null).GetPropValue("PK"));
					}
				}
			}

			var tableAttribute = entidade.GetType().GetTypeInfo().GetCustomAttributes<MapperTableAttribute>(false);
			foreach (var attribute in tableAttribute)
			{
				if (!attribute.PKIdentity)
				{
					result.Add(attribute.PK, entidade.PK);
					break;
				}
			}

			return result;
		}
		#endregion

		#endregion

		#endregion
	}
}
