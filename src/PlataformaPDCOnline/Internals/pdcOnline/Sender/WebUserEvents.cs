﻿using Pdc.Messaging;
using PlataformaPDCOnline.Editable.pdcOnline.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Internals.pdcOnline.Sender
{
    //clases temporales
    public class WebUserCreated : Event
    {
        public WebUserCreated(string aggregateId, string username, string usercode, CreateWebUser previous)
            : base(typeof(WebUser).Name, aggregateId, 0, previous)
        {
            Id = aggregateId;
            this.username = username;
            this.usercode = usercode;
        }

        public string Id { get; set; }
        public string username { set; get; }
        public string usercode { set; get; }
    }

    public class WebUserUpdated : Event
    {
        public WebUserUpdated(string aggregateId, string username, UpdateWebUser previous)
            : base(typeof(WebUser).Name, aggregateId, 0, previous)
        {
            Id = aggregateId;
            this.username = username;
        }

        public string Id { get; set; }
        public string username { set; get; }
    }

    public class WebUserDeleted : Event
    {
        public WebUserDeleted(string aggregateId, DeleteWebUser previous)
            : base(typeof(WebUser).Name, aggregateId, 0, previous)
        {
            Id = aggregateId;
        }

        public string Id { get; set; }
    }
}
