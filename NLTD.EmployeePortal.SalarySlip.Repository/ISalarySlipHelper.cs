using NLTD.EmployeePortal.SalarySlip.Common.DisplayModel;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.SalarySlip.Repository
{
    public interface ISalarySlipHelper : IDisposable
    {
        List<PaySlipItem> GetPaySlipItems(string filePath, string excelFileName, string xmlFileName, int month, int year, ref List<string> errorList);
    }
}