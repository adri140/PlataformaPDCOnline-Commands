using Microsoft.EntityFrameworkCore;
using PlataformaPDCOnline.tmpPruebas.recivirEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.tmpPruebas
{
    public class PurchaseOrdersDbContext : DbContext
    {
        public PurchaseOrdersDbContext(DbContextOptions<PurchaseOrdersDbContext> options) : base(options)
        {
        }

        public DbSet<WebUser> Customers { get; set; }
    }
}
