﻿using OdbcDatabase;
using OdbcDatabase.database;
using Pdc.Messaging;
using PlataformaPDCOnline.Editable;
using PlataformaPDCOnline.Internals.pdcOnline.Sender;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PlataformaPDCOnline.Internals.plataforma
{
    public class WebCommandsController
    {
        public readonly string CommandName;
        public readonly List<string> CommandParameters;
        public readonly string TableName;
        public readonly string UidTableName;
        public readonly string SqlCommand;

        /* private Semaphore StopSemaforo;
         private Semaphore MutexCommandsSend;
         private Queue<Dictionary<WebCommandsController, Command>> Commands;*/

        /// <summary>
        /// A partir de una fila de la tabla commands, genero este controller, mediante reflexion.
        /// </summary>
        /// <param name="controller">Diccionario (string, object) donde string es la columna y object es el dato en la base de datos</param>
        public WebCommandsController(Dictionary<string, object> controller/*, Semaphore stopSemaforo, Semaphore mutexCommandsSend, Queue<Dictionary<WebCommandsController, Command>> commands*/)
        {
            this.TableName = controller.GetValueOrDefault("tablename").ToString();
            this.CommandName = controller.GetValueOrDefault("commandname").ToString();
            this.UidTableName = controller.GetValueOrDefault("uidtablename").ToString();
            this.SqlCommand = controller.GetValueOrDefault("sqlcommand").ToString();
            this.CommandParameters = new List<string>();

            foreach (string parameter in controller.GetValueOrDefault("commandparameters").ToString().Split(","))
            {
                this.CommandParameters.Add(parameter.Trim());
            }
        }

        public override string ToString()
        {
            string result = "CommandName: " + this.CommandName + ", TableName: " + this.TableName + ", UidTableName: " + this.UidTableName + ", Parameters: [";
            if (this.CommandParameters != null)
            {
                foreach (string parameter in this.CommandParameters)
                {
                    result = result + parameter + ", ";
                }
            }
            result = result + "], SqlCommand: " + this.SqlCommand;
            return result;
        }

        public void RunDetector()
        {
            List<Dictionary<string, object>> table = ConsultasPreparadas.Singelton().getRowData(this.SqlCommand);

            //por cada fila que hay que actualizar de una tabla de la base de datos


            Type[] types = Assembly.GetExecutingAssembly().GetTypes(); //recuperamos todos los tipos

            //por cada typo 'clase'
            foreach (Type t in types)
            {
                if (t.Name == "Search" + this.CommandName) //si el nombre de la clase es igual a 'Search' + 'el nombre del controlador'
                {
                    object searchar = Activator.CreateInstance(t); //creamos una instancia de esta clase

                    if (searchar is ISearcher) //si la instancia implementa ISearcher y SearcherChangesController
                    {
                        foreach (MethodInfo method in searchar.GetType().GetMethods()) //sacamos todos sus methods y por cada uno
                        {
                            if (method.Name.Equals("RunSearcher")) //miramos si se llama 'RunSearchar', el qual comparara los datos de la base de datos informix con la base de datos ... para el command 'commandName'
                            {
                                foreach (Dictionary<string, object> row in table)
                                {
                                    Command commands = (Command)method.Invoke(searchar, new object[] { row, this }); //invocamos el methodo con la instancia searcher y le pasamos los parametros
                                    ConsultasPreparadas.Singelton().SendCommands(this, commands); //nos devuelve los commands, los cuales enviaremos
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Se ha encontrado la clase " + t.Name + ", pero no implementa ISearcher o SearcherChangesController."); //cambiar-lo
                    }
                    break;
                }
            }
        }
    }
}
