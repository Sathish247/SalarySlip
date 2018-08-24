using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using NLTD.EmployeePortal.SalarySlip.Utilities.Report;

namespace NLTD.EmployeePortal.SalarySlip.Ux.AppHelpers
{
    public class EmailHelper
    {
        private void SendHtmlFormattedEmail(string recepientEmail, IList<string> ccEmail, string subject, string body, string emailType, string attachmentFile = "")
        {
            string path = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(attachmentFile))
                    path = Path.GetDirectoryName(attachmentFile);
                else
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["SalarySlipPath"]);

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["UserName"]);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(recepientEmail));
                    mailMessage.Priority = MailPriority.Normal;
                    if (!string.IsNullOrWhiteSpace(attachmentFile))
                    {
                        Attachment attachment = new Attachment(attachmentFile)
                        {
                            Name = Path.GetFileName(attachmentFile)
                        };
                        mailMessage.Attachments.Add(attachment);
                    }
                    foreach (var item in ccEmail)
                    {
                        mailMessage.CC.Add(item);
                    }
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = ConfigurationManager.AppSettings["Host"];
                    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = ConfigurationManager.AppSettings["UserName"];
                    NetworkCred.Password = ConfigurationManager.AppSettings["Password"];
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                    smtp.Send(mailMessage);
                }
            }
            catch (Exception)
            {
                if (string.IsNullOrEmpty(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["SalarySlipPath"]);
                path = Path.Combine(new[] { path, "Errorlog.txt" });

                var message = "Salary slip file \"" + attachmentFile + "\" is not sent." + Environment.NewLine;
                Util.WriteLog(path, message);
                throw;
            }
        }
        
        public int SendSalaryMail(PaySlipItem paySlip)
        {
            try
            {
                var subject = "PaySlip for " + paySlip.PayMonth + " " + paySlip.PayYear;
                var body = "*** This is an automated message. Please do not reply to this email id. ***";

                if (!String.IsNullOrWhiteSpace(paySlip.FoodCoupon))
                {
                    body = "An amount of Rs " + paySlip.FoodCoupon + " has been loaded in your HDFC food card.<br/><br/>" + body;
                }

                SendHtmlFormattedEmail(paySlip.Email, new List<string>(), subject, body, "", paySlip.PaySlipFilePath);
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }        
    }
}