using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Web.Models
{
    public class Evento : IValidatableObject
    {
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "El tipo de evento es obligatorio.")]
        public int IdTipoEvento { get; set; }

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string NombreEvento { get; set; }

        [Required(ErrorMessage = "La descripcion es obligatoria.")]
        [StringLength(300, ErrorMessage = "La descripcion no puede exceder 300 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La modalidad es obligatoria.")]
        [StringLength(20)]
        public string Modalidad { get; set; }

        [Required(ErrorMessage = "El lugar es obligatorio.")]
        [StringLength(100, ErrorMessage = "El lugar no puede exceder 100 caracteres.")]
        public string Lugar { get; set; }

        [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaEvento { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "Los cupos totales son obligatorios.")]
        [Range(1, 2000, ErrorMessage = "Los cupos deben estar entre 1 y 2000.")]
        public int CuposTotales { get; set; }

        [Required(ErrorMessage = "La fecha de inicio de inscripcion es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaInicioInscripcion { get; set; }

        [Required(ErrorMessage = "La fecha fin de inscripcion es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaFinInscripcion { get; set; }

        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreTipoEvento { get; set; }

        // Propiedad calculada para reportes
        public int CuposOcupados { get; set; }
        public int CuposDisponibles => CuposTotales - CuposOcupados;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // CORRECCION 7: fecha futura solo al crear (IdEvento == 0)
            if (IdEvento == 0 && FechaEvento.Date < DateTime.Today)
                yield return new ValidationResult(
                    "La fecha del evento no puede ser en el pasado.",
                    new[] { nameof(FechaEvento) });

            if (HoraInicio >= HoraFin)
                yield return new ValidationResult(
                    "La hora de inicio debe ser anterior a la hora de fin.",
                    new[] { nameof(HoraInicio), nameof(HoraFin) });

            if (FechaInicioInscripcion > FechaFinInscripcion)
                yield return new ValidationResult(
                    "La fecha de inicio de inscripcion no puede ser posterior al cierre.",
                    new[] { nameof(FechaInicioInscripcion), nameof(FechaFinInscripcion) });

            if (FechaFinInscripcion > FechaEvento)
                yield return new ValidationResult(
                    "El cierre de inscripciones no puede ser posterior a la fecha del evento.",
                    new[] { nameof(FechaFinInscripcion), nameof(FechaEvento) });
        }
    }
}
