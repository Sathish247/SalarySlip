using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;

namespace NLTD.EmployeePortal.SalarySlip.Utilities.Report
{
    public class PayslipReport
    {
        public static class PayslipGenerator
        {
            public static void GeneratePayslip(PaySlipItem payslipData, string outputFilePath, bool sealBox,
                bool passwordProtect)
            {

                Document doc = new Document(PageSize.A4, 25, 25, 50, 35);
                PdfWriter pdfWritr = PdfWriter.GetInstance(doc, new FileStream(outputFilePath, FileMode.Create));

                //Setting Linebreak
                Paragraph linebreak = new Paragraph("\n");

                // Add meta information to the document
                doc.AddAuthor("Northern Lights Technology Development (Chennai) Pvt Ltd");
                doc.AddCreator("GenPayslip - (NL-India)");
                doc.AddKeywords("Payslip");
                doc.AddSubject("Payslip for the Month of " + payslipData.PayMonth + "-" + payslipData.PayYear);
                doc.AddTitle("PAYSLIP - " + payslipData.EmployeeName);

                doc.Open();

                pdfWritr.PageEvent = new PayslipFooter();

                PdfPTable tblLogo = new PdfPTable(1);
                Image logoImg = Image.GetInstance(AppDomain.CurrentDomain.BaseDirectory + @"\images\Logo.jpg");
                logoImg.ScalePercent(25f);

                PdfPCell cellCompanyLogo = new PdfPCell(logoImg);
                cellCompanyLogo.Colspan = 1;
                cellCompanyLogo.Rowspan = 3;
                cellCompanyLogo.PaddingTop = 50f;
                cellCompanyLogo.Border = Rectangle.NO_BORDER;
                cellCompanyLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                tblLogo.AddCell(cellCompanyLogo);
                doc.Add(tblLogo);

                PdfPTable tblHeader = new PdfPTable(1);
                PdfPCell cellCompanyName = new PdfPCell(new Phrase(
                    "NORTHERN LIGHTS TECHNOLOGY DEVELOPMENT (CHENNAI) PVT. LTD.",
                    new Font(Font.FontFamily.HELVETICA, 16f, Font.BOLD,
                        BaseColor.BLACK)));
                cellCompanyName.Border = Rectangle.NO_BORDER;
                cellCompanyName.VerticalAlignment = Element.ALIGN_BOTTOM;
                cellCompanyName.HorizontalAlignment = Element.ALIGN_CENTER;
                //cellCompanyName.PaddingLeft = 50f;
                tblHeader.AddCell(cellCompanyName);
                doc.Add(tblHeader);

                doc.Add(linebreak);

                PdfPTable tblTitle = new PdfPTable(1);
                tblTitle.DefaultCell.Border = Rectangle.NO_BORDER;
                tblTitle.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;

                tblTitle.AddCell("Payslip for the Month of " + payslipData.PayMonth + "-" + payslipData.PayYear);
                PdfPCell cellFy = new PdfPCell(new Phrase("Financial Period " + Util.GetFinYear(payslipData.PayMonth, payslipData.PayYear),
                    new Font(Font.FontFamily.HELVETICA, 10f,
                        Font.ITALIC, BaseColor.BLACK)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                tblTitle.AddCell(cellFy);
                doc.Add(tblTitle);

                doc.Add(linebreak);

                PdfPTable tblEmpData = new PdfPTable(4);
                tblEmpData.DefaultCell.Border = Rectangle.NO_BORDER;
                tblEmpData.TotalWidth = 460f;
                tblEmpData.LockedWidth = true;

                //private&Conf Cell
                PdfPCell pncCell = new PdfPCell(new Phrase("Private & Confidential",
                    new Font(Font.FontFamily.HELVETICA, 10f,
                        Font.NORMAL, BaseColor.BLACK)));
                pncCell.Colspan = 4;
                pncCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                pncCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                pncCell.PaddingRight = 0f;
                pncCell.Border = Rectangle.NO_BORDER;
                tblEmpData.AddCell(pncCell);

                // Employee Table Header.
                PdfPCell empName = new PdfPCell(new Phrase(payslipData.EmployeeName,
                    new Font(Font.FontFamily.HELVETICA, 12f,
                        Font.NORMAL, BaseColor.WHITE)));
                empName.Colspan = 4;
                empName.HorizontalAlignment = Element.ALIGN_LEFT;
                empName.VerticalAlignment = Element.ALIGN_MIDDLE;
                empName.PaddingBottom = 6f;
                empName.PaddingLeft = 10f;
                empName.BorderWidthBottom = 2f;
                empName.BorderWidthTop = 0f;
                empName.BorderWidthRight = 0f;
                empName.BorderWidthLeft = 0f;
                empName.BorderColorBottom = new BaseColor(252, 176, 023);
                empName.BackgroundColor = new BaseColor(68, 150, 210);
                tblEmpData.AddCell(empName);

                Dictionary<string, string> emptableCells = new Dictionary<string, string>();
                emptableCells.Add("Employee Number", payslipData.EmployeeNumber.ToString());
                emptableCells.Add("Designation", payslipData.Designation);
                emptableCells.Add("Date of Joining", Convert.ToDateTime(payslipData.DOJ).ToString("dd-MMM-yyyy").ToUpper());
                emptableCells.Add("Location", payslipData.Location);
                emptableCells.Add("Department", payslipData.Department);
                emptableCells.Add("PAN", payslipData.PAN);
                emptableCells.Add("UAN", payslipData.UAN);
                emptableCells.Add("PF Account Number", payslipData.PFAccNo);
                emptableCells.Add("Bank Name", payslipData.BankName);
                emptableCells.Add("Bank Account Number", payslipData.BankAccNo);
                emptableCells.Add("Days Paid", payslipData.PayDays);
                emptableCells.Add("Loss of Pay Days", payslipData.LOPDays);

                int iCount = 1;
                int jCount = 0;
                foreach (var empTabCell in emptableCells)
                {
                    iCount += 1;
                    if (iCount % 2 == 0)
                        jCount = jCount + 1;

                    AddEmpCellToTable(ref tblEmpData, empTabCell.Key, empTabCell.Value, jCount % 2);
                }
                
                doc.Add(tblEmpData);

                doc.Add(linebreak);
                doc.Add(linebreak);

                PdfPTable tblSalaryData = new PdfPTable(4);
                tblSalaryData.TotalWidth = 460f;
                tblSalaryData.LockedWidth = true;
                float[] colWidths = new float[] { 165f, 65f, 165f, 65f };
                tblSalaryData.SetWidths(colWidths);

                // Salary Table Header.
                PdfPCell earning = new PdfPCell(new Phrase("Earning", new Font(Font.FontFamily.HELVETICA, 10f,
                        Font.NORMAL, BaseColor.WHITE)));
                earning.HorizontalAlignment = Element.ALIGN_LEFT;
                earning.VerticalAlignment = Element.ALIGN_MIDDLE;
                earning.PaddingBottom = 6f;
                earning.PaddingLeft = 10f;
                earning.BorderWidth = 0.5f;
                earning.BorderColor = new BaseColor(225, 225, 225);
                earning.BackgroundColor = new BaseColor(68, 150, 210);

                PdfPCell amt = new PdfPCell(new Phrase("Amount", new Font(Font.FontFamily.HELVETICA, 10f,
                        Font.NORMAL, BaseColor.WHITE)));
                amt.HorizontalAlignment = Element.ALIGN_RIGHT;
                amt.VerticalAlignment = Element.ALIGN_MIDDLE;
                amt.PaddingBottom = 6f;
                amt.PaddingRight = 10f;
                amt.BorderWidth = 0.5f;
                amt.BorderColor = new BaseColor(225, 225, 225);
                amt.BackgroundColor = new BaseColor(68, 150, 210);

                PdfPCell deductions = new PdfPCell(new Phrase("Deductions", new Font(Font.FontFamily.HELVETICA, 10f,
                        Font.NORMAL, BaseColor.WHITE)));
                deductions.HorizontalAlignment = Element.ALIGN_LEFT;
                deductions.VerticalAlignment = Element.ALIGN_MIDDLE;
                deductions.PaddingBottom = 6f;
                deductions.PaddingLeft = 10f;
                deductions.BorderWidth = 0.5f;
                deductions.BorderColor = new BaseColor(225, 225, 225);
                deductions.BackgroundColor = new BaseColor(68, 150, 210);

                tblSalaryData.AddCell(earning);
                tblSalaryData.AddCell(amt);
                tblSalaryData.AddCell(deductions);
                tblSalaryData.AddCell(amt);
                
                //Creating earning list
                List<KeyValuePair<string, string>> earningKeyValuePair = new List<KeyValuePair<string, string>>();
                if (!string.IsNullOrWhiteSpace(payslipData.BasicAndDA))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Basic & DA", payslipData.BasicAndDA));
                if (!string.IsNullOrWhiteSpace(payslipData.HRA))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("House Rent Allowance", payslipData.HRA));
                if (!string.IsNullOrWhiteSpace(payslipData.SpecialAllowance))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Special Allowance", payslipData.SpecialAllowance));
                if (!string.IsNullOrWhiteSpace(payslipData.Conveyance))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Conveyance", payslipData.Conveyance));
                if (!string.IsNullOrWhiteSpace(payslipData.FlexiComponents))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Flexi Components", payslipData.FlexiComponents));
                if (!string.IsNullOrWhiteSpace(payslipData.OvertimeWages))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Overtime Pay", payslipData.OvertimeWages));
                if (!string.IsNullOrWhiteSpace(payslipData.DataCardCharges))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Data Card Reimbursement", payslipData.DataCardCharges));
                if (!string.IsNullOrWhiteSpace(payslipData.TravelReimbursementOrOthers))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Travel Reimbursement / Others", payslipData.TravelReimbursementOrOthers));
                if (!string.IsNullOrWhiteSpace(payslipData.TravellingAllowances))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Travel Allowance", payslipData.TravellingAllowances));
                if (!string.IsNullOrWhiteSpace(payslipData.MidShiftOrNightAllowance))
                    earningKeyValuePair.Add(new KeyValuePair<string, string>("Mid / Night Shift Allowance", payslipData.MidShiftOrNightAllowance));

                //Creating deduction list
                List<KeyValuePair<string, string>> deductionKeyValuePair = new List<KeyValuePair<string, string>>();
                if (!string.IsNullOrWhiteSpace(payslipData.EPFEmployeeRecovery))
                    deductionKeyValuePair.Add(new KeyValuePair<string, string>("EPF Employee Recovery - 12%", payslipData.EPFEmployeeRecovery));
                if (!string.IsNullOrWhiteSpace(payslipData.VPF))
                    deductionKeyValuePair.Add(new KeyValuePair<string, string>("Voluntary Provident Fund", payslipData.VPF));
                if (!string.IsNullOrWhiteSpace(payslipData.ProfessionalTax))
                    deductionKeyValuePair.Add(new KeyValuePair<string, string>("Professional Tax", payslipData.ProfessionalTax));
                if (!string.IsNullOrWhiteSpace(payslipData.TDS))
                    deductionKeyValuePair.Add(new KeyValuePair<string, string>("TDS On Salary", payslipData.TDS));
                if (!string.IsNullOrWhiteSpace(payslipData.OtherDeduction))
                    deductionKeyValuePair.Add(new KeyValuePair<string, string>("Loan Recovery/Deductions", payslipData.OtherDeduction));
                if (!string.IsNullOrWhiteSpace(payslipData.ESIRecovery))
                    deductionKeyValuePair.Add(new KeyValuePair<string, string>("ESI Deduction", payslipData.ESIRecovery));
                
                //To bring left side and right side of the elements are in same count.
                int diff = earningKeyValuePair.Count - deductionKeyValuePair.Count;
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                        deductionKeyValuePair.Add(new KeyValuePair<string, string>("", ""));
                }
                //Rare scenario mostly Earnings have higher elements than deduction.
                diff = deductionKeyValuePair.Count - earningKeyValuePair.Count;
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                        earningKeyValuePair.Add(new KeyValuePair<string, string>("", ""));
                }
                // left side and right side will have same count.
                for (int i = 0; i < earningKeyValuePair.Count; i++)
                {
                    AddSalaryCellToTable(ref tblSalaryData, earningKeyValuePair[i].Key, earningKeyValuePair[i].Value);
                    AddSalaryCellToTable(ref tblSalaryData, deductionKeyValuePair[i].Key, deductionKeyValuePair[i].Value);
                }
                
                tblSalaryData.AddCell(GetSalTotalCell("(A) Total Earnings", 1));
                tblSalaryData.AddCell(GetSalTotalCell(string.IsNullOrWhiteSpace(payslipData.TotalEarnings) ? "-" : payslipData.TotalEarnings, 0));

                tblSalaryData.AddCell(GetSalTotalCell("(B) Total Deductions", 1));
                tblSalaryData.AddCell(GetSalTotalCell(string.IsNullOrWhiteSpace(payslipData.TotalDeductions) ? "-" : payslipData.TotalDeductions, 0));

                doc.Add(tblSalaryData);
                doc.Add(linebreak);

                PdfPTable tblNetPay = new PdfPTable(4);
                tblNetPay.TotalWidth = 460f;
                tblNetPay.LockedWidth = true;

                PdfPCell netPay = new PdfPCell(new Phrase("Net Pay = (A)-(B)", new Font(Font.FontFamily.HELVETICA, 9f, Font.BOLD,
                        BaseColor.WHITE)));
                netPay.HorizontalAlignment = Element.ALIGN_LEFT;
                netPay.VerticalAlignment = Element.ALIGN_MIDDLE;
                netPay.PaddingBottom = 5f;
                netPay.PaddingLeft = 10f;
                netPay.BorderWidth = 0.5f;
                netPay.BorderColor = new BaseColor(225, 225, 225);
                netPay.BackgroundColor = new BaseColor(68, 150, 210);
                PdfPCell netPayAmt = new PdfPCell(new Phrase(payslipData.NetAmount, new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD,
                        BaseColor.BLACK)));
                netPayAmt.HorizontalAlignment = Element.ALIGN_RIGHT;
                netPayAmt.VerticalAlignment = Element.ALIGN_MIDDLE;
                netPayAmt.PaddingBottom = 5f;
                netPayAmt.PaddingRight = 10f;
                netPayAmt.BorderWidth = 0.5f;
                netPayAmt.BorderColor = new BaseColor(225, 225, 225);
                netPayAmt.Colspan = 3;

                PdfPCell amtInWord = new PdfPCell(new Phrase("Amount (in words)", new Font(Font.FontFamily.HELVETICA, 9f, Font.BOLD,
                        BaseColor.WHITE)));
                amtInWord.HorizontalAlignment = Element.ALIGN_LEFT;
                amtInWord.VerticalAlignment = Element.ALIGN_MIDDLE;
                amtInWord.PaddingBottom = 5f;
                amtInWord.PaddingLeft = 10f;
                amtInWord.BorderWidth = 0.5f;
                amtInWord.BorderColor = new BaseColor(225, 225, 225);
                amtInWord.BackgroundColor = new BaseColor(68, 150, 210);

                PdfPCell amtInWordContent = new PdfPCell(new Phrase(Util.ConvertToWords(payslipData.NetAmount),
                    new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD,
                        BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingBottom = 5f,
                    PaddingRight = 10f,
                    BorderWidth = 0.5f,
                    BorderColor = new BaseColor(225, 225, 225),
                    Colspan = 3
                };

                tblNetPay.AddCell(netPay);
                tblNetPay.AddCell(netPayAmt);
                tblNetPay.AddCell(amtInWord);
                tblNetPay.AddCell(amtInWordContent);

                doc.Add(tblNetPay);

                doc.Add(linebreak);

                PdfPTable tblBottom = new PdfPTable(1);
                tblBottom.TotalWidth = 460f;
                tblBottom.LockedWidth = true;

                PdfPCell bottomBox = new PdfPCell(new Phrase(
                    "For Northern Lights Technology Development (Chennai) Pvt. Ltd. \n\n\n\n\n\n\n\n Authorised Signatory",
                    new Font(Font.FontFamily.HELVETICA, 9f, Font.BOLD,
                        BaseColor.BLACK)));
                bottomBox.HorizontalAlignment = Element.ALIGN_LEFT;
                bottomBox.VerticalAlignment = Element.ALIGN_MIDDLE;
                bottomBox.PaddingBottom = 5f;
                bottomBox.PaddingLeft = 10f;
                bottomBox.BorderWidth = 0.5f;
                bottomBox.BorderColor = new BaseColor(225, 225, 225);
                PdfPCell bottomLine = new PdfPCell(new Phrase(
                    "This is a computer generated payslip and doesn't require signature or company seal.",
                    new Font(Font.FontFamily.HELVETICA, 9f, Font.BOLD,
                        BaseColor.BLACK)));
                bottomLine.HorizontalAlignment = Element.ALIGN_LEFT;
                bottomLine.VerticalAlignment = Element.ALIGN_MIDDLE;
                bottomLine.PaddingBottom = 5f;
                bottomLine.PaddingLeft = 0f;
                bottomLine.Border = Rectangle.NO_BORDER;

                tblBottom.AddCell(sealBox ? bottomBox : bottomLine);

                doc.Add(tblBottom);

                doc.Close();
                doc.Dispose();
                //if (passwordProtect)
                //{
                //    Utilities.pdfEncryptor(OutputFilePath,
                //        payslipData.PAN.ToUpper() + payslipData.DOJ.ToString("yyyy"));
                //}
            }

            private static PdfPCell GetEmpTableCell(string cellContent, int textStyle, int colorCode)
            {
                PdfPCell tableCell;
                switch (textStyle)
                {
                    case 1:
                        tableCell = new PdfPCell(new Phrase(cellContent,
                            new Font(Font.FontFamily.HELVETICA, 8f,
                                Font.BOLD, BaseColor.BLACK)));
                        break;
                    default:
                        tableCell = new PdfPCell(new Phrase(cellContent,
                            new Font(Font.FontFamily.HELVETICA, 7f,
                                Font.NORMAL, BaseColor.BLACK)));
                        break;
                }
                tableCell.Border = Rectangle.NO_BORDER;
                tableCell.HorizontalAlignment = Element.ALIGN_LEFT;
                tableCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                tableCell.PaddingLeft = 10f;
                tableCell.PaddingBottom = 5f;
                if (colorCode == 0)
                    tableCell.BackgroundColor = new BaseColor(225, 225, 225);
                return tableCell;
            }

            private static PdfPCell GetSalTableCell(string cellContent, int textStyle)
            {
                PdfPCell tableCell = new PdfPCell(new Phrase(cellContent,
                    new Font(Font.FontFamily.HELVETICA, 8f,
                       ((textStyle == 1) ? Font.BOLD : Font.NORMAL), BaseColor.BLACK)));

                if (textStyle == 1)
                {
                    tableCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableCell.PaddingLeft = 10f;
                }
                else
                {
                    tableCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableCell.PaddingRight = 10f;
                }
                tableCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                tableCell.PaddingBottom = 6f;
                tableCell.BorderWidth = 0.5f;
                tableCell.BorderColor = new BaseColor(225, 225, 225);
                return tableCell;
            }

            private static PdfPCell GetSalTotalCell(string cellContent, int textStyle)
            {
                PdfPCell tableCell = new PdfPCell(new Phrase(cellContent,
                    new Font(Font.FontFamily.HELVETICA, 9f, Font.BOLD,
                        BaseColor.BLACK)));
                if (textStyle == 1)
                {
                    tableCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    tableCell.PaddingLeft = 10f;
                }
                else
                {
                    tableCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    tableCell.PaddingRight = 10f;
                }
                tableCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                tableCell.PaddingBottom = 6f;
                tableCell.BorderWidth = 0.5f;
                tableCell.BorderColor = new BaseColor(225, 225, 225);
                tableCell.BackgroundColor = new BaseColor(210, 210, 210);
                return tableCell;
            }

            private static void AddSalaryCellToTable(ref PdfPTable tblSalaryData, string key, string value)
            {
                tblSalaryData.AddCell(GetSalTableCell(string.IsNullOrWhiteSpace(key) ? "" : key, 1));
                tblSalaryData.AddCell(GetSalTableCell(string.IsNullOrWhiteSpace(value) ? "" : value, 0));
            }
            private static void AddEmpCellToTable(ref PdfPTable tblEmpData, string key, string value, int colorCode)
            {
                tblEmpData.AddCell(GetEmpTableCell(string.IsNullOrWhiteSpace(key) ? "" : key, 1, colorCode));
                tblEmpData.AddCell(GetEmpTableCell(string.IsNullOrWhiteSpace(value) ? "-" : value, 0, colorCode));
            }
        }
        public class PayslipFooter : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document doc)
            {
                string footerText = "Registered Office: Northern Lights Technology Development. (Chennai) Private Limited \n334 OMR, Futura Techpark, Block B, I floor, OMR, Sholinganallur, \nChennai 600 119 Ph. 6571 5566";
                PdfPTable footerTbl = new PdfPTable(1);
                footerTbl.TotalWidth = 500;

                PdfPCell datacell = new PdfPCell(new Phrase(footerText, new Font(Font.FontFamily.HELVETICA, 6f, Font.BOLD, BaseColor.DARK_GRAY)));
                datacell.BorderColor = new BaseColor(225, 225, 225);
                datacell.BorderWidthTop = 0.5f;
                datacell.BorderWidthBottom = 0f;
                datacell.BorderWidthLeft = 0f;
                datacell.BorderWidthRight = 0f;
                datacell.HorizontalAlignment = Element.ALIGN_CENTER;
                footerTbl.AddCell(datacell);
                footerTbl.WriteSelectedRows(0, -1, 50, 50, writer.DirectContent);
            }
        }
    }
}
