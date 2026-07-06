using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

namespace EventPlanner.Web.Data
{
    public class Conexion
    {
        private readonly string cadena;

        public Conexion()
        {
            cadena = ConfigurationManager
                .ConnectionStrings["EventPlanner.DB"]
                .ConnectionString;
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadena);
        }
    }
}