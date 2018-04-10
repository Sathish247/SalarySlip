using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.SalarySlip.Utilities.Report
{
    public static class Extension
    {
        public static string ToRoundOffString(this string str, ref List<string> errorList)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                decimal decimalValue;
                if (decimal.TryParse(str, out decimalValue))
                {
                    str = Math.Round(decimalValue, MidpointRounding.AwayFromZero).ToString();
                }
                else
                {
                    errorList.Add(string.Format("Unable to convert the '{0}' value to decimal", str));
                }
            }
            return str;
        }
    }
}
