using Pdc.Messaging;
using System;
using System.Collections.Generic;
using PlataformaPDCOnline.Internals.plataforma;
using PlataformaPDCOnline.Editable.pdcOnline.Commands;

namespace PlataformaPDCOnline.Editable.Searchers
{
    public class SearchCreateWebUser : ISearcher
    {
        public Queue<Command> RunSearcher(Dictionary<string, object> row, WebCommandsController controller) //obligatorio tanto por la interface como que es el metodo que se ejecutara para buscar y crear el command
        {
            Queue<Command> commands = new Queue<Command>();

            if (row.GetValueOrDefault("userid").ToString().Equals(""))
            {
                string uid = Guid.NewGuid().ToString(); //generamos el guid del usuario

                if(ConsultasPreparadas.UpdateTableForGUID(controller, row, uid, "usercode") == 1)
                {
                    CreateWebUser command = new CreateWebUser(uid) { username = row.GetValueOrDefault("username").ToString(), usercode = row.GetValueOrDefault("usercode").ToString() };
                    commands.Enqueue(command);
                }              
            }

            return commands; //devuelvo el command
        }
    }

    public class SearchUpdateWebUser : ISearcher
    {
        public Queue<Command> RunSearcher(Dictionary<string, object> row, WebCommandsController controller)
        {
            Queue<Command> commands = new Queue<Command>();
            Console.WriteLine("Running searcher Update");

            /*
             * Passos a seguir:
             * buscamos en la otra base de datos el id de nuestra row, si no existe, creamos y enviamos un command de createWebUser.
             * si existe, comparamos parametro a parametro que cambio hay, una vez encontrado, lo añadimos a la cola de commands a enviar
             */ 
            return commands;
        }
    }

    public class SearchDeleteWebUser : ISearcher
    {
        public Queue<Command> RunSearcher(Dictionary<string, object> row, WebCommandsController controller)
        {
            Queue<Command> commands = new Queue<Command>();

            //lo sullo seria comprovar que en la otra base de datos no se ha eliminado, si se ha eliminado se quitaria el flag de changevalue a -1
            if (row.GetValueOrDefault("userid").ToString() != "")
            {
                commands = new Queue<Command>();
                DeleteWebUser command = new DeleteWebUser(row.GetValueOrDefault("userid").ToString());
                commands.Enqueue(command);
            }

            return commands;
        }
    }
}
