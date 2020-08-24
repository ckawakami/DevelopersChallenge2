using Nibo_Full_Stack_Developers_Challenge___Level_2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Nibo_Full_Stack_Developers_Challenge___Level_2.DAL
{
    public class ReportsContext : DbContext
    {
        public ReportsContext() : base("ReportsContext")
        {
        }

        public DbSet<TransactionDetails> TransactionDetails { get; set; }
        public DbSet<FileData> FileData { get; set; }

    }
}