using Pdc.Messaging;
using PlataformaPDCOnline.Internals.plataforma;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PlataformaPDCOnline.Internals.pdcOnline.Sender
{
    class ThreadSender
    {

        private Queue<Dictionary<WebCommandsController, Command>> Commands;
        private Semaphore MutexCommandsSemaforo;
        private Semaphore StopToPrepareCommands;

        private Boolean EndJob;

        public ThreadSender(Queue<Dictionary<WebCommandsController, Command>> commands, Semaphore mutexCommandsSemaforo, Semaphore stopToPrepareCommands)
        {
            this.Commands = commands;
            this.MutexCommandsSemaforo = mutexCommandsSemaforo;
            this.StopToPrepareCommands = stopToPrepareCommands;
            EndJob = false;
        }

        public async void RunThread()
        {
            Console.WriteLine("Thread en funcionamiento");
            Boolean wait = false;
            do
            {

                wait = StopToPrepareCommands.WaitOne(TimeSpan.FromSeconds(6)); //controlamos si hay commands que enviar

                if(wait)
                {
                    try
                    {
                        MutexCommandsSemaforo.WaitOne();
                        Dictionary<WebCommandsController, Command> commandDictionary = Commands.Dequeue();
                        MutexCommandsSemaforo.Release();

                        var commandSender = PrepareSender.Singelton();

                        foreach(WebCommandsController controller in commandDictionary.Keys)
                        {
                            Command com = commandDictionary.GetValueOrDefault(controller);
                            if(com != null)
                            {
                                await PrepareSender.Singelton().SendAsync(com);
                                /*ConsultasPreparadas.UpdateRestChangeValue(controller, com); //quitamos el changevalue de la base de datos
                                ConsultasPreparadas.UpdateSumEventCommit(controller, com); //subimos en 1 el commandcommit*/
                            }
                        }
                    }
                    catch (ThreadStateException e)
                    {
                        Console.WriteLine("SenderThread Exception: " + e.Message);
                    }
                }
            } while (!EndJob || wait);
            Console.WriteLine("Thread muere");
        }

        public void SetEndJob()
        {
            this.EndJob = true;
        }

    }
}
