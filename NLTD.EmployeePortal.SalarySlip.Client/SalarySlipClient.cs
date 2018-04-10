using System.Collections.Generic;
using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
using NLTD.EmployeePortal.SalarySlip.Dac.DbHelper;
using NLTD.EmployeePortal.SalarySlip.Repository;

namespace NLTD.EmployeePortal.SalarySlip.Client
{
    public class SalarySlipClient : ISalarySlipHelper
    {
        public void Dispose()
        {
            //Nothing to dispose...
        }

        public List<PaySlipItem> GetPaySlipItems(string filePath, string excelFileName, string xmlFileName, int month, int year, ref List<string> errorList)
        {
            using (ISalarySlipHelper helper = new SalarySlipHelper())
            {
                return helper.GetPaySlipItems(filePath, excelFileName, xmlFileName, month, year, ref errorList);
            }
        }

    }

}
