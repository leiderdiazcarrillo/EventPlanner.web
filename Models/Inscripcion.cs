using System;

namespace EventPlanner.Web.Models
{
    public class Inscripcion
    {
        public int IdInscripcion { get; set; }
        public int IdUsuario { get; set; }
        public int IdEvento { get; set; }
        public int? IdEquipo { get; set; }
        public DateTime FechaInscripcion { get; set; }

        // Navegacion - se cargan en JOINs
        public string NombreEvento { get; set; }
        public string TipoEvento { get; set; }
        public DateTime FechaEvento { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Lugar { get; set; }
        public string Modalidad { get; set; }
        public string NombreEquipo { get; set; }
        public string NombreUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public string FichaUsuario { get; set; }
        public string ProgramaUsuario { get; set; }
        public int CuposTotales { get; set; }
        public int CuposOcupados { get; set; }
        public int CuposDisponibles => CuposTotales - CuposOcupados;
    }
}
