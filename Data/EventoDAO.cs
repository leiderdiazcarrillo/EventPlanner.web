using EventPlanner.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EventPlanner.Web.Data
{
    public class EventoDAO
    {
        public List<Evento> Listar()
        {
            var lista = new List<Evento>();
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT E.*, T.NombreTipoEvento,
                               (SELECT COUNT(*) FROM Inscripcion I WHERE I.IdEvento = E.IdEvento) AS CuposOcupados
                               FROM Evento E
                               INNER JOIN TipoEvento T ON E.IdTipoEvento = T.IdTipoEvento
                               ORDER BY E.FechaCreacion DESC";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read()) lista.Add(Mapear(dr));
            }
            return lista;
        }

        public Evento ObtenerPorId(int id)
        {
            Evento ev = null;
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT E.*, T.NombreTipoEvento,
                               (SELECT COUNT(*) FROM Inscripcion I WHERE I.IdEvento = E.IdEvento) AS CuposOcupados
                               FROM Evento E
                               INNER JOIN TipoEvento T ON E.IdTipoEvento = T.IdTipoEvento
                               WHERE E.IdEvento = @id";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        if (dr.Read()) ev = Mapear(dr);
                }
            }
            return ev;
        }

        public bool Registrar(Evento e)
        {
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                if (e.FechaEvento.Date < DateTime.Today)
                    return false;

                if (ExisteCruceDeLugar(cn, e))
                    return false;

                e.FechaCreacion = DateTime.Now;
                e.Activo = true;

                string sql = @"INSERT INTO Evento
                    (IdTipoEvento, NombreEvento, Descripcion, Modalidad, Lugar,
                     FechaEvento, HoraInicio, HoraFin, CuposTotales,
                     FechaInicioInscripcion, FechaFinInscripcion, Activo, FechaCreacion)
                    VALUES
                    (@IdTipoEvento, @NombreEvento, @Descripcion, @Modalidad, @Lugar,
                     @FechaEvento, @HoraInicio, @HoraFin, @CuposTotales,
                     @FechaInicioInscripcion, @FechaFinInscripcion, @Activo, @FechaCreacion)";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    AgregarParams(cmd, e);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Editar(Evento e)
        {
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                if (e.FechaEvento.Date < DateTime.Today)
                    return false;

                if (ExisteCruceDeLugar(cn, e))
                    return false;

                string sql = @"UPDATE Evento SET
                    IdTipoEvento=@IdTipoEvento, NombreEvento=@NombreEvento,
                    Descripcion=@Descripcion, Modalidad=@Modalidad, Lugar=@Lugar,
                    FechaEvento=@FechaEvento, HoraInicio=@HoraInicio, HoraFin=@HoraFin,
                    CuposTotales=@CuposTotales, FechaInicioInscripcion=@FechaInicioInscripcion,
                    FechaFinInscripcion=@FechaFinInscripcion, Activo=@Activo
                    WHERE IdEvento=@IdEvento";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    AgregarParams(cmd, e);
                    cmd.Parameters.AddWithValue("@IdEvento", e.IdEvento);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public int ContarInscritos(int id)
        {
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Inscripcion WHERE IdEvento=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public bool Eliminar(int id)
        {
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                new SqlCommand("UPDATE Inscripcion SET IdEquipo=NULL WHERE IdEvento=@id", cn)
                { Parameters = { new SqlParameter("@id", id) } }.ExecuteNonQuery();
                new SqlCommand("DELETE FROM Inscripcion WHERE IdEvento=@id", cn)
                { Parameters = { new SqlParameter("@id", id) } }.ExecuteNonQuery();
                new SqlCommand("DELETE FROM Equipo WHERE IdEvento=@id", cn)
                { Parameters = { new SqlParameter("@id", id) } }.ExecuteNonQuery();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Evento WHERE IdEvento=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CambiarEstado(int id)
        {
            var cn_ = new Conexion();
            using (SqlConnection cn = cn_.ObtenerConexion())
            {
                cn.Open();
                string sql = "UPDATE Evento SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END WHERE IdEvento = @id";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private Evento Mapear(SqlDataReader dr)
        {
            var ev = new Evento
            {
                IdEvento = Convert.ToInt32(dr["IdEvento"]),
                IdTipoEvento = Convert.ToInt32(dr["IdTipoEvento"]),
                NombreEvento = dr["NombreEvento"].ToString(),
                Descripcion = dr["Descripcion"].ToString(),
                Modalidad = dr["Modalidad"].ToString(),
                Lugar = dr["Lugar"].ToString(),
                FechaEvento = Convert.ToDateTime(dr["FechaEvento"]),
                HoraInicio = (TimeSpan)dr["HoraInicio"],
                HoraFin = (TimeSpan)dr["HoraFin"],
                CuposTotales = Convert.ToInt32(dr["CuposTotales"]),
                FechaInicioInscripcion = Convert.ToDateTime(dr["FechaInicioInscripcion"]),
                FechaFinInscripcion = Convert.ToDateTime(dr["FechaFinInscripcion"]),
                Activo = Convert.ToBoolean(dr["Activo"]),
                FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                NombreTipoEvento = dr["NombreTipoEvento"].ToString()
            };
            try { ev.CuposOcupados = Convert.ToInt32(dr["CuposOcupados"]); } catch { }
            return ev;
        }

        private bool ExisteCruceDeLugar(SqlConnection cn, Evento e)
        {
            string sql = @"SELECT COUNT(*) FROM Evento 
                   WHERE Lugar = @Lugar 
                     AND FechaEvento = @FechaEvento 
                     AND IdEvento <> @IdEvento
                     AND (HoraInicio < @HoraFin AND HoraFin > @HoraInicio)";

            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Lugar", e.Lugar);
                cmd.Parameters.AddWithValue("@FechaEvento", e.FechaEvento);
                cmd.Parameters.AddWithValue("@IdEvento", e.IdEvento);
                cmd.Parameters.AddWithValue("@HoraInicio", e.HoraInicio);
                cmd.Parameters.AddWithValue("@HoraFin", e.HoraFin);

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        private void AgregarParams(SqlCommand cmd, Evento e)
        {
            cmd.Parameters.AddWithValue("@IdTipoEvento", e.IdTipoEvento);
            cmd.Parameters.AddWithValue("@NombreEvento", e.NombreEvento);
            cmd.Parameters.AddWithValue("@Descripcion", e.Descripcion);
            cmd.Parameters.AddWithValue("@Modalidad", e.Modalidad);
            cmd.Parameters.AddWithValue("@Lugar", e.Lugar);
            cmd.Parameters.AddWithValue("@CuposTotales", e.CuposTotales);
            cmd.Parameters.AddWithValue("@Activo", e.Activo);
            cmd.Parameters.AddWithValue("@HoraInicio", e.HoraInicio);
            cmd.Parameters.AddWithValue("@HoraFin", e.HoraFin);

            cmd.Parameters.AddWithValue("@FechaEvento",
                e.FechaEvento < new DateTime(1753, 1, 1) ? DateTime.Today : e.FechaEvento);

            cmd.Parameters.AddWithValue("@FechaInicioInscripcion",
                e.FechaInicioInscripcion < new DateTime(1753, 1, 1) ? DateTime.Today : e.FechaInicioInscripcion);

            cmd.Parameters.AddWithValue("@FechaFinInscripcion",
                e.FechaFinInscripcion < new DateTime(1753, 1, 1) ? DateTime.Today : e.FechaFinInscripcion);

            cmd.Parameters.AddWithValue("@FechaCreacion",
                e.FechaCreacion < new DateTime(1753, 1, 1) ? DateTime.Now : e.FechaCreacion);
        }
    }
}
