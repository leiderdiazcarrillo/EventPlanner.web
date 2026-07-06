using EventPlanner.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EventPlanner.Web.Data
{
    public class EquipoDAO
    {
        public List<Equipo> Listar()
        {
            List<Equipo> lista = new List<Equipo>();
            Conexion conexion = new Conexion();

            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT E.*, EV.NombreEvento
                               FROM Equipo E
                               LEFT JOIN Evento EV ON E.IdEvento = EV.IdEvento
                               ORDER BY E.NombreEquipo";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Equipo e = new Equipo
                            {
                                IdEquipo = Convert.ToInt32(dr["IdEquipo"]),
                                IdEvento = dr["IdEvento"] != DBNull.Value ? Convert.ToInt32(dr["IdEvento"]) : 0,
                                NombreEquipo = dr["NombreEquipo"].ToString(),
                                CantidadMinima = dr["CantidadMinima"] != DBNull.Value ? Convert.ToInt32(dr["CantidadMinima"]) : 0,
                                CantidadMaxima = dr["CantidadMaxima"] != DBNull.Value ? Convert.ToInt32(dr["CantidadMaxima"]) : 0,
                                NombreEvento = dr["NombreEvento"] != DBNull.Value ? dr["NombreEvento"].ToString() : string.Empty
                            };
                            lista.Add(e);
                        }
                    }
                }
            }

            return lista;
        }

        public List<Equipo> ListarPorEvento(int IdEvento)
        {
            List<Equipo> lista = new List<Equipo>();
            Conexion conexion = new Conexion();

            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();
                string sql = @"SELECT E.*, EV.NombreEvento
                               FROM Equipo E
                               LEFT JOIN Evento EV ON E.IdEvento = EV.IdEvento
                               WHERE E.IdEvento = @IdEvento
                               ORDER BY E.NombreEquipo";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdEvento", IdEvento);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Equipo e = new Equipo
                            {
                                IdEquipo = Convert.ToInt32(dr["IdEquipo"]),
                                IdEvento = dr["IdEvento"] != DBNull.Value ? Convert.ToInt32(dr["IdEvento"]) : 0,
                                NombreEquipo = dr["NombreEquipo"].ToString(),
                                CantidadMinima = dr["CantidadMinima"] != DBNull.Value ? Convert.ToInt32(dr["CantidadMinima"]) : 0,
                                CantidadMaxima = dr["CantidadMaxima"] != DBNull.Value ? Convert.ToInt32(dr["CantidadMaxima"]) : 0,
                                NombreEvento = dr["NombreEvento"] != DBNull.Value ? dr["NombreEvento"].ToString() : string.Empty
                            };
                            lista.Add(e);
                        }
                    }
                }
            }

            return lista;
        }

        public bool Registrar(Equipo modelo)
        {
            Conexion conexion = new Conexion();
            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();
                string sql = @"INSERT INTO Equipo (IdEvento, NombreEquipo, CantidadMinima, CantidadMaxima)
                               VALUES (@IdEvento, @NombreEquipo, @CantidadMinima, @CantidadMaxima)";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdEvento", modelo.IdEvento);
                    cmd.Parameters.AddWithValue("@NombreEquipo", modelo.NombreEquipo);
                    cmd.Parameters.AddWithValue("@CantidadMinima", modelo.CantidadMinima);
                    cmd.Parameters.AddWithValue("@CantidadMaxima", modelo.CantidadMaxima);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public Equipo ObtenerPorId(int id)
        {
            Equipo item = null;
            Conexion conexion = new Conexion();
            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();
                string sql = "SELECT * FROM Equipo WHERE IdEquipo = @IdEquipo";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdEquipo", id);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            item = new Equipo
                            {
                                IdEquipo = Convert.ToInt32(dr["IdEquipo"]),
                                IdEvento = dr["IdEvento"] != DBNull.Value ? Convert.ToInt32(dr["IdEvento"]) : 0,
                                NombreEquipo = dr["NombreEquipo"].ToString(),
                                CantidadMinima = dr["CantidadMinima"] != DBNull.Value ? Convert.ToInt32(dr["CantidadMinima"]) : 0,
                                CantidadMaxima = dr["CantidadMaxima"] != DBNull.Value ? Convert.ToInt32(dr["CantidadMaxima"]) : 0
                            };
                        }
                    }
                }
            }
            return item;
        }

        public bool Editar(Equipo modelo)
        {
            Conexion conexion = new Conexion();
            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();
                string sql = @"UPDATE Equipo SET IdEvento=@IdEvento, NombreEquipo=@NombreEquipo, CantidadMinima=@CantidadMinima, CantidadMaxima=@CantidadMaxima
                               WHERE IdEquipo=@IdEquipo";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdEvento", modelo.IdEvento);
                    cmd.Parameters.AddWithValue("@NombreEquipo", modelo.NombreEquipo);
                    cmd.Parameters.AddWithValue("@CantidadMinima", modelo.CantidadMinima);
                    cmd.Parameters.AddWithValue("@CantidadMaxima", modelo.CantidadMaxima);
                    cmd.Parameters.AddWithValue("@IdEquipo", modelo.IdEquipo);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Eliminar(int id)
        {
            Conexion conexion = new Conexion();
            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();

                string sqlDesvincular = "UPDATE Inscripcion SET IdEquipo = NULL WHERE IdEquipo = @IdEquipo";
                using (SqlCommand cmdUpdate = new SqlCommand(sqlDesvincular, cn))
                {
                    cmdUpdate.Parameters.AddWithValue("@IdEquipo", id);
                    cmdUpdate.ExecuteNonQuery();
                }

                string sqlDelete = "DELETE FROM Equipo WHERE IdEquipo = @IdEquipo";
                using (SqlCommand cmdDelete = new SqlCommand(sqlDelete, cn))
                {
                    cmdDelete.Parameters.AddWithValue("@IdEquipo", id);
                    return cmdDelete.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
