using PlataformaPDCOnline.Editable;
using OdbcDatabase.database;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;
using System.Threading;
using PlataformaPDCOnline.Internals.pdcOnline.Sender;

namespace PlataformaPDCOnline.Internals.plataforma
{
    class DetectorOfChangs
    {
        /// <summary>
        /// Metodo que extrae un command de la base de datos y busca por cada tabla que cambios son de este command
        /// </summary>
        /// <param name="commandsTable">Recibe una lista de diccionarios (string, object) donde string es la columna y el object es el contenido de la fila  en la base de datos</param>
        public static void prepareDetector(List<Dictionary<string, object>> commandsTable)
        {
            //Most(commandsTable);

            /*Queue<Dictionary<WebCommandsController, Command>> commands = new Queue< Dictionary < WebCommandsController, Command>> (); //creamos la cola compartida de commands
            Semaphore mutexCommandsSemaforo = new Semaphore(1, 1); //creamos un semaforo para la zona de exclusion mutua
            Semaphore stopSemafore = new Semaphore(0, int.MaxValue); //creamos un semaforo para hacer que espere el thread


            ThreadSender sender = new ThreadSender(commands, mutexCommandsSemaforo, stopSemafore); //instanciamos la clase thread

            Thread threadSender = new Thread(sender.RunThread); //creamos el thread
            threadSender.Start(); //iniciamos el thread*/

            foreach (Dictionary<string, object> row in commandsTable)
            {
                WebCommandsController controller = new WebCommandsController(row/*, stopSemafore, mutexCommandsSemaforo, commands*/); //generamos un webController a partir de la informacion de este controller

                /*Console.WriteLine(controller);

                Console.WriteLine("introduce un intro");
                Console.ReadLine();*/

                controller.RunDetector(); //lanzamos el controller
            }

            Console.WriteLine("end runs Controllers");

            /*sender.SetEndJob(); //indicamos a la clase del thread que ja puede morir

            threadSender.Join(); //esperamos a que el thread muera, por algun motivo no espera*/
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

        //envia un command mediante el ICommandSender
        /*private async void sendCommand(List<Command> commands)
        {
            foreach(Command command in commands)
            {
                if(command != null) await this.sender.SendAsync(command);
            }
        }*/
    }
}
