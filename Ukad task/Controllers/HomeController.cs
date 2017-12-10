using System.Collections.Generic;
using System.Web.Mvc;
using Ukad_task.Models;
using Ukad_task.Services;
using Ukad_task.Custom;

namespace Ukad_task.Controllers
{
    public class HomeController : Controller
    {
        DBService service = new DBService();

        public ActionResult Index()
        {
            return View();
        }

        [ValidateAjax]
        public JsonResult TestSpeed(UserInput input)
        {
            int Timeout = (int)(input.Timeout * 60000);
            SiteMapReader reader = new SiteMapReader(input.URL, input.IncludeInnerSitemaps, Timeout);
            List<Report> reports = reader.MeasureResponseTime();
            System.Threading.Tasks.Task.Run(() =>
            {
                service.AddNewReports(reports);
            });
            return Json(new { Success = true, Reports = reports });
        }

        public JsonResult GetAllReports()
        {
            List<Report> reports = service.GetAllReports();
            return Json(new { Success = true, Reports = reports });
        }

        public JsonResult RemoveAllReports()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                service.RemoveAllReports();
            });
            var reports = new List<Report>();
            return Json(new { Success = true, Reports = reports });
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Vinnicenko Sergey";

            return View();
        }

        public ActionResult History()
        {
            ViewBag.Title = "History";
            return View();
        }
    }
}