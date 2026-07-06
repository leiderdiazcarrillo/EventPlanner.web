using EventPlanner.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace EventPlanner.Web.Data
{
    public class ProgramaFormacionDAO
    {

        public List<ProgramaFormacion> Listar()
        {
            List<ProgramaFormacion> lista = new List<ProgramaFormacion>();

            Conexion conexion = new Conexion();

            using (SqlConnection cn = conexion.ObtenerConexion())
            {
                cn.Open();

                string sql = @"SELECT * FROM ProgramaFormacion
                WHERE NombrePrograma <> 'ADMINISTRACION SISTEMA'";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ProgramaFormacion
                            {
                                IdProgramaFormacion = (int)dr["IdProgramaFormacion"],
                                NombrePrograma = dr["NombrePrograma"].ToString()
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}