using EventPlanner.Web.Data;
using EventPlanner.Web.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Dashboard()
        {
            if (Session["Rol"] == null)
                return RedirectToAction("Login", "Usuario");
            if (Session["Rol"].ToString() != "ADMIN")
                return RedirectToAction("Index", "Home");

            ViewBag.Nombre = Session["Nombre"];
            ViewBag.Rol    = Session["Rol"];

            EventoDAO dao = new EventoDAO();
            List<Evento> eventos = dao.Listar();
            return View(eventos);
        }
    }
}
