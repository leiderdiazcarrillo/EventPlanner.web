using EventPlanner.Web.Data;
using EventPlanner.Web.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class EventoController : Controller
    {
        private readonly EventoDAO _dao = new EventoDAO();
        private readonly TipoEventoDAO _tipoDao = new TipoEventoDAO();
        private readonly EquipoDAO _equipoDao = new EquipoDAO();

        private ActionResult AdminGuard()
        {
            if (Session["Rol"] == null) return RedirectToAction("Login", "Usuario");
            if (Session["Rol"].ToString() != "ADMIN") return RedirectToAction("Index", "Home");
            return null;
        }

        private void PopulateSelects(int selectedTipo = 0, string selectedModalidad = null)
        {
            List<TipoEvento> tipos = _tipoDao.Listar();
            ViewBag.TipoEventos = new SelectList(tipos, "IdTipoEvento", "NombreTipoEvento", selectedTipo);
            ViewBag.Modalidades = new SelectList(new[] { "INDIVIDUAL", "EQUIPO" }, selectedModalidad);
        }

        public ActionResult Index()
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            return View(_dao.Listar());
        }

        public ActionResult Crear()
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            PopulateSelects();

            Evento nuevoEvento = new Evento
            {
                FechaEvento = DateTime.Today,
                FechaInicioInscripcion = DateTime.Today,
                FechaFinInscripcion = DateTime.Today,
                HoraInicio = new TimeSpan(8, 0, 0),
                HoraFin = new TimeSpan(12, 0, 0)
            };

            return View(nuevoEvento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Evento modelo)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            PopulateSelects(modelo.IdTipoEvento, modelo.Modalidad);
            if (!ModelState.IsValid) return View(modelo);

            modelo.Activo = true;
            modelo.FechaCreacion = DateTime.Now;

            if (_dao.Registrar(modelo))
            {
                TempData["Mensaje"] = "Evento creado correctamente.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "No se pudo crear el evento. Verifica que la fecha no sea en el pasado.");
            return View(modelo);
        }

        public ActionResult Editar(int? id)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            if (id == null) return RedirectToAction("Index");

            Evento ev = _dao.ObtenerPorId(id.Value);
            if (ev == null) return HttpNotFound();
            PopulateSelects(ev.IdTipoEvento, ev.Modalidad);
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Evento modelo)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            PopulateSelects(modelo.IdTipoEvento, modelo.Modalidad);
            if (!ModelState.IsValid) return View(modelo);

            Evento eventoOriginal = _dao.ObtenerPorId(modelo.IdEvento);
            if (eventoOriginal == null) return HttpNotFound();

            if (eventoOriginal.FechaEvento.Date < DateTime.Today)
            {
                ModelState.AddModelError("", "No se puede guardar cambios porque este evento ya se realizó en el pasado.");
                return View(modelo);
            }

            using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(new Data.Conexion().ObtenerConexion().ConnectionString))
            {
                cn.Open();
                string sql = @"SELECT COUNT(*) FROM Evento 
                       WHERE Lugar = @Lugar 
                         AND FechaEvento = @FechaEvento 
                         AND IdEvento <> @IdEvento
                         AND (HoraInicio < @HoraFin AND HoraFin > @HoraInicio)";

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Lugar", modelo.Lugar);
                    cmd.Parameters.AddWithValue("@FechaEvento", modelo.FechaEvento);
                    cmd.Parameters.AddWithValue("@IdEvento", modelo.IdEvento);
                    cmd.Parameters.AddWithValue("@HoraInicio", modelo.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", modelo.HoraFin);

                    if ((int)cmd.ExecuteScalar() > 0)
                    {
                        ModelState.AddModelError("", "No se pudo actualizar el evento. El lugar seleccionado (" + modelo.Lugar + ") ya se encuentra reservado en la fecha y rango de horario elegido.");
                        return View(modelo);
                    }
                }
            }

            if (_dao.Editar(modelo))
            {
                TempData["Mensaje"] = "Evento actualizado correctamente.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "No se pudo actualizar el evento por un problema en la base de datos.");
            return View(modelo);
        }

        public ActionResult CambiarEstado(int id)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            _dao.CambiarEstado(id);
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            int inscritos = _dao.ContarInscritos(id);
            if (inscritos > 0)
                TempData["Advertencia"] = "El evento fue eliminado junto con " + inscritos + " inscripcion(es) activa(s).";
            else
                TempData["Mensaje"] = "Evento eliminado correctamente.";
            _dao.Eliminar(id);
            return RedirectToAction("Index");
        }

        public ActionResult Equipos(int id)
        {
            ActionResult g = AdminGuard(); if (g != null) return g;
            List<Equipo> equipos = _equipoDao.ListarPorEvento(id);
            ViewBag.EventoId = id;
            return View("Equipo", equipos);
        }
    }
}
