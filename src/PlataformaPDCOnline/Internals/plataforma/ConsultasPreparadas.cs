using System;
using System.Collections.Generic;
using System.Data.Odbc;
using OdbcDatabase;
using OdbcDatabase.database;
using OdbcDatabase.excepciones;
using Pdc.Messaging;
using PlataformaPDCOnline.Internals.pdcOnline.Sender;

namespace PlataformaPDCOnline.Internals.plataforma
{
    /// <summary>
    /// La clase ConsultasPreparadas se ocupa de recuperar los datos de la base de datos informix, los metodos tienen la sigiente estructura: RecoberDatos'nombre de la tabla'.
    /// </summary>
    class ConsultasPreparadas
    {
        private InformixOdbcDao infx;

        private static ConsultasPreparadas consultas;

        private ConsultasPreparadas()
        {
            infx = new InformixOdbcDao();
        }

        public static ConsultasPreparadas Singelton()
        {
            if (consultas == null) consultas = new ConsultasPreparadas();
            return consultas;
        }

        //te devuelve los datos de la tabla webcommands, imprescindible para trabajar!!
        public List<Dictionary<string, object>> getCommands()
        {
            string sql = "SELECT commandname, commandparameters, tablename, uidtablename, sqlcommand FROM webcommands WHERE active = ? ORDER BY ordercommand ASC";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("active", 1);

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add("active", OdbcType.Int);

            OdbcCommand commandOdbc = new OdbcCommand(sql, infx.Database.Connection);

            DatabaseTools.InsertParameters(parameters, types, commandOdbc);
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                //Console.WriteLine("abre conexion " + sql);
                infx.Database.Connection.Open();
                result = this.executeCommadForSelect(commandOdbc);
                //Console.WriteLine("cierra conexion " + sql);
                infx.Database.Connection.Close();
            }
            catch (MyOdbcException e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (infx.Database.Connection.State == System.Data.ConnectionState.Open) infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }
            return result;
        }

        //devuelve los datos de la consulta sql de la tabla webcommands
        public List<Dictionary<string, object>> getRowData(string sql)
        {
            OdbcCommand commandOdbc = new OdbcCommand(sql, infx.Database.Connection);

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                //Console.WriteLine("abre conexion " + sql);
                infx.Database.Connection.Open();
                result = this.executeCommadForSelect(commandOdbc);
                //Console.WriteLine("cierra conexion " + sql);
                infx.Database.Connection.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (infx.Database.Connection.State == System.Data.ConnectionState.Open) infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }
            return result;
        }

        //ejecuta un command de ODBCCommand, que sea un select y te devuelve una lista de diccionarios
        public List<Dictionary<string, object>> executeCommadForSelect(OdbcCommand command)
        {
            List<Dictionary<string, object>> tablaResult = new List<Dictionary<string, object>>();
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
                    tablaResult.Add(rowResult);
                }
                
            }
            catch (MyOdbcException e)
            {
                throw new MyOdbcException("Error consultas preparadas" + e.ToString());
            }
            return tablaResult;
        }

        //actualiza el GUID de una fila en una tabla, por ahora solo comprovado con createWebUser
        public int UpdateTableForGUID(WebCommandsController controller, Dictionary<string, object> row, string uid, string campoCodeId)
        {
            string sql = "UPDATE " + controller.TableName + " SET " + controller.UidTableName + " = ? WHERE " + campoCodeId + " = ?;";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(controller.UidTableName, uid);
            parameters.Add(campoCodeId, row.GetValueOrDefault(campoCodeId).ToString());

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add(controller.UidTableName, OdbcType.VarChar);
            types.Add(campoCodeId, OdbcType.VarChar);

            OdbcCommand commandOdbc = new OdbcCommand(sql, infx.Database.Connection);

            DatabaseTools.InsertParameters(parameters, types, commandOdbc);

            int updateadas = 0;
            try
            {
                //Console.WriteLine("abre conexion " + sql);
                infx.Database.Connection.Open();

                updateadas = commandOdbc.ExecuteNonQuery();

                //Console.WriteLine("cierra conexion " + sql);
                infx.Database.Connection.Close();
            }
            catch (MyOdbcException e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (infx.Database.Connection.State == System.Data.ConnectionState.Open) infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }

            return updateadas;
        }

        //Actualiza los valores de cambios y los eventos commiteados de la base de datos con una transaccion, si algo peta, no se actualizaran ninguno
        public int UpdateChangesValuesAndCommitsValues(WebCommandsController controller, Command command, OdbcTransaction tran)
        {

            string sql = "SELECT eventcommit, changevalue FROM " + controller.TableName + " WHERE " + controller.UidTableName + " = ?";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(controller.UidTableName, command.AggregateId);

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add(controller.UidTableName, OdbcType.VarChar);

            OdbcCommand selectCommand = new OdbcCommand(sql, infx.Database.Connection, tran);

            DatabaseTools.InsertParameters(parameters, types, selectCommand);

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                result = this.executeCommadForSelect(selectCommand);
            }
            catch (MyOdbcException e)
            {
                throw new MyOdbcException("error en la select de la transaccion: " + e.ToString());
            }

            if(result.Count == 1)
            {
                parameters.Clear();
               
                foreach(Dictionary<string, object> dic in result)
                {
                    parameters.Add("changevalue", ((int) (dic.GetValueOrDefault("changevalue")) - 1));
                    parameters.Add("eventcommit", ((int) (dic.GetValueOrDefault("eventcommit")) + 1));
                }


                sql = "UPDATE " + controller.TableName + " SET changevalue = ?, eventcommit = ?  WHERE " + controller.UidTableName + " = ?;";
                
                parameters.Add(controller.UidTableName, command.AggregateId);

                types.Add("changevalue", OdbcType.Int);
                types.Add("eventcommit", OdbcType.Int);

                OdbcCommand updateCommand = new OdbcCommand(sql, infx.Database.Connection, tran);

                DatabaseTools.InsertParameters(parameters, types, updateCommand);
                
                try
                {
                    return updateCommand.ExecuteNonQuery();
                }
                catch (MyOdbcException e)
                {
                    throw new MyOdbcException("Error en la update de la transaccion: " + e.ToString());
                }
            }

            return 0;
        }

        //actualiza los datos eventcommit y changevalue de la base de datos, si los ha podido actualizar envia el command.
        public async void SendCommands(WebCommandsController controller, Command commands)
        {
            if (commands != null)
            {
                InformixOdbcDatabase data = infx.Database;
                OdbcTransaction transaction = null;

                try
                {
                    //Console.WriteLine("abre conexion para transaccion");
                    data.Connection.Open();
                    transaction = data.Connection.BeginTransaction();

                    if (UpdateChangesValuesAndCommitsValues(controller, commands, transaction) == 0) throw new MyOdbcException();

                    //Console.WriteLine("commit");
                    transaction.Commit();

                    if (infx.Database.Connection.State == System.Data.ConnectionState.Open)
                    {
                        infx.Database.Connection.Close();
                        //Console.WriteLine("cierra conexion");
                    }
                    
                    await PrepareSender.Singelton().SendAsync(commands);

                   // Console.WriteLine("Command enviado");

                }
                catch (MyOdbcException e)
                {
                    transaction.Rollback();
                    if (infx.Database.Connection.State == System.Data.ConnectionState.Open)
                    {
                        infx.Database.Connection.Close();
                       // Console.WriteLine("cierra conexion");
                    }
                    ErrorDBLog.Write("Error: " + e.ToString());
                }
                catch(Exception e)
                {
                    ErrorDBLog.Write("Error: " + e.ToString());
                }
            }
        }
    }
}
