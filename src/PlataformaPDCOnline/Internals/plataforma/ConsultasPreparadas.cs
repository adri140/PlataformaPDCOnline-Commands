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
        private InformixOdbcDao Infx;

        private static ConsultasPreparadas Consultas;

        private ConsultasPreparadas()
        {
            Infx = new InformixOdbcDao();
        }

        public static ConsultasPreparadas Singelton()
        {
            if (Consultas == null) Consultas = new ConsultasPreparadas();
            return Consultas;
        }

        //te devuelve los datos de la tabla webcommands, imprescindible para trabajar!!
        public List<Dictionary<string, object>> GetCommands()
        {
            string sql = "SELECT commandname, commandparameters, tablename, uidtablename, sqlcommand FROM webcommands WHERE active = ? ORDER BY ordercommand ASC";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "active", 1 }
            };

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>
            {
                { "active", OdbcType.Int }
            };

            OdbcCommand commandOdbc = new OdbcCommand(sql, Infx.Database.Connection);

            DatabaseTools.InsertParameters(parameters, types, commandOdbc);
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                //Console.WriteLine("abre conexion " + sql);
                Infx.Database.Connection.Open();
                result = this.ExecuteCommadForSelect(commandOdbc);
                //Console.WriteLine("cierra conexion " + sql);
                Infx.Database.Connection.Close();
            }
            catch (MyOdbcException e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }
            return result;
        }

        //devuelve los datos de la consulta sql de la tabla webcommands
        public List<Dictionary<string, object>> GetRowData(string sql)
        {
            OdbcCommand commandOdbc = new OdbcCommand(sql, Infx.Database.Connection);

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                //Console.WriteLine("abre conexion " + sql);
                Infx.Database.Connection.Open();
                result = this.ExecuteCommadForSelect(commandOdbc);
                //Console.WriteLine("cierra conexion " + sql);
                Infx.Database.Connection.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }
            return result;
        }

        //ejecuta un command de ODBCCommand, que sea un select y te devuelve una lista de diccionarios
        private List<Dictionary<string, object>> ExecuteCommadForSelect(OdbcCommand command)
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

        /// <summary>
        /// Actualiza el GUID de una fila en una tabla, comprovado con webusers y webaccessgroup
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="row"></param>
        /// <param name="uid"></param>
        /// <param name="campoCodeId"></param>
        /// <returns>Devuelve le numero de filas actualizadas en la base de  datos</returns>
        public int UpdateTableForGUID(WebCommandsController controller, Dictionary<string, object> row, string uid, string campoCodeId)
        {
            string sql = "UPDATE " + controller.TableName + " SET " + controller.UidTableName + " = ? WHERE " + campoCodeId + " = ?;";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { controller.UidTableName, uid },
                { campoCodeId, row.GetValueOrDefault(campoCodeId).ToString() }
            };

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>
            {
                { controller.UidTableName, OdbcType.VarChar },
                { campoCodeId, OdbcType.VarChar }
            };

            OdbcCommand commandOdbc = new OdbcCommand(sql, Infx.Database.Connection);

            DatabaseTools.InsertParameters(parameters, types, commandOdbc);

            int updateadas = 0;
            try
            {
                //Console.WriteLine("abre conexion " + sql);
                Infx.Database.Connection.Open();

                updateadas = commandOdbc.ExecuteNonQuery();

                //Console.WriteLine("cierra conexion " + sql);
                Infx.Database.Connection.Close();
            }
            catch (MyOdbcException e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }

            return updateadas;
        }

        //actualiza los datos eventcommit y changevalue de la base de datos, si los ha podido actualizar envia el command.
        public async void SendCommands(Command command)
        {
            if (command != null)
            {
                try
                {
                    await Sender.Singelton().SendAsync(command);
                }
                catch (Exception e)
                {
                    ErrorDBLog.Write("Error: " + e.ToString());
                }
            }
        }
    }
}
