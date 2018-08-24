using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
using NLTD.EmployeePortal.SalarySlip.Repository;
using NLTD.EmployeePortal.SalarySlip.Utilities.Report;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NLTD.EmployeePortal.SalarySlip.Dac.Dac
{
    public class SalarySlipDac : ISalarySlipHelper
    {
        public void Dispose()
        {
            //Nothing to dispose.
        }

        public List<PaySlipItem> GetPaySlipItems(string filePath, string excelFileName, string xmlFileName, int month, int year, ref List<string> errorList)
        {
            List<PaySlipItem> paySlipList = new List<PaySlipItem>();
            string conString = string.Empty;
            string extension = string.Empty;

            int usVisaFee;
            int travelReimbursement;
            int foodCoupon;
            decimal lopDays;
            decimal payDays;

            if (excelFileName != null)
            {
                extension = Path.GetExtension(excelFileName);
            }

            switch (extension)
            {
                case ".xls": //Excel 97-03.
                    conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                    break;

                case ".xlsx": //Excel 07 and above.
                    conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                    break;
            }
            conString = string.Format(conString, Path.Combine(filePath, excelFileName));
            DataTable dt = new DataTable();
            using (OleDbConnection connExcel = new OleDbConnection(conString))
            {
                using (OleDbCommand cmdExcel = new OleDbCommand())
                {
                    using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                    {
                        cmdExcel.Connection = connExcel;

                        //Get the name of First Sheet.
                        connExcel.Open();
                        DataTable dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                        //connExcel.Close();

                        ////Read Data from First Sheet.
                        //connExcel.Open();
                        cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                        odaExcel.SelectCommand = cmdExcel;
                        odaExcel.Fill(dt);
                        connExcel.Close();
                    }
                }
            }
            //errorList.Add("Columns: " + dt.Columns.Count.ToString());
            //errorList.Add("Rows: " + dt.Rows.Count.ToString());


            XDocument document = XDocument.Load(Path.Combine(filePath, xmlFileName));
            DateTime payDateTime = new DateTime(year, month, 1);
            string payMonth = payDateTime.ToString("MMMM");
            string payYear = payDateTime.ToString("yyyy");

            foreach (DataRow row in dt.Rows)
            {
                usVisaFee = 0;
                foodCoupon = 0;
                travelReimbursement = 0;
                lopDays = 0;
                payDays = 0;

                PaySlipItem paySlip = new PaySlipItem();

                int empNumber = 0;
                Int32.TryParse(Convert.ToString(row["Employee Number"]), out empNumber);

                //if no employee number or name associated, just skip it.
                if (empNumber == 0 && string.IsNullOrWhiteSpace(Convert.ToString(row["Particulars"])))
                {
                    continue; //Skip this row, Since it doesn't hold any employee's salary.
                }

                paySlip.EmployeeNumber = empNumber;
                paySlip.BasicAndDA = Convert.ToString(row["1#Basic & DA"]).ToRoundOffString(ref errorList);
                paySlip.HRA = Convert.ToString(row["2#House Rent Allowance"]).ToRoundOffString(ref errorList);
                paySlip.SpecialAllowance = Convert.ToString(row["3#Special Allowance"]).ToRoundOffString(ref errorList);
                paySlip.Conveyance = Convert.ToString(row["4#Conveyance"]).ToRoundOffString(ref errorList);
                paySlip.FlexiComponents = Convert.ToString(row["5#Flexi Components"]).ToRoundOffString(ref errorList);
                paySlip.Gross = Convert.ToString(row["Gross"]).ToRoundOffString(ref errorList);
                paySlip.DataCardCharges = Convert.ToString(row["6#Data Card Charges"]).ToRoundOffString(ref errorList);
                paySlip.UsVisaFeeOrNoticePeriod = Convert.ToString(row["Usvisa Fee/Notice period"]).ToRoundOffString(ref errorList);
                paySlip.MidShiftOrNightAllowance = Convert.ToString(row["Midshift/Night Allowance"]).ToRoundOffString(ref errorList);
                paySlip.OvertimeWages = Convert.ToString(row["Overtime Wages"]).ToRoundOffString(ref errorList);
                paySlip.TravellingAllowances = Convert.ToString(row["TRAVELLING-ALLOWANCES"]).ToRoundOffString(ref errorList);
                paySlip.TravelReimbursementOrOthers = Convert.ToString(row["Travel-Reimbursement/others"]).ToRoundOffString(ref errorList);
                paySlip.FoodCoupon = Convert.ToString(row["Food coupon"]).ToRoundOffString(ref errorList);
                paySlip.TotalEarnings = Convert.ToString(row["Total Earnings"]).ToRoundOffString(ref errorList);
                paySlip.ESIRecovery = Convert.ToString(row["ESI Recovery"]).ToRoundOffString(ref errorList);
                paySlip.ProfessionalTax = Convert.ToString(row["9#Professional Tax"]).ToRoundOffString(ref errorList);
                paySlip.TDS = Convert.ToString(row["10#TDS On Salary"]).ToRoundOffString(ref errorList);
                paySlip.EPFEmployeeRecovery = Convert.ToString(row["EPF Employee Recovery -12%"]).ToRoundOffString(ref errorList);
                paySlip.OtherDeduction = Convert.ToString(row["other deductions"]).ToRoundOffString(ref errorList);
                paySlip.VPF = Convert.ToString(row["VPF"]).ToRoundOffString(ref errorList);
                paySlip.TotalDeductions = Convert.ToString(row["Total Deductions"]).ToRoundOffString(ref errorList);
                paySlip.NetAmount = Convert.ToString(row["Net Payable"]).ToRoundOffString(ref errorList);
                paySlip.LOPDays = Convert.ToString(row["LOP days"]);
                paySlip.PayYear = payYear;
                paySlip.PayMonth = payMonth;

                //Get Datas from XML.
                XElement employee = (from node in document.Descendants("Employee")
                                     where Convert.ToInt32(node.Element("EmployeeNumber")?.Value) == paySlip.EmployeeNumber
                                     select node).FirstOrDefault();

                if (employee != null)
                {
                    paySlip.EmployeeName = employee.Element("Name")?.Value;
                    paySlip.Designation = employee.Element("Designation")?.Value;

                    // Parsing the DOJ date time.
                    if (employee.Element("DOJ") != null && !string.IsNullOrWhiteSpace(employee.Element("DOJ").Value))
                    {
                        DateTime dojDateTime;
                        if (!DateTime.TryParse(employee.Element("DOJ").Value, out dojDateTime))
                        {
                            errorList.Add(string.Format(
                                "Date of joining in XML has unidentified Date format {0} for employee number {1}",
                                employee.Element("DOJ").Value, paySlip.EmployeeNumber));
                        }
                        paySlip.DOJ = dojDateTime;
                    }
                    else
                    {
                        errorList.Add(string.Format(
                            "Date of joining for employee number {0} is not found in XML data file.", paySlip.EmployeeNumber));
                    }

                    //if the pay month and Date of Joining month are same then
                    //we are taking DOJ as New Joinee Date.
                    if (paySlip.DOJ.Month == month && paySlip.DOJ.Year == year)
                    {
                        paySlip.NewJoineeDate = employee.Element("DOJ")?.Value;
                    }

                    paySlip.Location = employee.Element("Location")?.Value;
                    paySlip.Department = employee.Element("Department")?.Value;
                    paySlip.PAN = employee.Element("PAN")?.Value;
                    paySlip.UAN = employee.Element("UAN")?.Value;
                    paySlip.PFAccNo = employee.Element("PFAccNo")?.Value;
                    paySlip.BankName = employee.Element("BankName")?.Value;
                    paySlip.BankAccNo = employee.Element("BankAccNo")?.Value;
                    paySlip.Email = employee.Element("email")?.Value;
                }
                else
                {
                    errorList.Add(string.Format(
                        "Employee number {0} is not found in XML data file.", paySlip.EmployeeNumber));
                }

                if (!string.IsNullOrWhiteSpace(paySlip.UsVisaFeeOrNoticePeriod))
                {
                    if (!Int32.TryParse(paySlip.UsVisaFeeOrNoticePeriod, out usVisaFee))
                        errorList.Add(string.Format(
                            "Usvisa Fee/Notice period Column has invalid number {0} for employee number {1}",
                            paySlip.UsVisaFeeOrNoticePeriod, paySlip.EmployeeNumber));
                }

                if (!string.IsNullOrWhiteSpace(paySlip.FoodCoupon))
                {
                    if (!Int32.TryParse(paySlip.FoodCoupon, out foodCoupon))
                        errorList.Add(string.Format(
                            "Food coupon period Column has invalid number {0} for employee number {1}",
                            paySlip.FoodCoupon, paySlip.EmployeeNumber));
                }

                if (!string.IsNullOrWhiteSpace(paySlip.TravelReimbursementOrOthers))
                {
                    if (!Int32.TryParse(paySlip.TravelReimbursementOrOthers, out travelReimbursement))
                        errorList.Add(string.Format(
                            "Travel-Reimbursement/others Column has invalid number {0} for employee number {1}",
                            paySlip.TravelReimbursementOrOthers, paySlip.EmployeeNumber));
                }
                //As per Suresh's suggestion the other amount are included with travel reimbursement.
                travelReimbursement = travelReimbursement + usVisaFee;
                paySlip.TravelReimbursementOrOthers = travelReimbursement == 0 ? "" : travelReimbursement.ToString();

                //Employee is salaried for the full day of this month.
                if (string.IsNullOrWhiteSpace(paySlip.LOPDays) && string.IsNullOrWhiteSpace(paySlip.NewJoineeDate))
                {
                    paySlip.PayDays = Convert.ToString(DateTime.DaysInMonth(payDateTime.Year, payDateTime.Month));
                }

                //Pay Days calculations based upon LOP Days
                if (!string.IsNullOrWhiteSpace(paySlip.LOPDays))
                {
                    if (!Decimal.TryParse(paySlip.LOPDays, out lopDays))
                        errorList.Add(string.Format(
                            "LOP days Column has invalid number {0} for employee number {1}",
                            paySlip.LOPDays, paySlip.EmployeeNumber));
                    payDays = DateTime.DaysInMonth(payDateTime.Year, payDateTime.Month) - lopDays;
                    paySlip.PayDays = payDays.ToString();
                }

                //Pay Days calculations based upon New Joinee Date.
                if (!string.IsNullOrWhiteSpace(paySlip.NewJoineeDate))
                {
                    DateTime joineeDateTime;
                    double noOfPayDays;
                    //Calculate numbers of days in the month.
                    int totalDaysinPayMonth = DateTime.DaysInMonth(payDateTime.Year, payDateTime.Month);
                    //Get End Datetime of the salary month.
                    DateTime newJoineeDateTime = new DateTime(payDateTime.Year, payDateTime.Month, totalDaysinPayMonth);

                    if (!DateTime.TryParse(paySlip.NewJoineeDate, out joineeDateTime))
                    {
                        errorList.Add(string.Format(
                            "New Joinee date Column has unidentified Date format {0} for employee number {1}",
                            paySlip.NewJoineeDate, paySlip.EmployeeNumber));
                    }
                    noOfPayDays = (newJoineeDateTime - joineeDateTime).TotalDays + 1;

                    if (noOfPayDays < 0 || noOfPayDays > totalDaysinPayMonth)
                    {
                        int joinDay = joineeDateTime.Day;
                        int joinMonth = joineeDateTime.Month;
                        int joinYear = joineeDateTime.Year;

                        // Changing the month to day and day to month, to bring valid Joniee Date.
                        DateTime joinDateTime;
                        if (joinDay <= 12)
                        {
                            joinDateTime = new DateTime(joinYear, joinDay, joinMonth);
                            noOfPayDays = (newJoineeDateTime - joinDateTime).TotalDays + 1;
                        }

                        if (noOfPayDays < 0 || noOfPayDays > totalDaysinPayMonth)
                        {
                            errorList.Add(string.Format(
                                "New Joinee date Column has invalid Date format {0} for employee number {1}",
                                paySlip.NewJoineeDate, paySlip.EmployeeNumber));
                        }
                    }
                    //Calculate pay days from LOP days for the New Joinee.
                    noOfPayDays = noOfPayDays - Convert.ToDouble(lopDays);
                    //payDateTime holds the first day of the salary month year.
                    paySlip.PayDays = Convert.ToString(noOfPayDays);
                }

                if (string.IsNullOrWhiteSpace(paySlip.LOPDays))
                {
                    paySlip.LOPDays = "0";
                }

                paySlip.PaySlipFilePath = Path.Combine(filePath, paySlip.EmployeeName + "_" +
                                 paySlip.EmployeeNumber + "_" + paySlip.PayMonth.Substring(0, 3) +
                                 paySlip.PayYear + ".pdf");
                paySlipList.Add(paySlip);
            }
            return paySlipList;
        }
    }
}