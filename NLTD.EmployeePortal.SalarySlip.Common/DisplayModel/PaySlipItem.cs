using System;

namespace NLTD.EmployeePortal.SalarySlip.Common.DisplayModel
{
    public class PaySlipItem
    {
        public int EmployeeNumber { get; set; }
        public string EmployeeName { get; set; }
        public string BasicAndDA { get; set; }
        public string HRA { get; set; }
        public string SpecialAllowance { get; set; }
        public string Conveyance { get; set; }
        public string FlexiComponents { get; set; }
        public string Gross { get; set; }
        public string DataCardCharges { get; set; }
        public string UsVisaFeeOrNoticePeriod { get; set; }
        public string MidShiftOrNightAllowance { get; set; }
        public string OvertimeWages { get; set; }
        public string TravellingAllowances { get; set; }
        public string TravelReimbursementOrOthers { get; set; }
        public string FoodCoupon { get; set; }
        public string TotalEarnings { get; set; }
        public string ESIRecovery { get; set; }
        public string TDS { get; set; }
        public string EPFEmployeeRecovery { get; set; }
        public string OtherDeduction { get; set; }
        public string VPF { get; set; }
        public string TotalDeductions { get; set; }
        public string NetAmount { get; set; }
        public string ProfessionalTax { get; set; }
        public string NewJoineeDate { get; set; }
        public string LOPDays { get; set; }
        public string PayDays { get; set; }
        public string PayYear { get; set; }
        public string PayMonth { get; set; }
        public string Designation { get; set; }
        public DateTime DOJ { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public string PAN { get; set; }
        public string UAN { get; set; }
        public string PFAccNo { get; set; }
        public string BankName { get; set; }
        public string BankAccNo { get; set; }
        public string Email { get; set; }
        public string PaySlipFilePath { get; set; }
    }
}
