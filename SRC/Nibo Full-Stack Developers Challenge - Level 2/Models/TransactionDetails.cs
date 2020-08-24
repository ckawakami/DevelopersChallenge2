using System;
using System.Data.Entity;

namespace Nibo_Full_Stack_Developers_Challenge___Level_2.Models
{
    public class TransactionDetails
    {
        public int Id { get; set; }
        public EnumType TransactionType { get; set; }
        public DateTime DatePosted { get; set; }
        public decimal TransactionAmmount { get; set; }
        public string Memo { get; set; }
        public int FileDataId { get; set; }
        public FileData FileData { get; set; }
        public string OfxFileName { get; set; }
    }
    public enum EnumType
    {      
        Debit, 
        Credit 
    }
}