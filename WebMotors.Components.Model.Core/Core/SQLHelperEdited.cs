using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace WebMotors.Components.Model.Core
{
	public class SQLHelperEdited
	{
		#region [ NON QUERY ]
		public static int ExecuteNonQuery(Database database, string pNomeProcedure, ArrayList pParameters, CommandType commandType = CommandType.StoredProcedure)
		{
			using (DbCommand oSqlCommand = SQLHelperEdited.CreateCommand(database, pNomeProcedure, pParameters, commandType))
			{
				return oSqlCommand.ExecuteNonQuery();
			}
		}
		#endregion

		#region [ SCALAR ]
		public static object ExecuteScalar(Database database, string pNomeProcedure, ArrayList pParameters, CommandType pCommandType = CommandType.StoredProcedure)
		{
			using (DbCommand oSqlCommand = SQLHelperEdited.CreateCommand(database, pNomeProcedure, pParameters, pCommandType))
			{
				return oSqlCommand.ExecuteScalar();
			}
		}
		#endregion

		#region [ READER ]
		public static DbDataReader ExecuteReader(Database database, string pNameProcedure, ArrayList pParameters, CommandType pCommandType = CommandType.StoredProcedure)
		{
			using (DbCommand oSqlCommand = SQLHelperEdited.CreateCommand(database, pNameProcedure, pParameters, pCommandType))
			{
				return oSqlCommand.ExecuteReader();
			}
		}
		#endregion

		#region [ CREATE COMMAND ]
		private static DbCommand CreateCommand(Database database, string pCommandText, ArrayList pParameters, CommandType commandType)
		{
			DbCommand oSqlCommand = database.CreateCommand();
			oSqlCommand.Connection = database.Connection;
			if (database.Transaction != null) oSqlCommand.Transaction = database.Transaction;
			oSqlCommand.CommandText = pCommandText;
			oSqlCommand.CommandType = commandType;
			foreach (DbParameter oSqlParameter in pParameters)
				oSqlCommand.Parameters.Add(CheckParameter(oSqlParameter));
			return oSqlCommand;
		}
		#endregion

		#region [ PARAMETROS ]
		public static ArrayList AddParameter(Database database, string vParameter, object oValor)
		{
			ArrayList parameters = new ArrayList();
			AddParameter(database, ref parameters, vParameter, oValor);
			return parameters;
		}

		public static ArrayList AddParameter(Database database, string vParameter, object oValor, DbType oDbType)
		{
			ArrayList parameters = new ArrayList();
			AddParameter(database, ref parameters, vParameter, oValor, oDbType);
			return parameters;
		}

		public static void AddParameter(Database database, ref ArrayList oArrayList, string vParameter, object value, DbType oDbType)
		{
			switch (oDbType)
			{
				case DbType.Date:
				case DbType.DateTime:
				case DbType.DateTime2:
				case DbType.DateTimeOffset:
					DateTime oDateTime;
					if (value != null && DateTime.TryParse(value.ToString(), out oDateTime))
						AddParameter(database, ref oArrayList, vParameter, oDateTime);
					else
						AddParameter(database, ref oArrayList, vParameter, DBNull.Value);
					break;
				default:
					AddParameter(database, ref oArrayList, vParameter, value);
					break;
			}
		}

		public static void AddParameter(Database database, ref ArrayList parameters, string vParameter, object value)
		{
			vParameter = AddArroba(vParameter);
			parameters.Add(CreateParameter(database, vParameter, value));
		}

		public static DbParameter CreateParameter(Database database, string name, object value)
		{
			return database.CreateParameter(name, value);
		}

		private static string AddArroba(string vParameter)
		{
			if (vParameter.Substring(0, 1) != "@")
				vParameter = vParameter.Insert(0, "@");
			return vParameter;
		}

		private static object TransformDate(object value)
		{
			object oObject;
			DateTime oDateTime;
			if (value != null && DateTime.TryParse(value.ToString(), out oDateTime))
				oObject = oDateTime;
			else
				oObject = value;

			return oObject;
		}

		internal static DbParameter CheckParameter(DbParameter pSqlParameter)
		{
			if (pSqlParameter.Value == null)
				pSqlParameter.Value = DBNull.Value;
			if (pSqlParameter.DbType == DbType.String && pSqlParameter.Value.ToString().Trim() == string.Empty)
				pSqlParameter.Value = DBNull.Value;
			if (pSqlParameter.DbType == DbType.DateTime && Convert.ToDateTime(pSqlParameter.Value.ToString()) == DateTime.MinValue)
				pSqlParameter.Value = DBNull.Value;
			if ((pSqlParameter.DbType == DbType.Int16 ||
					pSqlParameter.DbType == DbType.Int32 ||
					pSqlParameter.DbType == DbType.Int64 ||
					pSqlParameter.DbType == DbType.Currency ||
					pSqlParameter.DbType == DbType.Decimal ||
					pSqlParameter.DbType == DbType.Double ||
					pSqlParameter.DbType == DbType.Single ||
					pSqlParameter.DbType == DbType.UInt16 ||
					pSqlParameter.DbType == DbType.UInt32 ||
					pSqlParameter.DbType == DbType.UInt64 ||
					pSqlParameter.DbType == DbType.VarNumeric ||
					pSqlParameter.DbType == DbType.Byte
				) && pSqlParameter.Value.ToString() == "-1")
				pSqlParameter.Value = DBNull.Value;
			return pSqlParameter;
		}
		#endregion
	}
}
