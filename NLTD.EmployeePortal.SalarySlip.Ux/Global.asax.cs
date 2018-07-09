using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NLTD.EmployeePortal.SalarySlip.Ux
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Session_End(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SalarySlipPath"]))
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        ConfigurationManager.AppSettings["SalarySlipPath"]);

                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing.
            }
        }
    }
}
