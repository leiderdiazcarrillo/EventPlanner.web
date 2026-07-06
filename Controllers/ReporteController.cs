using EventPlanner.Web.Data;
using EventPlanner.Web.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class ReporteController : Controller
    {
        private bool EsAdmin() =>
            Session["Rol"] != null && Session["Rol"].ToString() == "ADMIN";

        public ActionResult Eventos()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Usuario");
            EventoDAO dao = new EventoDAO();
            List<Evento> lista = dao.Listar();
            return View(lista);
        }

        public ActionResult Inscripciones(int? IdEvento)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Usuario");

            InscripcionDAO dao = new InscripcionDAO();
            List<Inscripcion> lista = IdEvento.HasValue
                ? dao.ListarPorEvento(IdEvento.Value)
                : dao.ListarTodos();

            EventoDAO eventoDao = new EventoDAO();
            ViewBag.Eventos = eventoDao.Listar();
            ViewBag.IdEvento = IdEvento;
            return View(lista);
        }

        public ActionResult Inscritos(int? id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Usuario");

            if (id == null) return RedirectToAction("Eventos");

            InscripcionDAO dao = new InscripcionDAO();
            EventoDAO eventoDao = new EventoDAO();

            var evento = eventoDao.ObtenerPorId(id.Value);
            if (evento == null) return HttpNotFound();

            ViewBag.Evento = evento;
            List<Inscripcion> lista = dao.ListarPorEvento(id.Value);
            return View(lista);
        }
    }
}
