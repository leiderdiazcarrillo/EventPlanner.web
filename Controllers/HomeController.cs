using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "EventPlanner - Gestiona tus eventos de forma eficiente";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Ponte en contacto con nuestro equipo de soporte";

            return View();
        }
    }
}
