using System;
using System.Collections.Generic;
using System.Threading;
using OdbcDatabase.excepciones;
using PlataformaPDCOnline.Editable.pdcOnline.Commands;
using PlataformaPDCOnline.Internals.pdcOnline.Sender;
using PlataformaPDCOnline.Internals.plataforma;

namespace PlataformaPDCOnline
{
    class Program
    {

        public static Boolean end = false;


        public static void Main(string[] args)
        {
            StartFunction();

            while(!end) Thread.Sleep(10000);

            try
            {
                WebCommandsController.EndSender();
            }
            catch(NullReferenceException ne)
            {
                Console.WriteLine(ne.Message);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("terminado el envio de commands");
            Console.ReadLine();
        }

        //inicia el programa, cargando todos los commands que hay en la base de datos informix
        private static void StartFunction()
        {
            List<Dictionary<string, object>> webCommandsTable = ConsultasPreparadas.Singelton().GetCommands();

            if (webCommandsTable.Count > 0) PrepareDetector(webCommandsTable); //si hay commands con los que trabajar, trabajamos
        }

        /// <summary>
        /// Metodo que extrae un command de la base de datos y busca por cada tabla que cambios son de este command
        /// </summary>
        /// <param name="commandsTable">Recibe una lista de diccionarios (string, object) donde string es la columna y el object es el contenido de la fila  en la base de datos</param>
        private async static void PrepareDetector(List<Dictionary<string, object>> commandsTable)
        {
            foreach (Dictionary<string, object> row in commandsTable)
            {
                try
                {
                    WebCommandsController controller = new WebCommandsController(row); //generamos un webController a partir de la informacion de este controller
                    Console.WriteLine("Prepare Detector: preparando trabajo para: " + controller.CommandName);
                    await controller.RunDetector(); //lanzamos el controller
                }
                catch (MyNoImplementedException ni)
                {
                    Console.WriteLine(ni.Message);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            end = true;
        }
    }
}
