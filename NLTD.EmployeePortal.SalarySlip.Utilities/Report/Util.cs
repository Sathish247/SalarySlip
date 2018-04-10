using System;
using System.IO;

namespace NLTD.EmployeePortal.SalarySlip.Utilities.Report
{
    public static class Util
    {
        public static string GetFinYear(string month, string year)
        {
            string finYear;
            switch (month.ToLower())
            {
                case "january":
                case "february":
                case "march":
                    finYear = (Convert.ToInt32(year) - 1) + "-" + year;
                    break;
                default:
                    finYear = year + "-" + (Convert.ToInt32(year) + 1);
                    break;
            }
            return finYear;
        }
        public static string ConvertToWords(string numb)
        {
            string val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            string endStr = " Only";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = " and ";// just to separate whole numbers from points/cents   
                        endStr = "Paisa" + endStr;//Cents   
                        pointStr = ConvertDecimals(points);
                    }
                }
                val = $"{ConvertWholeNumber(wholeNo).Trim()}{andStr}{pointStr}{endStr}";
            }
            catch { }
            return val.ToUpper();
        }
        private static string ConvertWholeNumber(string number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX 
                bool isDone = false;//test if already translated 
                double dblAmt = (Convert.ToDouble(number));
                //if ((dblAmt > 0) && number.StartsWith("0")) 
                if (dblAmt >= 0)
                {//test for zero or digit zero in a nuemric 
                    beginsZero = number.StartsWith("0");

                    int numDigits = number.Length;
                    int pos = 0;//store digit grouping 
                    string place = "";//digit grouping name:hundres,thousand,etc... 
                    switch (numDigits)
                    {
                        case 1://ones' range 

                            word = Ones(number);
                            isDone = true;
                            break;
                        case 2://tens' range 
                            word = Tens(number);
                            isDone = true;
                            break;
                        case 3://hundreds' range 
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range 
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range 
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range 
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion... 
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!) 
                        if (number.Substring(0, pos) != "0" && number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(number.Substring(0, pos)) + place + ConvertWholeNumber(number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(number.Substring(0, pos)) + ConvertWholeNumber(number.Substring(pos));
                        }

                        //check for trailing zeros 
                        //if (beginsZero) word = " and " + word.Trim(); 
                    }
                    //ignore digit grouping names 
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }
        private static string Tens(string number)
        {
            int _Number = Convert.ToInt32(number);
            string name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = Tens(number.Substring(0, 1) + "0") + " " + Ones(number.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private static string Ones(string number)
        {
            int _Number = Convert.ToInt32(number);
            string name = "";
            switch (_Number)
            {
                case 0:
                    name = "Zero";
                    break;
                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }
        private static string ConvertDecimals(string number)
        {
            string cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = Ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }
        public static void WriteLog(string path, string message = "", Exception ex = null)
        {
            if (ex != null)
            {
                message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", ex.Message);
                message += Environment.NewLine;
                message += string.Format("StackTrace: {0}", ex.StackTrace);
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
            }
            if (string.IsNullOrWhiteSpace(message))
                message = "An UnKnow Error Occured.";

            //If error file name is not provided by user then the default application path is used.
            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(new[] { AppDomain.CurrentDomain.BaseDirectory, "ErrorLog" });
            var dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            if (!path.Trim().ToLower().EndsWith("log.txt"))
                path = Path.Combine(path, "ErrorLog.txt");

            File.AppendAllText(path, message);
        }

        //public static void GetValuesByColumnName(string colName, DataRow dataRow, ref PaySlipItem paySlipItem)
        //{
        //    try
        //    {
        //        if (colName.Trim().ToLower().Contains("number") && paySlipItem.EmployeeNumber == 0)
        //        {
        //            paySlipItem.EmployeeNumber = Convert.ToInt32(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("basic") && string.IsNullOrWhiteSpace(paySlipItem.BasicAndDA))
        //        {
        //            paySlipItem.BasicAndDA = Convert.ToString(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("house") && string.IsNullOrWhiteSpace(paySlipItem.HRA))
        //        {
        //            paySlipItem.HRA = Convert.ToString(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("special") && string.IsNullOrWhiteSpace(paySlipItem.SpecialAllowance))
        //        {
        //            paySlipItem.SpecialAllowance = Convert.ToString(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("conveyance") && string.IsNullOrWhiteSpace(paySlipItem.Conveyance))
        //        {
        //            paySlipItem.Conveyance = Convert.ToString(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("flexi") && string.IsNullOrWhiteSpace(paySlipItem.FlexiComponents))
        //        {
        //            paySlipItem.FlexiComponents = Convert.ToString(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("gross") && string.IsNullOrWhiteSpace(paySlipItem.Gross))
        //        {
        //            paySlipItem.Gross = Convert.ToString(dataRow[colName]);
        //        }
        //        else if (colName.Trim().ToLower().Contains("data") && string.IsNullOrWhiteSpace(paySlipItem.DataCardCharges))
        //        {
        //            paySlipItem.DataCardCharges = Convert.ToString(dataRow[colName]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        Console.WriteLine(ex);
        //    }
        //}
    }
}
