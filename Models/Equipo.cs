using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Web.Models
{
    public class Equipo : IValidatableObject
    {
        public int IdEquipo { get; set; }

        [Required(ErrorMessage = "El evento es obligatorio.")]
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string NombreEquipo { get; set; }

        [Required(ErrorMessage = "La cantidad minima es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad minima debe ser mayor que 0.")]
        public int CantidadMinima { get; set; }

        [Required(ErrorMessage = "La cantidad maxima es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad maxima debe ser mayor que 0.")]
        public int CantidadMaxima { get; set; }

        public string NombreEvento { get; set; }
        public string ModalidadEvento { get; set; }

        // CORRECCION 5: min < max en servidor
        // CORRECCION 6: evento debe ser EQUIPO
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CantidadMinima > CantidadMaxima)
                yield return new ValidationResult(
                    "La cantidad minima no puede ser mayor que la maxima.",
                    new[] { nameof(CantidadMinima), nameof(CantidadMaxima) });

            if (!string.IsNullOrEmpty(ModalidadEvento) && ModalidadEvento != "EQUIPO")
                yield return new ValidationResult(
                    "Solo se pueden crear equipos en eventos de modalidad EQUIPO.",
                    new[] { nameof(IdEvento) });
        }
    }
}
