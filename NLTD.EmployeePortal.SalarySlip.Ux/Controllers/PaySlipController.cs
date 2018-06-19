using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using NLTD.EmployeePortal.SalarySlip.Client;
using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
using NLTD.EmployeePortal.SalarySlip.Utilities.Report;
using NLTD.EmployeePortal.SalarySlip.Ux.AppHelpers;

namespace NLTD.EmployeePortal.SalarySlip.Ux.Controllers
{
    public class PaySlipController : BaseController
    {
        private string _path = string.Empty;
        private string _excelFileName = string.Empty;
        private string _xmlFileName = string.Empty;

        #region Action Methods
        // GET: PaySlip
        public ActionResult Index()
        {
            try
            {
                ViewBag.ErrorMessage = new List<string>();
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SalarySlipPath"]))
                {
                    _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        ConfigurationManager.AppSettings["SalarySlipPath"]);

                    DirectoryInfo directoryInfo = new DirectoryInfo(_path);
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage.Add(e.Message);
            }

            return View();
        }

        //First Upload the XMl file.
        [HttpPost]
        public ActionResult LoadFiles(IEnumerable<HttpPostedFileBase> postedFiles, string salaryDay)
        {
            Session.Clear();
            List<string> errorList = new List<string>();
            List<PaySlipItem> paySlipList = null;
            try
            {
                int month = 0;
                int year = 0;
                salaryDay = salaryDay.Replace("'", "");
                if (!string.IsNullOrWhiteSpace(salaryDay))
                {
                    if (salaryDay.Trim().Length == 7)
                    {
                        string salaryMonth = salaryDay.Substring(0, 2);
                        string salaryYear = salaryDay.Substring(3);

                        if (!Int32.TryParse(salaryMonth, out month))
                            errorList.Add("Salary month is not valid");
                        if (!Int32.TryParse(salaryYear, out year))
                            errorList.Add("Salary year is not valid");
                    }
                    else
                    {
                        errorList.Add("The selected salary month and year is invalid.");
                    }
                }
                else
                {
                    errorList.Add("Please select the salary month and year.");
                }
                //After the Salary Date is month and year are valid. 
                //We are 
                if (errorList.Count == 0)
                {
                    foreach (HttpPostedFileBase file in postedFiles)
                    {
                        if (file != null)
                            UploadFile(file, ref errorList);
                    }

                    if (string.IsNullOrWhiteSpace(_excelFileName))
                        errorList.Add("Please Upload the Excel file.");

                    if (string.IsNullOrWhiteSpace(_xmlFileName))
                        errorList.Add("Please Upload the Xml file.");

                    if (errorList.Count == 0)
                    {
                        //Validate XML File
                        ValidateXml(_path, _xmlFileName, ref errorList);
                        if (!errorList.Contains("XmlException"))
                        {
                            paySlipList = GetPaySlipList(_excelFileName, _xmlFileName, month, year, ref errorList);
                            ValidateExcel(paySlipList, ref errorList);
                        }
                        ViewBag.ShowPDF = false; //Hides the PDF icon.
                    }
                    if (errorList.Count == 0)
                        HttpContext.Session["PaySlipList"] = paySlipList;
                }
            }
            catch (Exception e)
            {
                errorList.Add(e.Message);
            }
            finally
            {
                ViewBag.ErrorMessage = errorList;
            }
            return PartialView("DisplayAllSalarySlipPartial", paySlipList);
        }
        public ActionResult LoadAllPaySlip()
        {
            List<PaySlipItem> paySlipList = (List<PaySlipItem>)HttpContext.Session["PaySlipList"];
            ViewBag.ErrorMessage = new List<string>();
            _path = Session["Path"]?.ToString();
            if (!string.IsNullOrWhiteSpace(_path))
            {
                try
                {
                    if (paySlipList != null && paySlipList.Count > 0)
                    {
                        if (ViewBag.ErrorMessage.Count > 0)
                        {
                            Util.WriteLog(_path, String.Join(Environment.NewLine, ViewBag.ErrorMessage));
                        }
                        if (ViewBag.ErrorMessage.Count == 0)
                        {
                            //Generate PaySlip PDF.
                            foreach (var paySlip in paySlipList)
                                PayslipReport.PayslipGenerator.GeneratePayslip(paySlip, paySlip.PaySlipFilePath, false, false);
                            ViewBag.ShowPDF = true;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage.Add(
                            "No files are uploaded (or) Some problem in generating salary slip.");
                        Util.WriteLog(_path, "No file name is provided.");
                    }
                }
                catch (Exception e)
                {
                    if (string.IsNullOrWhiteSpace(_path))
                        _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                            ConfigurationManager.AppSettings["SalarySlipPath"]);

                    _path = Path.Combine(new[] { _path, "ErrorLog.txt" });
                    Util.WriteLog(_path, ex: e);
                }
            }
            else
                ViewBag.ErrorMessage.Add("Files are not uploaded to generate salary slip(s).");

            return PartialView("DisplayAllSalarySlipPartial", paySlipList);
        }
        public FileResult PreviewPaySlip(string paySlipFilePath)
        {
            byte[] bytes;
            using (FileStream fs = System.IO.File.OpenRead(paySlipFilePath))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            }

            return File(bytes, "application/pdf");
        }
        public JsonResult SendMail(List<string> values)
        {
            var paySlipList = (List<PaySlipItem>)Session["PaySlipList"];
            string emailLogMessage = "Please generate salary slip before sending email.";
            _path = Session["Path"]?.ToString();
            if (paySlipList != null && paySlipList.Count > 0)
            {
                var numberOfErrorEmail = 0;
                var emailHelper = new EmailHelper();

                paySlipList = FilterPaySlipList(values, paySlipList);

                foreach (var paySlip in paySlipList)
                {
                    numberOfErrorEmail += emailHelper.SendSalaryMail(paySlip);
                }

                if (numberOfErrorEmail > 0)
                    emailLogMessage = numberOfErrorEmail + " unsuccessful emails.";
                else
                    emailLogMessage = "All mails sent successfully.";
                ViewBag.Message = emailLogMessage;

                if (string.IsNullOrWhiteSpace(_path))
                    _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["SalarySlipPath"]);
            }
            var logMsg = DateTime.Today.ToString("MMM/dd/yyyy") +
                         Environment.NewLine +
                         emailLogMessage +
                         Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(_path))
                _path = Path.Combine(new[] { _path, "Log.txt" });

            Util.WriteLog(_path, logMsg);
            return Json(emailLogMessage);
        }

        public ActionResult DownloadSalarySlip(string values)
        {
            MemoryStream memoryStream = null;
            List<PaySlipItem> paySlipList;
            try
            {
                if (Session["PaySlipList"] != null)
                {
                    paySlipList = (List<PaySlipItem>)Session["PaySlipList"];
                    var valueArray = new JavaScriptSerializer().Deserialize<List<string>>(values);

                    _path = Session["Path"]?.ToString();

                    List<PaySlipItem> filteredPaySlipItems = FilterPaySlipList(valueArray, paySlipList);

                    using (memoryStream = new MemoryStream())
                    {
                        using (ZipArchive ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            foreach (PaySlipItem fileName in filteredPaySlipItems)
                            {
                                ziparchive.CreateEntryFromFile(fileName.PaySlipFilePath, Path.GetFileName(fileName.PaySlipFilePath));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("ErrorOnDownload", e);
            }
            return File(memoryStream?.ToArray(), "application/zip", "SalarySlips.zip");
        }
        #endregion

        #region Common Methods
        private void UploadFile(HttpPostedFileBase postedFile, ref List<string> errorList)
        {
            try
            {
                if (postedFile?.FileName != null)
                {
                    string fileExtension = Path.GetExtension(postedFile.FileName);

                    if (fileExtension.ToLower().Contains(".xml"))
                        Session["xmlFileName"] = _xmlFileName = Path.GetFileName(postedFile.FileName);
                    else if (fileExtension.ToLower().Contains(".xls"))
                        Session["excelFileName"] = _excelFileName = Path.GetFileName(postedFile.FileName);
                    else
                        errorList.Add("Upload Only XML and Excel files.");

                    if (errorList.Count == 0)
                    {
                        if (Session["Path"] == null)
                        {
                            var dateTimePath = DateTime.Now.ToString("yyyy-MMM-hhmmss");
                            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                ConfigurationManager.AppSettings["SalarySlipPath"],
                                dateTimePath);
                            Session["Path"] = _path;
                            if (!Directory.Exists(_path))
                                Directory.CreateDirectory(_path);
                        }
                        var filePath = Path.Combine(_path, Path.GetFileName(postedFile.FileName));
                        postedFile.SaveAs(filePath);
                    }
                }
                else
                {
                    errorList.Add("Please select files to upload.");
                }
            }
            catch (Exception e)
            {
                _path = Path.Combine(new[] { _path, "ErrorLog.txt" });
                Util.WriteLog(_path, ex: e);
            }
        }
        private void ValidateXml(string path, string xmlFileName, ref List<string> errorList)
        {
            bool validPaySlipList = true;
            try
            {
                XDocument document = XDocument.Load(Path.Combine(path, xmlFileName));

                var employeeDups = document.Descendants("Employee").GroupBy(x => x.Element("email")?.Value)
                    .Where(gr => gr.Count() > 1).Select(x => x.Key).ToList();
                if (employeeDups.Any())
                    errorList.Add(string.Format("Duplication found in employee email(s): {0}",
                        String.Join(", ", employeeDups.Select(dupl => dupl))));

                int emptyEmail = document.Descendants("Employee").Where(x => string.IsNullOrWhiteSpace(x.Element("email")?.Value)).ToList().Count;
                if (emptyEmail > 0)
                    errorList.Add(string.Format("{0} record(s) has no email id.", emptyEmail));

                int emptyEmpId = document.Descendants("Employee").Where(x => string.IsNullOrWhiteSpace(x.Element("EmployeeNumber")?.Value)).ToList().Count;
                if (emptyEmpId > 0)
                    errorList.Add(string.Format("{0} record(s) has no emp id.", emptyEmpId));

                var employeeIdDups = document.Descendants("Employee").GroupBy(x => x.Element("EmployeeNumber")?.Value != String.Empty ? x.Element("EmployeeNumber")?.Value : "0")
                    .Where(gr => gr.Count() > 1).Select(x => x.Key).ToList();
                if (employeeIdDups.Any())
                    errorList.Add(string.Format("Duplication found in emp id(s) in XML file: {0}",
                        String.Join(", ", employeeIdDups.Select(int.Parse))));

            }
            catch (Exception e)
            {
                //When the uploaded XML is incorrect.
                errorList.Add(e.GetType().Name);
                errorList.Add(e.Message);
            }
        }
        private void ValidateExcel(List<PaySlipItem> paySlipList, ref List<string> errorList)
        {
            bool validPaySlipList = true;
            var empDuplications = paySlipList.AsEnumerable().GroupBy(payslip => payslip.EmployeeNumber).Where(gr => gr.Count() > 1).ToList();

            if (empDuplications.Any())
                errorList.Add(string.Format("Duplication found in employee number(s): {0}",
                    String.Join(", ", empDuplications.Select(dupl => dupl.Key))));

            int emptyEmpNumber = paySlipList.AsEnumerable().Where(x => x.EmployeeNumber == 0).ToList().Count;
            if (emptyEmpNumber > 0)
                errorList.Add(string.Format("{0} record(s) has no employee number.", emptyEmpNumber));
        }
        private List<PaySlipItem> GetPaySlipList(string excelFileName, string xmlFileName, int month, int year, ref List<string> errorList)
        {
            List<PaySlipItem> paySlipList;
            //Get the List of payslips from uploaded excel.
            using (var client = new SalarySlipClient())
            {
                paySlipList = client.GetPaySlipItems(_path, excelFileName, xmlFileName, month, year, ref errorList);
            }
            return paySlipList;
        }

        private List<PaySlipItem> FilterPaySlipList(List<string> empNumbers, List<PaySlipItem> paySlipItems)
        {
            var filteredList = paySlipItems.Where(item => empNumbers.Contains(item.EmployeeNumber.ToString()))
                .Select(x => x).ToList();

            return filteredList;
        }

        #endregion

        public ActionResult ErrorOnDownload(Exception exception)
        {
            ViewBag.DownloadErrMsg = exception.Message;
            return View();
        }
    }
}