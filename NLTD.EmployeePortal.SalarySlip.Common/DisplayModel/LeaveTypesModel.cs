﻿using System;

namespace NLTD.EmployeePortal.SalarySlip.Common.DisplayModel
{
    public class LeaveTypesModel
    {
        public string LeaveTypeText { get; set; }

        public Int64 LeaveTypeId { get; set; }

        public bool IsTimeBased { get; set; }
    }
}
