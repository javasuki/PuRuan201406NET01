using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;


namespace Mini.Data
{
    public static class DbFactory
    {
        public static T DbScalar<T>(string sql, Action<Exception> dbAct = null)
        {
            var o = DbSubmit(sql, DbOper.SelectScalar, null, "", dbAct);
            if (o == null) return default(T);
            if (o == DBNull.Value) return default(T);
            return (T)Convert.ChangeType(o, typeof(T));
        }

        public static DataTable DbSelect(string sql, Action<Exception> dbAct = null)
        {
            var o = DbSubmit(sql, DbOper.Select, null, "", dbAct);
            return (DataTable)o;
        }

        public static void DbInsert(string sql, List<object> lstParamValues, Action<Exception> dbAct = null)
        {
            DbSubmit(sql, DbOper.Insert, lstParamValues, "", dbAct);
        }

        public static int DbInsert(string sql, List<object> lstParamValues, string autoFieldName, Action<Exception> dbAct = null)
        {
            if (string.IsNullOrEmpty(autoFieldName))
                throw new ArgumentNullException("autoFieldName");

            object o = DbSubmit(sql, DbOper.Insert, lstParamValues, autoFieldName, dbAct);
            return Convert.ToInt32(o);
        }

        public static void DbUpdate(string sql, List<object> lstParamValues, Action<Exception> dbAct = null)
        {
            DbSubmit(sql, DbOper.Update, lstParamValues, "", dbAct);
        }

        public static void DbDelete(string sql, List<object> lstParamValues = null, Action<Exception> dbAct = null)
        {
            DbSubmit(sql, DbOper.Delete, lstParamValues ?? new List<object>(), "", dbAct);
        }

        

        static bool IsCheckProviderInvariantName = false;
        static object DbSubmit(string sql, DbOper oper, List<object> lstParamValues, string autoFieldName, Action<Exception> act = null)
        {
            string prvName = DbConfig.Get().ProviderInvariantName;
            #region check ProviderInvariantName
            if (!IsCheckProviderInvariantName)
            {
                IsCheckProviderInvariantName = true;
                var cdt = DbProviderFactories.GetFactoryClasses();
                var rs = from r in cdt.AsEnumerable() where r.Field<string>("InvariantName") == prvName select r;
                if (rs.Count() == 0)
                    throw new Exception(prvName + " not found!");
            }
            #endregion

            object oResult = null;
            if(oper == DbOper.Select)
                oResult = new DataTable();
            var factory = DbProviderFactories.GetFactory(prvName);
            using (var conn = factory.CreateConnection())
            {
                #region connection db
                conn.ConnectionString = DbConfig.Get().ConnectionString;
                try
                {
                    conn.Open();
                }
                catch(Exception ex) 
                {
                    if (act != null)
                        act.Invoke(ex);
                    return oResult;
                }
                #endregion

                #region execute command
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    try
                    {
                        if (oper == DbOper.Select)
                        {
                            #region select
                            using (var adp = factory.CreateDataAdapter())
                            {
                                adp.SelectCommand = cmd;
                                adp.Fill((DataTable)oResult);
                            }
                            #endregion
                        }
                        else if (oper == DbOper.SelectScalar)
                        {
                            oResult = cmd.ExecuteScalar();
                        }
                        else if (oper == DbOper.Insert || oper == DbOper.Update || oper == DbOper.Delete)
                        {
                            #region insert/update/delete
                            foreach (object p in lstParamValues)
                            {
                                var piName = p.GetType().GetProperty("Name");
                                var piValue = p.GetType().GetProperty("Value");

                                var prm = cmd.CreateParameter();
                                prm.ParameterName = piName.GetValue(p, null).ToString();
                                prm.Value = piValue.GetValue(p, null);
                                cmd.Parameters.Add(prm);
                            }


                            if (string.IsNullOrEmpty(autoFieldName))
                                cmd.ExecuteNonQuery(); //update,delete时, autoFieldName永为空
                            else
                            {
                                //cmd.CommandText = "SELECT @@Identity";
                                string tblName = GetTableName(sql);
                                cmd.CommandText += ";select IDENT_CURRENT('" + tblName + "')";
                                oResult = cmd.ExecuteScalar();
                            }
                            #endregion
                        }
                        else if (oper == DbOper.SP)
                        {
                            #region store
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        if (act != null)
                            act.Invoke(ex);
                        return oResult;
                    }
                }
                #endregion
            }

            return oResult;
        }

        static string GetTableName(string sql)
        {
            var s1 = sql.ToLower().Replace("insert into ", "");
            int idx = sql.Length - s1.Length;
            return sql.Substring(idx).Split("(".ToCharArray(),StringSplitOptions.RemoveEmptyEntries)[0];
        }

        enum DbOper
        {
            SelectScalar,
            Select,
            Insert,
            Update,
            Delete,
            SP
        }
    }
}
