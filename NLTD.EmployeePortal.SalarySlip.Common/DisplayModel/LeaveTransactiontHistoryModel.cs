﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLTD.EmployeePortal.SalarySlip.Common.DisplayModel
{
    public class LeaveTransactiontHistoryModel
    {
        public Int64 TransactionId { get; set; }
        public Int64 UserId { get; set; }
        public Int64 LeaveId { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public DateTime TransactionDate { get; set; }
        public String TransactionType { get; set; }
        public Decimal NumberOfDays { get; set; }
        public Int64 TransactionBy { get; set; }
        public string Remarks { get; set; }
        public string Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeId { get; set; }
    }

    public class LeaveTransactionDetail
    {
        public string ReportingTo { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public List<LeaveTransactiontHistoryModel> LeaveTransactiontHistoryModel { get; set; }
    }
}
