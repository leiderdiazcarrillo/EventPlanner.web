using EventPlanner.Web.Data;
using EventPlanner.Web.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class EquipoController : Controller
    {
        private readonly EquipoDAO _dao = new EquipoDAO();
        private readonly EventoDAO _eventoDao = new EventoDAO();

        private ActionResult AdminGuard()
        {
            if (Session["Rol"] == null) return RedirectToAction("Login", "Usuario");
            if (Session["Rol"].ToString() != "ADMIN") return RedirectToAction("Index", "Home");
            return null;
        }

        public ActionResult Index()
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            return View(_dao.Listar());
        }

        public ActionResult Crear()
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            ViewBag.Eventos = new SelectList(_eventoDao.Listar(), "IdEvento", "NombreEvento");
            return View(new Equipo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Equipo modelo)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;

            if (modelo.IdEvento > 0)
            {
                Evento ev = _eventoDao.ObtenerPorId(modelo.IdEvento);
                if (ev != null) modelo.ModalidadEvento = ev.Modalidad;
            }

            ViewBag.Eventos = new SelectList(_eventoDao.Listar(), "IdEvento", "NombreEvento", modelo.IdEvento);

            if (modelo.CantidadMinima > modelo.CantidadMaxima)
            {
                ModelState.AddModelError("", "La cantidad mínima de integrantes no puede ser mayor que la cantidad máxima.");
            }

            if (!ModelState.IsValid) return View(modelo);

            if (_dao.Registrar(modelo))
            {
                TempData["Mensaje"] = "Equipo creado correctamente.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "No se pudo crear el equipo.");
            return View(modelo);
        }

        public ActionResult Editar(int? id)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            if (id == null) return RedirectToAction("Index");

            Equipo item = _dao.ObtenerPorId(id.Value);
            if (item == null) return HttpNotFound();
            ViewBag.Eventos = new SelectList(_eventoDao.Listar(), "IdEvento", "NombreEvento", item.IdEvento);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Equipo modelo)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;

            if (modelo.IdEvento > 0)
            {
                Evento ev = _eventoDao.ObtenerPorId(modelo.IdEvento);
                if (ev != null) modelo.ModalidadEvento = ev.Modalidad;
            }

            ViewBag.Eventos = new SelectList(_eventoDao.Listar(), "IdEvento", "NombreEvento", modelo.IdEvento);

            if (modelo.CantidadMinima > modelo.CantidadMaxima)
            {
                ModelState.AddModelError("", "La cantidad mínima de integrantes no puede ser mayor que la cantidad máxima.");
            }

            if (!ModelState.IsValid) return View(modelo);

            if (_dao.Editar(modelo))
            {
                TempData["Mensaje"] = "Equipo actualizado correctamente.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "No se pudo actualizar el equipo.");
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            _dao.Eliminar(id);
            TempData["Mensaje"] = "Equipo eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
