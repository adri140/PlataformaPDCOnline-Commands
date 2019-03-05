using System;

namespace OdbcDatabase.database
{
    public class InformixOdbcDao
    {
        public InformixOdbcDatabase Database { get; private set; }

        public InformixOdbcDao()
        {
            try
            {
                if (Database == null) { Database = new InformixOdbcDatabase(DatabaseTools.GetConnectionString("[OdbcInformixServer]")); }
            }
            catch(Exception e)
            {
                ErrorDBLog.Write(e.Message);
                Database = null;
            }
        }

        /*public List<Dictionary<string, object>> ExecuteSelect(string sql, Dictionary<string, object> parameters, Dictionary<string, OdbcType> types)
        {
            List<Dictionary<string, object>> tableResult = new List<Dictionary<string, object>>();
            try
            {
                if (sql.Trim().ToUpper().IndexOf("SELECT") != 0 && !sql.Trim().ToUpper().Contains("FROM")) throw new Exception("Consulta select mal formada o incompleta, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("UPDATE")) throw new Exception("La consulta 'select' no puede incluir la palabra 'update', valor no permitido, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DELETE")) throw new Exception("La consulta 'select' no puede incluir la palabra 'delete', valor no permitido, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DROP")) throw new Exception("La consulta 'select' no puede incluir la palabra 'drop', valor no permitido, sentencia: " + sql);

                OdbcCommand command = new OdbcCommand(sql, Database.Connection);

                if (parameters != null)
                {
                    if (!sql.Trim().ToUpper().Contains("WHERE")) throw new Exception("Consulta select mal formada o incompleta, sentencia: " + sql);
                    if (types == null) throw new Exception("Se deven de proporcionar los tipos de datos Odbc para los parametros de la consulta.");
                    DatabaseTools.InsertParameters(parameters, types, command);
                }
                
                try
                {
                    Database.Connection.Open();
                    Console.WriteLine("abre conexion");

                    OdbcDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Dictionary<string, object> rowResult = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            rowResult.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        tableResult.Add(rowResult);
                    }
                }
                catch(OdbcException ex)
                {
                    ErrorDBLog.Write("Error: " + ex.Message);
                }
                finally
                {
                    Database.Connection.Close();
                    Console.WriteLine("cierra conexion");
                }
            }
            catch (Exception e)
            {
                ErrorDBLog.Write("Error: " + e.Message);
            }

            return tableResult;
        }

        public List<Dictionary<string, object>> ExecuteSelect(string sql, Dictionary<string, object> parameters, Dictionary<string, OdbcType> types, OdbcTransaction tran)
        {
            List<Dictionary<string, object>> tableResult = new List<Dictionary<string, object>>();
            try
            {
                if (sql.Trim().ToUpper().IndexOf("SELECT") != 0 && sql.Trim().ToUpper().Contains("FROM")) throw new Exception("Consulta select mal formada o incompleta, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("UPDATE")) throw new Exception("La consulta 'select' no puede incluir la palabra 'update', valor no permitido, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DELETE")) throw new Exception("La consulta 'select' no puede incluir la palabra 'delete', valor no permitido, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DROP")) throw new Exception("La consulta 'select' no puede incluir la palabra 'drop', valor no permitido, sentencia: " + sql);

                OdbcCommand command = new OdbcCommand(sql, tran.Connection, tran);

                if (parameters != null)
                {
                    if (!sql.Trim().ToUpper().Contains("WHERE")) throw new Exception("Consulta select mal formada o incompleta, sentencia: " + sql);
                    if (types == null) throw new Exception("Se deven de proporcionar los tipos de datos Odbc para los parametros de la consulta.");
                    DatabaseTools.InsertParameters(parameters, types, command);
                }

                try
                {
                    OdbcDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Dictionary<string, object> rowResult = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            rowResult.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        tableResult.Add(rowResult);
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine("cierra conexion para transaccion rollback");
                    tran.Rollback();
                    Database.Connection.Close();
                    ErrorDBLog.Write("Error: " + ex.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("cierra conexion para transaccion rollback");
                tran.Rollback();
                Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.Message);
            }

            return tableResult;
        }

        public int ExecuteUpdate(string sql, Dictionary<string, object> parameters, Dictionary<string, OdbcType> types)
        {
            int updateadas = 0;

            try
            {
                if (sql.Trim().ToUpper().IndexOf("UPDATE") != 0 && sql.Trim().ToUpper().Contains("SET")) throw new Exception("Consulta update mal formada o incompleta, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DELETE")) throw new Exception("La consulta 'update' no puede incluir la palabra 'delete', valor no permitido, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DROP")) throw new Exception("La consulta 'update' no puede incluir la palabra 'drop', valor no permitido, sentencia: " + sql);

                OdbcCommand command = new OdbcCommand(sql, Database.Connection);

                if (parameters != null)
                {
                    if (!sql.Trim().ToUpper().Contains("WHERE")) throw new Exception("Consulta select mal formada o incompleta, sentencia: " + sql);
                    if (types == null) throw new Exception("Se deven de proporcionar los tipos de datos para los parametros de la consulta.");
                    DatabaseTools.InsertParameters(parameters, types, command);
                }
                else throw new Exception("Se deven proporcionar los parametros para la update a la base de datos.");

                try
                {
                    Database.Connection.Open();
                    Console.WriteLine("abre conexion");

                    updateadas = command.ExecuteNonQuery();
                }
                catch (OdbcException ex)
                {
                    ErrorDBLog.Write("Error: " + ex.Message + ". Sentence: " + sql);
                }
                finally
                {
                    Database.Connection.Close();
                    Console.WriteLine("cierra conexion");
                }
            }
            catch(Exception e)
            {
                ErrorDBLog.Write("Error: " + e.Message + ". Sentence: " + sql);
            }

            return updateadas;
        }

        public int ExecuteUpdate(string sql, Dictionary<string, object> parameters, Dictionary<string, OdbcType> types, OdbcTransaction tran)
        {
            int updateadas = 0;

            try
            {
                if (sql.Trim().ToUpper().IndexOf("UPDATE") != 0 && sql.Trim().ToUpper().Contains("SET")) throw new Exception("Consulta update mal formada o incompleta, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DELETE")) throw new Exception("La consulta 'update' no puede incluir la palabra 'delete', valor no permitido, sentencia: " + sql);
                if (sql.Trim().ToUpper().Contains("DROP")) throw new Exception("La consulta 'update' no puede incluir la palabra 'drop', valor no permitido, sentencia: " + sql);

                OdbcCommand command = new OdbcCommand(sql, tran.Connection, tran);

                if (parameters != null)
                {
                    if (!sql.Trim().ToUpper().Contains("WHERE")) throw new Exception("Consulta select mal formada o incompleta, sentencia: " + sql);
                    if (types == null) throw new Exception("Se deven de proporcionar los tipos de datos para los parametros de la consulta.");
                    DatabaseTools.InsertParameters(parameters, types, command);
                }
                else throw new Exception("Se deven proporcionar los parametros para la update a la base de datos.");

                try
                {
                    updateadas = command.ExecuteNonQuery();
                    ErrorDBLog.Write("info: " + sql + " updateadas: " + updateadas);
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine("cierra conexion para transaccion rollback");
                    tran.Rollback();
                    Database.Connection.Close();
                    ErrorDBLog.Write("Error: " + ex.Message + ". Sentence: " + sql);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("cierra conexion para transaccion rollback");
                tran.Rollback();
                Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.Message + ". Sentence: " + sql);
            }

            return updateadas;
        }*/
    }
}
