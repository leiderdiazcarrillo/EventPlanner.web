using System;
using System.Collections.Generic;
using EventPlanner.Web.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace EventPlanner.Web.Data
{
    public class TipoEventoDAO
    {
        public List<TipoEvento> Listar()
        {
            List<TipoEvento> lista = new List<TipoEvento>();
            Conexion conexion = new Conexion();

            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();
                string sql = "SELECT IdTipoEvento, NombreTipoEvento FROM TipoEvento ORDER BY NombreTipoEvento";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            TipoEvento t = new TipoEvento
                            {
                                IdTipoEvento = Convert.ToInt32(dr["IdTipoEvento"]),
                                NombreTipoEvento = dr["NombreTipoEvento"].ToString()
                            };
                            lista.Add(t);
                        }
                    }
                }
            }

            return lista;
        }
    }
}
