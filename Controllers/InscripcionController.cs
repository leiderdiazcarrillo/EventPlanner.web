using EventPlanner.Web.Data;
using EventPlanner.Web.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class InscripcionController : Controller
    {
        private readonly InscripcionDAO _dao = new InscripcionDAO();
        private readonly EquipoDAO _equipoDao = new EquipoDAO();

        private bool EsAprendiz() =>
            Session["Rol"] != null && Session["Rol"].ToString() == "APRENDIZ";

        private int? IdUsuario()
        {
            if (Session["IdUsuario"] == null) return null;
            return (int)Session["IdUsuario"];
        }

        public ActionResult EventosDisponibles()
        {
            if (!EsAprendiz()) return RedirectToAction("Login", "Usuario");
            return View(_dao.ListarEventosDisponibles());
        }

        public ActionResult Inscribirse(int? id)
        {
            if (!EsAprendiz()) return RedirectToAction("Login", "Usuario");

            if (id == null) return RedirectToAction("EventosDisponibles");

            var eventoDao = new EventoDAO();
            Evento evento = eventoDao.ObtenerPorId(id.Value);
            if (evento == null) return HttpNotFound();

            int? uid = IdUsuario();
            ViewBag.YaInscrito = uid.HasValue && _dao.EstaInscrito(uid.Value, id.Value);
            ViewBag.Equipos = _equipoDao.ListarPorEvento(id.Value);
            return View(evento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarInscripcion(int IdEvento, int? IdEquipo)
        {
            if (!EsAprendiz()) return RedirectToAction("Login", "Usuario");
            int? uid = IdUsuario();
            if (!uid.HasValue) return RedirectToAction("Login", "Usuario");

            string error = _dao.Inscribir(uid.Value, IdEvento, IdEquipo);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Inscribirse", new { id = IdEvento });
            }
            TempData["Exito"] = "Te inscribiste correctamente al evento.";
            return RedirectToAction("MisInscripciones");
        }

        public ActionResult MisInscripciones()
        {
            if (!EsAprendiz()) return RedirectToAction("Login", "Usuario");
            int? uid = IdUsuario();
            if (!uid.HasValue) return RedirectToAction("Login", "Usuario");
            return View(_dao.ListarPorUsuario(uid.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int IdInscripcion)
        {
            if (!EsAprendiz()) return RedirectToAction("Login", "Usuario");
            int? uid = IdUsuario();
            if (!uid.HasValue) return RedirectToAction("Login", "Usuario");

            string error = _dao.Cancelar(IdInscripcion, uid.Value);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Exito"] = "Inscripcion cancelada correctamente.";

            return RedirectToAction("MisInscripciones");
        }
    }
}
