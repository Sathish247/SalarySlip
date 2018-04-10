using System;
using System.Security.Principal;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.SalarySlip.Ux.AppHelpers
{
    public class BaseController : Controller
    {
        public Int64 UserId { get; set; }
        public Int64 OfficeId { get; set; }

        public bool IsLMSApprover { get; set; }

        public string ReportingToName { get; set; }

        public string Role { get; set; }

        public string IsAuthorized { get; set; }
        public BaseController()
        {

        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var identity = (WindowsIdentity)System.Web.HttpContext.Current.User.Identity;

            var windowsLoginName = identity?.Name?.ToString().ToUpper();

            //var windowsLoginName = "CORP\\MJAIN";

            ViewBag.DisplayName = windowsLoginName;
            
        }
    }
}
