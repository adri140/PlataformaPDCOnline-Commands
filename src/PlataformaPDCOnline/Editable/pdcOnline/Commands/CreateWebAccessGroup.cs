using Pdc.Messaging;

namespace PlataformaPDCOnline.Editable.pdcOnline.Commands
{
    public class CreateWebAccessGroup : Command
    {
        public CreateWebAccessGroup(string aggregateId) : base(aggregateId, null)
        {

        }

        public string accessgroupname { get; set; }
    }
}
