using EventPlanner.Web.Data;
using EventPlanner.Web.Models;
using EventPlanner.Web.ViewModels;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class UsuarioController : Controller
    {
        // CORRECCION 10: contador de intentos fallidos en sesion
        private const int MAX_INTENTOS = 5;

        private void CargarProgramas()
        {
            ViewBag.Programas = new ProgramaFormacionDAO().Listar();
        }

        [HttpGet]
        public ActionResult Registro()
        {
            CargarProgramas();
            return View();
        }

        // CORRECCION 8,9: validacion servidor + AntiForgery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(Usuario usuario)
        {
            usuario.Rol = "APRENDIZ";
            usuario.FechaRegistro = DateTime.Now;

            // CORRECCION 8: validacion de contrasena en servidor
            if (string.IsNullOrWhiteSpace(usuario.Contrasena) ||
                usuario.Contrasena.Length < 6 || usuario.Contrasena.Length > 8)
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "La contrasena debe tener entre 6 y 8 caracteres.";
                CargarProgramas(); return View();
            }

            // Validacion de correo institucional en servidor
            if (string.IsNullOrWhiteSpace(usuario.Correo) ||
                !usuario.Correo.EndsWith("@soy.sena.edu.co"))
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "Debes usar un correo institucional @soy.sena.edu.co";
                CargarProgramas(); return View();
            }

            var dao = new UsuarioDAO();
            if (dao.ExisteCorreo(usuario.Correo))
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "Ya existe un usuario registrado con este correo.";
                CargarProgramas(); return View();
            }
            if (dao.ExisteDocumento(usuario.NumeroDocumento))
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "Ya existe un usuario registrado con este numero de documento.";
                CargarProgramas(); return View();
            }

            dao.Registrar(usuario);
            ViewBag.TipoMensaje = "exito";
            ViewBag.Mensaje = "Usuario registrado correctamente.";
            CargarProgramas();
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Rol"] != null)
            {
                if (Session["Rol"].ToString() == "ADMIN")
                    return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("EventosDisponibles", "Inscripcion");
            }
            return View();
        }

        // CORRECCION 10: limite de intentos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel modelo)
        {
            // Verificar limite de intentos
            int intentos = Session["IntentosLogin"] != null ? (int)Session["IntentosLogin"] : 0;
            if (intentos >= MAX_INTENTOS)
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "Demasiados intentos fallidos. Cierra el navegador e intentalo de nuevo.";
                return View();
            }

            var dao = new UsuarioDAO();
            Usuario usuario = dao.Login(modelo.Correo, modelo.Contrasena);

            if (usuario != null)
            {
                Session["IntentosLogin"] = 0;
                Session["IdUsuario"] = usuario.IdUsuario;
                Session["Nombre"]    = usuario.NombreCompleto;
                Session["Rol"]       = usuario.Rol;

                // Cargar foto de perfil en sesion
                var daoFoto = new UsuarioDAO();
                Usuario uCompleto = daoFoto.ObtenerPorId(usuario.IdUsuario);
                Session["FotoPerfil"] = uCompleto?.FotoPerfil;

                if (usuario.Rol == "ADMIN")
                    return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("EventosDisponibles", "Inscripcion");
            }

            Session["IntentosLogin"] = intentos + 1;
            int restantes = MAX_INTENTOS - (intentos + 1);
            ViewBag.TipoMensaje = "error";
            ViewBag.Mensaje = restantes > 0
                ? "Correo o contrasena incorrectos. Intentos restantes: " + restantes
                : "Demasiados intentos fallidos. Cierra el navegador e intentalo de nuevo.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Usuario");
        }

        [HttpGet]
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarContrasena(string correo, string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(numeroDocumento))
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "Debes ingresar el correo y el numero de documento.";
                return View();
            }

            var dao = new UsuarioDAO();
            Usuario u = dao.BuscarParaRecuperar(correo, numeroDocumento);

            if (u == null)
            {
                ViewBag.TipoMensaje = "error";
                ViewBag.Mensaje = "No se encontro un usuario con ese correo y documento.";
                return View();
            }

            string temporal = Guid.NewGuid().ToString("N").Substring(0, 8);
            dao.CambiarContrasena(u.IdUsuario, temporal);

            ViewBag.TipoMensaje    = "exito";
            ViewBag.Mensaje        = "Tu contrasena temporal es:";
            ViewBag.ContrasenaTemp = temporal;
            ViewBag.Nombre         = u.NombreCompleto;
            return View();
        }

        // ── Ver Perfil ───────────────────────────────────────────────────
        [HttpGet]
        public ActionResult VerPerfil()
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Usuario");

            int id = (int)Session["IdUsuario"];
            var dao = new UsuarioDAO();
            Usuario u = dao.ObtenerPorId(id);
            if (u == null) return RedirectToAction("Login", "Usuario");

            var vm = new PerfilViewModel
            {
                IdUsuario      = u.IdUsuario,
                NombreCompleto = u.NombreCompleto,
                Correo         = u.Correo,
                Rol            = u.Rol,
                FotoPerfil     = u.FotoPerfil
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerPerfil(PerfilViewModel vm)
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Usuario");

            int id = (int)Session["IdUsuario"];
            string rol = Session["Rol"]?.ToString();
            var dao = new UsuarioDAO();
            Usuario u = dao.ObtenerPorId(id);
            if (u == null) return RedirectToAction("Login", "Usuario");

            vm.IdUsuario  = id;
            vm.Correo     = u.Correo;
            vm.Rol        = u.Rol;
            vm.FotoPerfil = u.FotoPerfil;

            bool cambios = false;

            // Actualizar nombre (ambos roles)
            if (!string.IsNullOrWhiteSpace(vm.NombreCompleto) &&
                vm.NombreCompleto.Trim().Length >= 3 &&
                vm.NombreCompleto.Trim() != u.NombreCompleto)
            {
                dao.ActualizarNombre(id, vm.NombreCompleto.Trim());
                Session["Nombre"] = vm.NombreCompleto.Trim();
                cambios = true;
            }

            // Foto de perfil (ambos roles)
            if (vm.FotoArchivo != null && vm.FotoArchivo.ContentLength > 0)
            {
                string[] permitidos = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string ext = Path.GetExtension(vm.FotoArchivo.FileName).ToLower();
                if (Array.IndexOf(permitidos, ext) < 0)
                {
                    ViewBag.TipoMensaje = "error";
                    ViewBag.Mensaje = "Solo se permiten imagenes JPG, PNG, GIF o WEBP.";
                    return View(vm);
                }
                if (vm.FotoArchivo.ContentLength > 2 * 1024 * 1024)
                {
                    ViewBag.TipoMensaje = "error";
                    ViewBag.Mensaje = "La imagen no puede superar 2 MB.";
                    return View(vm);
                }

                string carpeta = Server.MapPath("~/Content/Fotos");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                string nombreArchivo = "user_" + id + "_" + DateTime.Now.Ticks + ext;
                string ruta = Path.Combine(carpeta, nombreArchivo);
                vm.FotoArchivo.SaveAs(ruta);

                string rutaRelativa = "/Content/Fotos/" + nombreArchivo;
                dao.ActualizarFoto(id, rutaRelativa);
                vm.FotoPerfil = rutaRelativa;
                Session["FotoPerfil"] = rutaRelativa;
                cambios = true;
            }

            // Cambiar contrasena (solo APRENDIZ)
            if (rol == "APRENDIZ" &&
                !string.IsNullOrWhiteSpace(vm.ContrasenaActual) &&
                !string.IsNullOrWhiteSpace(vm.NuevaContrasena))
            {
                if (!dao.VerificarContrasena(id, vm.ContrasenaActual))
                {
                    ViewBag.TipoMensaje = "error";
                    ViewBag.Mensaje = "La contrasena actual es incorrecta.";
                    return View(vm);
                }
                if (vm.NuevaContrasena.Length < 6 || vm.NuevaContrasena.Length > 8)
                {
                    ViewBag.TipoMensaje = "error";
                    ViewBag.Mensaje = "La nueva contrasena debe tener entre 6 y 8 caracteres.";
                    return View(vm);
                }
                if (vm.NuevaContrasena != vm.ConfirmarContrasena)
                {
                    ViewBag.TipoMensaje = "error";
                    ViewBag.Mensaje = "La nueva contrasena y la confirmacion no coinciden.";
                    return View(vm);
                }
                dao.CambiarContrasena(id, vm.NuevaContrasena);
                cambios = true;
            }

            ViewBag.TipoMensaje = "exito";
            ViewBag.Mensaje = cambios ? "Perfil actualizado correctamente." : "No se detectaron cambios.";
            return View(vm);
        }
    }
}
