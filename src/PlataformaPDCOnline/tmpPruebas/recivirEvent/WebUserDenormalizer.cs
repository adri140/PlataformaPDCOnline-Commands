using Pdc.Denormalization;
using Pdc.Messaging;
using PlataformaPDCOnline.Internals.pdcOnline.Sender;
using PlataformaPDCOnline.tmpPruebas.tratarCommand;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlataformaPDCOnline.tmpPruebas.recivirEvent
{
    public class WebUserDenormalizer : PlataformaDenormalizer<WebUser>, IEventHandler<WebUserCreated>, IEventHandler<WebUserUpdated>, IEventHandler<WebUserDeleted>
    {
        //private static WebEventController WebUserCreatedController = null;
        //private static WebEventController WebUserUpdatedController = null;
        //private static WebEventController WebUserDeletedController = null;
       // public WebUserDenormalizer(PurchaseOrdersDbContext dbContext) : base(dbContext) { }

        public async Task HandleAsync(WebUserCreated message, CancellationToken cancellationToken = default)
        {
            //if (WebUserCreatedController == null) WebUserCreatedController = DenormalizersTools.GetWebEventController(message.GetType().Name);

            try
            {
                //await ConsultasPreparadas.Singelton().RunEventCommitCommit(message, WebUserCreatedController);
                Console.WriteLine("Evento - webUserCreated - Recivido");
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task HandleAsync(WebUserUpdated message, CancellationToken cancellationToken = default)
        {
            //if (WebUserUpdatedController == null) WebUserUpdatedController = DenormalizersTools.GetWebEventController(message.GetType().Name);

            try
            {
                //await ConsultasPreparadas.Singelton().RunEventCommitCommit(message, WebUserUpdatedController);
                Console.WriteLine("Evento - WebUserUpdated - Recivido");
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task HandleAsync(WebUserDeleted message, CancellationToken cancellationToken = default)
        {
            //if (WebUserDeletedController == null) WebUserDeletedController = DenormalizersTools.GetWebEventController(message.GetType().Name);

            try
            {
                //await ConsultasPreparadas.Singelton().RunEventCommitCommit(message, WebUserDeletedController);
                Console.WriteLine("Evento - WebUserDeleted - Recivido");
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
