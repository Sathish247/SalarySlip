﻿using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
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
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["SalarySlipPath"]);
                path = Path.Combine(new[] { path, "Errorlog.txt" });

                var message = "Salary slip file \"" + attachmentFile + "\" is not sent." + Environment.NewLine;
                Util.WriteLog(path, message);
                throw;
            }
        }
        //private string PopulateBody(string userName, string url, string Description, string requestFor, string empId, string requestType, string range, string duration, string reason, string approverName, string approverComments, string leaveId, string emailType)
        //{
        //    string body = string.Empty;
        //    //HttpRequest request = HttpContext.Current.Request;
        //    string baseUrl = ConfigurationManager.AppSettings["LMSUrl"]; // request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped);
        //    if (emailType == "Applied")
        //    {

        //        using (var stream = new FileStream(HostingEnvironment.MapPath("~/EmailTemplate.html"), FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            StreamReader reader = new StreamReader(stream);
        //            body = reader.ReadToEnd();
        //        }

        //    }
        //    else
        //    {
        //        using (var stream = new FileStream(HostingEnvironment.MapPath("~/EmailCCTemplate.html"), FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            StreamReader reader = new StreamReader(stream);
        //            body = reader.ReadToEnd();
        //        }
        //    }

        //    body = body.Replace("{Url}", url);


        //    body = body.Replace("{RequestFor}", requestFor);
        //    body = body.Replace("{EmpId}", empId);
        //    body = body.Replace("{RequestType}", requestType);
        //    body = body.Replace("{Range}", range);
        //    body = body.Replace("{Duration}", duration);
        //    body = body.Replace("{ApproverName}", approverName);
        //    if (userName != "")
        //        userName = " " + userName;
        //    body = body.Replace("{HelloUser}", userName);
        //    body = body.Replace("{Description}", Description);
        //    body = body.Replace("{Reason}", reason);
        //    body = body.Replace("{ApproverComments}", approverComments);

        //    body = body.Replace("{ManageLink}", baseUrl + "/Leaves/ManageLeaveRequest");
        //    return body;
        //}
        ////     public void SendEmail(Int64 leavId, string actionName)
        //     {
        //         try
        //         {
        //             EmailDataModel mdl;
        //             string helloUser = string.Empty;
        //             string descritpion = string.Empty;
        //             using (var client = new LeaveClient())
        //             {
        //                 mdl = client.GetEmailData(leavId, actionName);
        //             }
        //             if (mdl.ToEmailId == "AutoApproved")
        //             {
        //                 actionName = "Auto Approved";
        //                 mdl.ToEmailId = mdl.RequestorEmailId;
        //             }
        //             if (actionName == "Applied")
        //             {
        //                 helloUser = mdl.ReportingToName;
        //                 descritpion = "The request of the below employee is pending for your action:";
        //             }
        //             else
        //             {
        //                 helloUser = mdl.RequestFor;
        //                 descritpion = "Your request has been " + actionName + ".";
        //             }

        //             //cc Applied
        //             string emailType = string.Empty;
        //             string body = string.Empty;



        //             if (actionName == "Applied")
        //             {

        //                 body = PopulateBody(helloUser, "", descritpion
        //     , mdl.RequestFor, mdl.EmpId, mdl.LeaveTypeText, mdl.Date, mdl.Duration, mdl.Reason, mdl.ReportingToName, mdl.ApproverComments, Convert.ToString(leavId), actionName);

        //                 SendHtmlFormattedEmail(mdl.ToEmailId, mdl.CcEmailIds, "LMS - Request from " + mdl.RequestFor + " - " + "Pending", body, actionName);
        //             }
        //             else
        //             {
        //                 emailType = "ToAndCc";
        //                 body = PopulateBody(helloUser, "", descritpion
        //, mdl.RequestFor, mdl.EmpId, mdl.LeaveTypeText, mdl.Date, mdl.Duration, mdl.Reason, mdl.ReportingToName, mdl.ApproverComments, Convert.ToString(leavId), emailType);


        //                 SendHtmlFormattedEmail(mdl.ToEmailId, mdl.CcEmailIds, "LMS - Request from " + mdl.RequestFor + " - " + actionName, body, emailType);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             LogError(ex, leavId);
        //         }

        //     }
        public int SendSalaryMail(PaySlipItem paySlip)
        {
            try
            {
                var subject = "PaySlip for " + paySlip.PayMonth + " " + paySlip.PayYear;
                var body = "*** This is an automated message from Salary Slip. Please do not reply to this email id. ***";
                SendHtmlFormattedEmail(paySlip.Email, new List<string>(), subject, body, "", paySlip.PaySlipFilePath);
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        //private void LogError(Exception ex, Int64 leaveId)
        //{
        //    try
        //    {
        //        string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //        message += Environment.NewLine;
        //        message += "Leave ID:" + leaveId;
        //        message += Environment.NewLine;
        //        message += "-----------------------------------------------------------";
        //        message += Environment.NewLine;
        //        message += string.Format("Message: {0}", ex.Message);
        //        message += Environment.NewLine;
        //        message += string.Format("StackTrace: {0}", ex.StackTrace);
        //        message += Environment.NewLine;
        //        message += string.Format("Source: {0}", ex.Source);
        //        message += Environment.NewLine;
        //        message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
        //        message += Environment.NewLine;
        //        message += "-----------------------------------------------------------";
        //        message += Environment.NewLine;
        //        string path = HostingEnvironment.MapPath("~/ErrorLog/ErrorLog.txt");
        //        using (StreamWriter writer = new StreamWriter(path, true))
        //        {
        //            writer.WriteLine(message);
        //            writer.Close();
        //        }
        //    }
        //    catch { }
        //}
    }
}