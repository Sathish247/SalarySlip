using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
using NLTD.EmployeePortal.SalarySlip.Dac.Dac;
using NLTD.EmployeePortal.SalarySlip.Repository;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.SalarySlip.Dac.DbHelper
{
    public class SalarySlipHelper : ISalarySlipHelper
    {
        public void Dispose()
        {
            //Nothing to dispose...
        }

        public List<PaySlipItem> GetPaySlipItems(string filePath, string excelFileName, string xmlFileName, int month, int year, ref List<string> errorList)
        {
            using (var dac = new SalarySlipDac())
            {
                return dac.GetPaySlipItems(filePath, excelFileName, xmlFileName, month, year, ref errorList);
            }
        }
    }
}