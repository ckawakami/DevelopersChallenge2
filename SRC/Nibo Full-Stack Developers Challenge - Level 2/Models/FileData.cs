using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Nibo_Full_Stack_Developers_Challenge___Level_2.Models
{
    public class FileData
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public long AccountNumber { get; set; }
        public string AccountType { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public List<TransactionDetails> TransactionDetails { get; set; }
        public decimal Total { get; set; }    
    }  
}