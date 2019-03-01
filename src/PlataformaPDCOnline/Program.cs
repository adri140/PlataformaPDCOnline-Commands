using System;
using System.Collections.Generic;
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
        private static void StartFunction()
        {
            List<Dictionary<string, object>> webCommandsTable = ConsultasPreparadas.getCommands();

            if (webCommandsTable.Count > 0) DetectorOfChangs.prepareDetector(webCommandsTable); //si hay commands con los que trabajar, trabajamos
        }
    }
}
