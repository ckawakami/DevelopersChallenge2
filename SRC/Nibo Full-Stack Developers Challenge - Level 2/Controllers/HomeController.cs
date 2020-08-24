using Nibo_Full_Stack_Developers_Challenge___Level_2.DAL;
using Nibo_Full_Stack_Developers_Challenge___Level_2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nibo_Full_Stack_Developers_Challenge___Level_2.Controllers
{
    public class HomeController : Controller
    {
        private ReportsContext db = new ReportsContext();
        private static FileData fileData = new FileData();

        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Get uploaded files and convert them to class objects.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StreamFiles(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    if (fileExtension != ".ofx")
                    {
                        ViewBag.ResponseMessage = "ERRO: Somente arquivos OFX são aceitos.";
                        return View("Index");
                    }

                    fileData = ConvertOTXFileToFileData(file);
                    if (fileData == null)
                    {
                        fileData = new FileData();
                        return View("Index");
                    }
                }
            }            
            return View("Index", fileData);
        }
        /// <summary>
        /// Save record to database
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFiles()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.FileData.Add(fileData);
                    db.SaveChanges();
                    fileData = new FileData();
                    ViewBag.ResponseMessage = "Registros salvos com sucesso!";
                    return View("Index");
                }
                else
                {
                    ViewBag.ResponseMessage = "Ocorreu um erro na sua aplicação.";
                    return View("Index");
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ViewBag.ResponseMessage = "Ocorreu um erro na sua aplicação.";
                return View("Index");
            }
        }
        /// <summary>
        /// Clears model and view table
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearData()
        {
            fileData = new FileData();
            return View("Index", fileData);
        }
        /// <summary>
        /// Check if transaction is already saved in databe or present in multiple uploaded files
        /// </summary>
        /// <param name="transactionDetails"></param>
        /// <param name="transactionDetailsList"></param>
        /// <param name="dateRange"></param>
        /// <returns>true if transaction is unique, false if it´s already saved or duplicated between files.</returns>
        public bool UniqueTransaction(TransactionDetails transactionDetails, List<TransactionDetails> transactionDetailsList, bool dateRange)
        {            
            if (dateRange)
            {
                var details = db.TransactionDetails;
                foreach (var trans in details)
                {
                    if (trans.Memo == transactionDetails.Memo && trans.DatePosted == transactionDetails.DatePosted && trans.TransactionAmmount == transactionDetails.TransactionAmmount)
                        return false;
                }
            }            
            foreach (var trans in transactionDetailsList)
            {                
                if (trans.Memo == transactionDetails.Memo && trans.DatePosted == transactionDetails.DatePosted && trans.TransactionAmmount == transactionDetails.TransactionAmmount && trans.OfxFileName != transactionDetails.OfxFileName)
                    return false;
            }
            
            return true;
        }
        /// <summary>
        /// Convert and validates otx file data
        /// </summary>
        /// <param name="file"></param>
        /// <returns>FileData object</returns>
        public FileData ConvertOTXFileToFileData(HttpPostedFileBase file)
        {

            TransactionDetails transactionDetails = new TransactionDetails();
            List<TransactionDetails> transactionDetailsList = fileData.TransactionDetails == null ? new List<TransactionDetails>() : fileData.TransactionDetails;
            bool insideDateRange = false;
            string message;
               
            using (StreamReader content = new StreamReader(file.InputStream))
            {
                while ((message = content.ReadLine()) != null)
                {                    
                    if (message.Contains("<BANKID>"))
                    {
                        int bankId = Convert.ToInt32(message.Remove(0, 8));
                        if (fileData.BankId != bankId)
                            fileData.BankId = bankId;
                    }
                    else if (message.Contains("<ACCTID>"))
                    {
                        long accountNumber = Convert.ToInt64(message.Remove(0, 8));
                        if (fileData.AccountNumber != accountNumber)
                            fileData.AccountNumber = accountNumber;
                    }
                    else if (message.Contains("<ACCTTYPE>"))
                    {
                        fileData.AccountType = message.Remove(0, 10);
                    }
                    else if (message.Contains("<DTSTART>"))
                    {
                        string dateToConvert = message.Remove(0, 9);
                        DateTime? convertedDate = ConvertDataStringToDateTime(dateToConvert);
                        if (convertedDate.HasValue)
                        {
                            if (fileData.DateStart != DateTime.MinValue)
                            {
                                if (fileData.DateStart > convertedDate.Value)
                                    fileData.DateStart = convertedDate.Value;
                            }
                            else
                                fileData.DateStart = convertedDate.Value;
                        }
                        else
                        {
                            ViewBag.ResponseMessage = "ERRO: Data inicial do período inválida.";
                            return null;
                        }
                    }
                    else if (message.Contains("<DTEND>"))
                    {
                        string dateToConvert = message.Remove(0, 7);
                        DateTime? convertedDate = ConvertDataStringToDateTime(dateToConvert);
                        if (convertedDate.HasValue)
                        {
                            if (fileData.DateEnd != DateTime.MinValue)
                            {
                                if (fileData.DateEnd < convertedDate.Value)
                                    fileData.DateEnd = convertedDate.Value;
                            }
                            else
                                fileData.DateEnd = convertedDate.Value;                            
                            insideDateRange = CheckIfTransactionIsAlreadySaved();
                        }
                        else
                        {
                            ViewBag.ResponseMessage = "ERRO: Data final do período inválida.";                           
                            return null;
                        }
                    }
                    else if (message.Contains("<TRNTYPE>"))//New Transaction
                    {
                        transactionDetails.TransactionType = message.Remove(0, 9) == "DEBIT" ? EnumType.Debit : EnumType.Credit;
                    }
                    else if (message.Contains("<DTPOSTED>"))
                    {
                        string dateToConvert = message.Remove(0, 10);
                        DateTime? convertedDate = ConvertDataStringToDateTime(dateToConvert);
                        if (convertedDate.HasValue)
                            transactionDetails.DatePosted = convertedDate.Value;
                        else
                        {
                            ViewBag.ResponseMessage = "ERRO: Data da operação inválida.";
                            return null;
                        }
                    }
                    else if (message.Contains("<TRNAMT>"))
                    {
                        transactionDetails.TransactionAmmount = decimal.Parse(message.Remove(0, 8), CultureInfo.InvariantCulture);
                    }
                    else if (message.Contains("<MEMO>"))
                    {
                        transactionDetails.Memo = message.Remove(0, 6);
                    }                    
                    else if (message.Contains("</STMTTRN>")) //closing transaction, renew transactionDetails object to receive next transactions
                    {
                        transactionDetails.OfxFileName = file.FileName;
                        if (UniqueTransaction(transactionDetails, transactionDetailsList, insideDateRange))
                        { 
                            transactionDetailsList.Add(transactionDetails);
                            fileData.Total += transactionDetails.TransactionAmmount;
                        }                        
                        transactionDetails = new TransactionDetails();  
                    }
                }
            }           
            fileData.TransactionDetails = transactionDetailsList.OrderBy(order => order.DatePosted).ToList();                    
            return fileData;
        }
        /// <summary>
        /// Convert date format from otx file do datetime
        /// </summary>
        /// <param name="dateToConvert"></param>
        /// <returns>DateTime if dateToConvert is properly formatted, null if it is not/returns>
        private static DateTime? ConvertDataStringToDateTime(string dateToConvert)
        {
            DateTime? convertedDate = null;
            try
            {
                string removeEst = dateToConvert.Remove(14, 9);

                string formatedDate = String.Format("{0}-{1}-{2} {3}:{4}:{5}",
                removeEst.Substring(0, 4), removeEst.Substring(4, 2), removeEst.Substring(6, 2), removeEst.Substring(8, 2), removeEst.Substring(10, 2), removeEst.Substring(12, 2));

                convertedDate = DateTime.ParseExact(formatedDate, "yyyy-MM-dd HH:mm:ss",
                              CultureInfo.InvariantCulture);
            }
            catch { }
            return convertedDate;
        }
        /// <summary>
        /// Checks if transaction is already saved in DB
        /// </summary>
        /// <returns>true if already saved, false if not</returns>
        public bool CheckIfTransactionIsAlreadySaved()
        {
            var fileDataSaved = db.FileData;

            if (fileDataSaved != null)
            {
                foreach (var fileSadev in fileDataSaved)
                {
                    if (fileData.BankId == fileSadev.BankId && fileData.AccountNumber == fileSadev.AccountNumber && fileData.AccountType == fileSadev.AccountType)
                    {                        
                        if (fileData.DateEnd >= fileSadev.DateStart && fileSadev.DateEnd >= fileData.DateStart)
                        {
                            return true;
                        }

                        if (fileData.DateStart <= fileSadev.DateEnd && fileSadev.DateStart <= fileData.DateStart)
                        {
                            return true;
                        }                        
                    }
                }
            }
            return false;
        }
    }
}