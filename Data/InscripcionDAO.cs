using EventPlanner.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EventPlanner.Web.Data
{
    public class InscripcionDAO
    {
        private readonly Conexion _cn = new Conexion();

        public List<Evento> ListarEventosDisponibles()
        {
            var lista = new List<Evento>();
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT E.*, T.NombreTipoEvento,
                           (SELECT COUNT(*) FROM Inscripcion I WHERE I.IdEvento = E.IdEvento) AS CuposOcupados
                    FROM Evento E
                    INNER JOIN TipoEvento T ON E.IdTipoEvento = T.IdTipoEvento
                    WHERE E.Activo = 1
                      AND CAST(GETDATE() AS DATE) BETWEEN E.FechaInicioInscripcion AND E.FechaFinInscripcion
                    ORDER BY E.FechaEvento";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read()) lista.Add(MapearEvento(dr));
            }
            return lista;
        }

        public List<Inscripcion> ListarPorUsuario(int IdUsuario)
        {
            var lista = new List<Inscripcion>();
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT I.*, E.NombreEvento, E.FechaEvento, E.HoraInicio, E.HoraFin,
                           E.Lugar, E.Modalidad, T.NombreTipoEvento, EQ.NombreEquipo
                    FROM Inscripcion I
                    INNER JOIN Evento E ON I.IdEvento = E.IdEvento
                    INNER JOIN TipoEvento T ON E.IdTipoEvento = T.IdTipoEvento
                    LEFT JOIN  Equipo EQ ON I.IdEquipo = EQ.IdEquipo
                    WHERE I.IdUsuario = @IdUsuario
                    ORDER BY E.FechaEvento";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        while (dr.Read()) lista.Add(MapearInscripcion(dr));
                }
            }
            return lista;
        }

        public List<Inscripcion> ListarPorEvento(int IdEvento)
        {
            var lista = new List<Inscripcion>();
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT I.*, E.NombreEvento, E.FechaEvento, E.HoraInicio, E.HoraFin,
                           E.Lugar, E.Modalidad, T.NombreTipoEvento, EQ.NombreEquipo,
                           U.NombreCompleto AS NombreUsuario, U.Correo AS CorreoUsuario,
                           U.Ficha AS FichaUsuario, P.NombrePrograma AS ProgramaUsuario
                    FROM Inscripcion I
                    INNER JOIN Evento E   ON I.IdEvento  = E.IdEvento
                    INNER JOIN TipoEvento T ON E.IdTipoEvento = T.IdTipoEvento
                    INNER JOIN Usuario U   ON I.IdUsuario = U.IdUsuario
                    INNER JOIN ProgramaFormacion P ON U.IdProgramaFormacion = P.IdProgramaFormacion
                    LEFT JOIN  Equipo EQ  ON I.IdEquipo  = EQ.IdEquipo
                    WHERE I.IdEvento = @IdEvento
                    ORDER BY U.NombreCompleto";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdEvento", IdEvento);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        while (dr.Read()) lista.Add(MapearInscripcionCompleta(dr));
                }
            }
            return lista;
        }

        // CORRECCION 12+13: validaciones completas en Inscribir
        public string Inscribir(int IdUsuario, int IdEvento, int? IdEquipo)
        {
            // CORRECCION 13: sesion invalida
            if (IdUsuario <= 0)
                return "Sesion invalida. Vuelve a iniciar sesion.";

            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();

                // Obtener datos del evento
                string modalidad = "";
                DateTime fechaEvento = DateTime.MinValue;
                using (SqlCommand c = new SqlCommand(
                    "SELECT Modalidad, FechaEvento FROM Evento WHERE IdEvento=@e", cn))
                {
                    c.Parameters.AddWithValue("@e", IdEvento);
                    using (SqlDataReader dr = c.ExecuteReader())
                    {
                        if (!dr.Read()) return "Evento no encontrado.";
                        modalidad   = dr["Modalidad"].ToString();
                        fechaEvento = Convert.ToDateTime(dr["FechaEvento"]);
                    }
                }

                // CORRECCION 12: validar que el equipo pertenezca al evento
                if (IdEquipo.HasValue)
                {
                    if (modalidad != "EQUIPO")
                        return "Este evento no es por equipos.";

                    using (SqlCommand c = new SqlCommand(
                        "SELECT COUNT(*) FROM Equipo WHERE IdEquipo=@eq AND IdEvento=@e", cn))
                    {
                        c.Parameters.AddWithValue("@eq", IdEquipo.Value);
                        c.Parameters.AddWithValue("@e",  IdEvento);
                        if ((int)c.ExecuteScalar() == 0)
                            return "El equipo seleccionado no pertenece a este evento.";
                    }
                }
                else if (modalidad == "EQUIPO")
                {
                    return "Debes seleccionar un equipo para este evento.";
                }

                // 1. Ya inscrito
                using (SqlCommand c = new SqlCommand(
                    "SELECT COUNT(*) FROM Inscripcion WHERE IdUsuario=@u AND IdEvento=@e", cn))
                {
                    c.Parameters.AddWithValue("@u", IdUsuario);
                    c.Parameters.AddWithValue("@e", IdEvento);
                    if ((int)c.ExecuteScalar() > 0)
                        return "Ya estas inscrito en este evento.";
                }

                // 2. Cruce de horario
                using (SqlCommand c = new SqlCommand(@"
                    SELECT COUNT(*) FROM Inscripcion I
                    INNER JOIN Evento E ON I.IdEvento = E.IdEvento
                    INNER JOIN Evento N ON N.IdEvento = @e
                    WHERE I.IdUsuario = @u
                      AND E.FechaEvento = N.FechaEvento
                      AND (E.HoraInicio < N.HoraFin AND E.HoraFin > N.HoraInicio)", cn))
                {
                    c.Parameters.AddWithValue("@u", IdUsuario);
                    c.Parameters.AddWithValue("@e", IdEvento);
                    if ((int)c.ExecuteScalar() > 0)
                        return "Tienes otro evento con el mismo horario ese dia.";
                }

                // 3. Cupos disponibles
                using (SqlCommand c = new SqlCommand(@"
                    SELECT E.CuposTotales -
                           (SELECT COUNT(*) FROM Inscripcion WHERE IdEvento=@e)
                    FROM Evento E WHERE E.IdEvento=@e", cn))
                {
                    c.Parameters.AddWithValue("@e", IdEvento);
                    object result = c.ExecuteScalar();
                    if (result == null || result == DBNull.Value || (int)result <= 0)
                        return "El evento no tiene cupos disponibles.";
                }

                // 4. Insertar
                using (SqlCommand c = new SqlCommand(@"
                    INSERT INTO Inscripcion(IdUsuario, IdEvento, IdEquipo, FechaInscripcion)
                    VALUES (@u, @e, @eq, GETDATE())", cn))
                {
                    c.Parameters.AddWithValue("@u",  IdUsuario);
                    c.Parameters.AddWithValue("@e",  IdEvento);
                    c.Parameters.AddWithValue("@eq",
                        IdEquipo.HasValue ? (object)IdEquipo.Value : DBNull.Value);
                    c.ExecuteNonQuery();
                }
            }
            return null;
        }

        // CORRECCION 2: no cancelar si el evento ya paso
        public string Cancelar(int IdInscripcion, int IdUsuario)
        {
            if (IdUsuario <= 0) return "Sesion invalida.";

            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();

                // Verificar que la inscripcion pertenece al usuario y obtener fecha del evento
                DateTime fechaEvento = DateTime.MinValue;
                using (SqlCommand c = new SqlCommand(@"
                    SELECT E.FechaEvento FROM Inscripcion I
                    INNER JOIN Evento E ON I.IdEvento = E.IdEvento
                    WHERE I.IdInscripcion=@id AND I.IdUsuario=@u", cn))
                {
                    c.Parameters.AddWithValue("@id", IdInscripcion);
                    c.Parameters.AddWithValue("@u",  IdUsuario);
                    object result = c.ExecuteScalar();
                    if (result == null) return "Inscripcion no encontrada.";
                    fechaEvento = Convert.ToDateTime(result);
                }

                // CORRECCION 2: bloquear cancelacion si el evento ya paso
                if (fechaEvento.Date < DateTime.Today)
                    return "No puedes cancelar una inscripcion de un evento que ya se realizo.";

                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Inscripcion WHERE IdInscripcion=@id AND IdUsuario=@u", cn))
                {
                    cmd.Parameters.AddWithValue("@id", IdInscripcion);
                    cmd.Parameters.AddWithValue("@u",  IdUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
            return null;
        }

        public bool EstaInscrito(int IdUsuario, int IdEvento)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Inscripcion WHERE IdUsuario=@u AND IdEvento=@e", cn))
                {
                    cmd.Parameters.AddWithValue("@u", IdUsuario);
                    cmd.Parameters.AddWithValue("@e", IdEvento);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public List<Inscripcion> ListarTodos()
        {
            var lista = new List<Inscripcion>();
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT I.*, E.NombreEvento, E.FechaEvento, E.HoraInicio, E.HoraFin,
                           E.Lugar, E.Modalidad, T.NombreTipoEvento, EQ.NombreEquipo,
                           U.NombreCompleto AS NombreUsuario, U.Correo AS CorreoUsuario,
                           U.Ficha AS FichaUsuario, P.NombrePrograma AS ProgramaUsuario
                    FROM Inscripcion I
                    INNER JOIN Evento E   ON I.IdEvento  = E.IdEvento
                    INNER JOIN TipoEvento T ON E.IdTipoEvento = T.IdTipoEvento
                    INNER JOIN Usuario U   ON I.IdUsuario = U.IdUsuario
                    INNER JOIN ProgramaFormacion P ON U.IdProgramaFormacion = P.IdProgramaFormacion
                    LEFT JOIN  Equipo EQ  ON I.IdEquipo  = EQ.IdEquipo
                    ORDER BY E.FechaEvento, U.NombreCompleto";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read()) lista.Add(MapearInscripcionCompleta(dr));
            }
            return lista;
        }

        private Evento MapearEvento(SqlDataReader dr)
        {
            var ev = new Evento
            {
                IdEvento               = Convert.ToInt32(dr["IdEvento"]),
                IdTipoEvento           = Convert.ToInt32(dr["IdTipoEvento"]),
                NombreEvento           = dr["NombreEvento"].ToString(),
                Descripcion            = dr["Descripcion"].ToString(),
                Modalidad              = dr["Modalidad"].ToString(),
                Lugar                  = dr["Lugar"].ToString(),
                FechaEvento            = Convert.ToDateTime(dr["FechaEvento"]),
                HoraInicio             = (TimeSpan)dr["HoraInicio"],
                HoraFin                = (TimeSpan)dr["HoraFin"],
                CuposTotales           = Convert.ToInt32(dr["CuposTotales"]),
                FechaInicioInscripcion = Convert.ToDateTime(dr["FechaInicioInscripcion"]),
                FechaFinInscripcion    = Convert.ToDateTime(dr["FechaFinInscripcion"]),
                Activo                 = Convert.ToBoolean(dr["Activo"]),
                FechaCreacion          = Convert.ToDateTime(dr["FechaCreacion"]),
                NombreTipoEvento       = dr["NombreTipoEvento"].ToString()
            };
            try { ev.CuposOcupados = Convert.ToInt32(dr["CuposOcupados"]); } catch { }
            return ev;
        }

        private Inscripcion MapearInscripcion(SqlDataReader dr)
        {
            return new Inscripcion
            {
                IdInscripcion    = Convert.ToInt32(dr["IdInscripcion"]),
                IdUsuario        = Convert.ToInt32(dr["IdUsuario"]),
                IdEvento         = Convert.ToInt32(dr["IdEvento"]),
                IdEquipo         = dr["IdEquipo"] != DBNull.Value ? (int?)Convert.ToInt32(dr["IdEquipo"]) : null,
                FechaInscripcion = Convert.ToDateTime(dr["FechaInscripcion"]),
                NombreEvento     = dr["NombreEvento"].ToString(),
                TipoEvento       = dr["NombreTipoEvento"].ToString(),
                FechaEvento      = Convert.ToDateTime(dr["FechaEvento"]),
                HoraInicio       = (TimeSpan)dr["HoraInicio"],
                HoraFin          = (TimeSpan)dr["HoraFin"],
                Lugar            = dr["Lugar"].ToString(),
                Modalidad        = dr["Modalidad"].ToString(),
                NombreEquipo     = dr["NombreEquipo"] != DBNull.Value ? dr["NombreEquipo"].ToString() : ""
            };
        }

        private Inscripcion MapearInscripcionCompleta(SqlDataReader dr)
        {
            var i = MapearInscripcion(dr);
            i.NombreUsuario   = dr["NombreUsuario"].ToString();
            i.CorreoUsuario   = dr["CorreoUsuario"].ToString();
            i.FichaUsuario    = dr["FichaUsuario"].ToString();
            i.ProgramaUsuario = dr["ProgramaUsuario"].ToString();
            return i;
        }
    }
}
