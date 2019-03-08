using OdbcDatabase.excepciones;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlataformaPDCOnline.Internals.plataforma
{
    public class WebCommandsController
    {
        public readonly string CommandName;
        public readonly List<string> CommandParameters;
        public readonly string TableName;
        public readonly string UidTableName;
        public readonly string SqlCommand;

        /// <summary>
        /// A partir de una fila de la tabla commands, genero este controller, mediante reflexion.
        /// </summary>
        /// <param name="controller">Diccionario (string, object) donde string es la columna y object es el dato en la base de datos</param>
        public WebCommandsController(Dictionary<string, object> controller)
        {
            this.TableName = controller.GetValueOrDefault("tablename").ToString();
            this.CommandName = controller.GetValueOrDefault("commandname").ToString();
            //Console.WriteLine("nombre: " + this.CommandName +  " indice de create: " + this.CommandName.IndexOf("Create"));
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
            return result + "], SqlCommand: " + this.SqlCommand;
        }

        //ejecuta el search para cada fila recuperada de la base de datos, antes de esto, debemos encontrar el search correspondiente para el command que toca, para eso usamos reflexion
        public void RunDetector()
        {
            List<Dictionary<string, object>> table = ConsultasPreparadas.Singelton().GetRowData(this.SqlCommand);

            Type[] types = Assembly.GetExecutingAssembly().GetTypes(); //recuperamos todos los tipos

            //por cada typo 'clase'
            foreach (Type t in types)
            {
                if (t.Name == "Search" + this.CommandName) //si el nombre de la clase es igual a 'Search' + 'el nombre del controlador'
                {
                    object search = Activator.CreateInstance(t); //creamos una instancia de esta clase

                    if (search is ISearcher) //si la instancia implementa ISearcher y SearcherChangesController
                    {
                        MethodInfo method = search.GetType().GetMethod("RunSearcher");

                        foreach (Dictionary<string, object> row in table)
                        {
                            Command commands = (Command)method.Invoke(search, new object[] { row, this }); //invocamos el methodo con la instancia searcher y le pasamos los parametros
                            ConsultasPreparadas.Singelton().SendCommands(commands); //nos devuelve los commands, los cuales enviaremos
                        }
                        break;
                    }
                    else throw new MyNoImplementedException("Se ha encontrado la clase " + t.Name + ", pero no implementa ISearcher."); //ok
                }
            }
        }
    }
}
