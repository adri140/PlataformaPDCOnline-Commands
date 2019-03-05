using System;
using System.Collections.Generic;
using PlataformaPDCOnline.Internals.pdcOnline.Sender;
using PlataformaPDCOnline.Internals.plataforma;

namespace PlataformaPDCOnline
{
    class Program
    {

        public static void Main(string[] args)
        {
            Boolean exit = false;
            Console.WriteLine("Plataforma PDC Online Version: b0.0.1");
            do
            {
                string command = Console.ReadLine();
                
                switch(command)
                {
                    case "start":
                        StartFunction();
                        break;
                    case "exit":
                        exit = true;
                        break;
                    default:
                        break;
                }

            } while (!exit);
        }

        //inicia el programa, cargando todos los commands que hay en la base de datos informix
        private async static void StartFunction()
        {
            List<Dictionary<string, object>> webCommandsTable = ConsultasPreparadas.Singelton().getCommands();

            if (webCommandsTable.Count > 0) prepareDetector(webCommandsTable); //si hay commands con los que trabajar, trabajamos
        }

        /// <summary>
        /// Metodo que extrae un command de la base de datos y busca por cada tabla que cambios son de este command
        /// </summary>
        /// <param name="commandsTable">Recibe una lista de diccionarios (string, object) donde string es la columna y el object es el contenido de la fila  en la base de datos</param>
        private static void prepareDetector(List<Dictionary<string, object>> commandsTable)
        {
            //Most(commandsTable);

            foreach (Dictionary<string, object> row in commandsTable)
            {
                WebCommandsController controller = new WebCommandsController(row/*, stopSemafore, mutexCommandsSemaforo, commands*/); //generamos un webController a partir de la informacion de este controller
                Console.WriteLine("preparando trabajo para: " + controller.CommandName);
                controller.RunDetector(); //lanzamos el controller
            }

            Console.WriteLine("end runs Controllers");
        }

        //temporal, muestra los datos de una lista de diccionarios
        private static void Most(List<Dictionary<string, object>> data)
        {
            if (data != null)
            {
                foreach (Dictionary<string, object> fila in data)
                {
                    Console.WriteLine();
                    foreach (string columna in fila.Keys)
                    {
                        Console.WriteLine(columna + ": " + fila.GetValueOrDefault(columna));
                    }
                }
            }
        }
    }
}
