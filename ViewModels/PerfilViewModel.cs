using System.ComponentModel.DataAnnotations;
using System.Web;

namespace EventPlanner.Web.ViewModels
{
    public class PerfilViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string NombreCompleto { get; set; }

        public string Correo { get; set; }
        public string Rol { get; set; }
        public string FotoPerfil { get; set; }

        // Solo para APRENDIZ
        public string ContrasenaActual { get; set; }
        public string NuevaContrasena { get; set; }
        public string ConfirmarContrasena { get; set; }

        public HttpPostedFileBase FotoArchivo { get; set; }
    }
}
