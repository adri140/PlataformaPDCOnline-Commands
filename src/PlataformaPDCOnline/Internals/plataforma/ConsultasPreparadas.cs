using System;
using System.Collections.Generic;
using System.Data.Odbc;
using OdbcDatabase.database;
using Pdc.Messaging;

namespace PlataformaPDCOnline.Internals.plataforma
{
    /// <summary>
    /// La clase ConsultasPreparadas se ocupa de recuperar los datos de la base de datos informix, los metodos tienen la sigiente estructura: RecoberDatos'nombre de la tabla'.
    /// </summary>
    class ConsultasPreparadas
    {
        private static InformixOdbcDao infx;

        public static List<Dictionary<string, object>> getCommands()
        { 
            if (infx == null) infx = new InformixOdbcDao("webcommands");
            else infx.TableName = "webcommands";

            string sql = "SELECT commandname, commandparameters, tablename, uidtablename, sqlcommand FROM webcommands WHERE active = ? ORDER BY ordercommand ASC";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("active", 1);

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add("active", OdbcType.Int);

            return infx.ExecuteSelect(sql, parameters, types);
        }

        public static List<Dictionary<string, object>> getRowData(string sql)
        {
            return infx.ExecuteSelect(sql, null, null);
        }

        public static int UpdateTableForGUID(WebCommandsController controller, Dictionary<string, object> row, string uid, string campoCodeId)
        {
            string sql = "UPDATE " + controller.TableName + " SET " + controller.UidTableName + " = ? WHERE " + campoCodeId + " = ?;";

            Console.WriteLine("Sentencia: " + sql);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(controller.UidTableName, uid);
            parameters.Add(campoCodeId, row.GetValueOrDefault(campoCodeId).ToString());

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add(controller.UidTableName, OdbcType.VarChar);
            types.Add(campoCodeId, OdbcType.VarChar);

            return infx.ExecuteUpdate(sql, parameters, types);
        }

        public static int UpdateSumEventCommit(WebCommandsController controller, Command command, OdbcTransaction tran) //PROVAR CON EL THREAD
        {
            string sql = "SELECT eventcommit FROM " + controller.TableName + " WHERE " + controller.UidTableName + " = ?;";
            
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(controller.UidTableName, command.AggregateId);

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add(controller.UidTableName, OdbcType.VarChar);

            List<Dictionary<string, object>> returned = infx.ExecuteSelect(sql, parameters, types, tran);

            if (returned.Count == 1)
            {
                int eventcommit = 0;
                foreach (Dictionary<string, object> dic in returned)
                {
                    eventcommit = (int)dic.GetValueOrDefault("changevalue");
                }

                sql = "UPDATE " + controller.TableName + " SET eventcommit = " + (eventcommit - 1) + " changevalue =  WHERE " + controller.UidTableName + " = ?;";

                return infx.ExecuteUpdate(sql, parameters, types, tran);
            }

            return 0;
        }

        public static int UpdateRestChangeValue(WebCommandsController controller, Command command, OdbcTransaction tran)
        {
            string sql = "SELECT changevalue FROM " + controller.TableName + " WHERE " + controller.UidTableName + " = ?";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(controller.UidTableName, command.AggregateId);

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add(controller.UidTableName, OdbcType.VarChar);

            List<Dictionary<string, object>> returned = infx.ExecuteSelect(sql, parameters, types, tran);

            if(returned.Count == 1)
            {
                int changevalue = 0;
                foreach(Dictionary<string, object> dic in returned)
                {
                    changevalue = (int) dic.GetValueOrDefault("changevalue");
                }
                
                sql = "UPDATE " + controller.TableName + " SET changevalue = " + (changevalue - 1) + " changevalue =  WHERE " + controller.UidTableName + " = ?;";
                
                return infx.ExecuteUpdate(sql, parameters, types, tran);
            }

            return 0;
        }


    }
}
