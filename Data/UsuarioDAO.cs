using EventPlanner.Web.Helpers;
using EventPlanner.Web.Models;
using System;
using System.Data.SqlClient;

namespace EventPlanner.Web.Data
{
    public class UsuarioDAO
    {
        private readonly Conexion _cn = new Conexion();

        public bool Registrar(Usuario usuario)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                string sql = @"INSERT INTO Usuario
                    (IdProgramaFormacion,TipoDocumento,NumeroDocumento,NombreCompleto,
                     Correo,Ficha,Jornada,Rol,FechaRegistro,Contrasena)
                    VALUES
                    (@IdProgramaFormacion,@TipoDocumento,@NumeroDocumento,@NombreCompleto,
                     @Correo,@Ficha,@Jornada,@Rol,@FechaRegistro,@Contrasena)";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@IdProgramaFormacion", usuario.IdProgramaFormacion);
                    cmd.Parameters.AddWithValue("@TipoDocumento",       usuario.TipoDocumento);
                    cmd.Parameters.AddWithValue("@NumeroDocumento",     usuario.NumeroDocumento);
                    cmd.Parameters.AddWithValue("@NombreCompleto",      usuario.NombreCompleto);
                    cmd.Parameters.AddWithValue("@Correo",              usuario.Correo);
                    cmd.Parameters.AddWithValue("@Ficha",               usuario.Ficha);
                    cmd.Parameters.AddWithValue("@Jornada",             usuario.Jornada);
                    cmd.Parameters.AddWithValue("@Rol",                 usuario.Rol);
                    cmd.Parameters.AddWithValue("@FechaRegistro",       usuario.FechaRegistro);
                    cmd.Parameters.AddWithValue("@Contrasena",
                        EncryptionHelper.HashPassword(usuario.Contrasena));
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        public Usuario Login(string correo, string contrasena)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM Usuario WHERE Correo = @Correo", cn))
                {
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string hash = dr["Contrasena"].ToString();
                            if (EncryptionHelper.VerifyPassword(contrasena, hash))
                            {
                                return new Usuario
                                {
                                    IdUsuario      = Convert.ToInt32(dr["IdUsuario"]),
                                    NombreCompleto = dr["NombreCompleto"].ToString(),
                                    Correo         = dr["Correo"].ToString(),
                                    Rol            = dr["Rol"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }

        public bool ExisteCorreo(string correo)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuario WHERE Correo=@c", cn))
                {
                    cmd.Parameters.AddWithValue("@c", correo);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public bool ExisteDocumento(string numeroDocumento)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuario WHERE NumeroDocumento=@d", cn))
                {
                    cmd.Parameters.AddWithValue("@d", numeroDocumento);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // ── Recuperar contrasena: buscar usuario por correo y documento ──
        public Usuario BuscarParaRecuperar(string correo, string numeroDocumento)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM Usuario WHERE Correo=@c AND NumeroDocumento=@d", cn))
                {
                    cmd.Parameters.AddWithValue("@c", correo);
                    cmd.Parameters.AddWithValue("@d", numeroDocumento);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return new Usuario
                            {
                                IdUsuario      = Convert.ToInt32(dr["IdUsuario"]),
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                Correo         = dr["Correo"].ToString(),
                                Contrasena     = dr["Contrasena"].ToString(),
                                Rol            = dr["Rol"].ToString()
                            };
                    }
                }
            }
            return null;
        }

        // ── Cambiar contrasena ───────────────────────────────────────────
        public bool CambiarContrasena(int IdUsuario, string nuevaContrasena)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuario SET Contrasena=@c WHERE IdUsuario=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@c",
                        EncryptionHelper.HashPassword(nuevaContrasena));
                    cmd.Parameters.AddWithValue("@id", IdUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ── Obtener usuario por ID ───────────────────────────────────────
        public Usuario ObtenerPorId(int IdUsuario)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM Usuario WHERE IdUsuario=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", IdUsuario);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int fotOrd = dr.GetOrdinal("FotoPerfil");
                            return new Usuario
                            {
                                IdUsuario      = Convert.ToInt32(dr["IdUsuario"]),
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                Correo         = dr["Correo"].ToString(),
                                Rol            = dr["Rol"].ToString(),
                                Contrasena     = dr["Contrasena"].ToString(),
                                FotoPerfil     = dr.IsDBNull(fotOrd) ? null : dr["FotoPerfil"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // ── Verificar contrasena actual ──────────────────────────────────
        public bool VerificarContrasena(int IdUsuario, string contrasenaIngresada)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Contrasena FROM Usuario WHERE IdUsuario=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", IdUsuario);
                    object result = cmd.ExecuteScalar();
                    if (result == null) return false;
                    return EncryptionHelper.VerifyPassword(contrasenaIngresada, result.ToString());
                }
            }
        }

        // ── Actualizar nombre (ambos roles) ─────────────────────────────
        public bool ActualizarNombre(int IdUsuario, string nuevoNombre)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuario SET NombreCompleto=@n WHERE IdUsuario=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@n", nuevoNombre);
                    cmd.Parameters.AddWithValue("@id", IdUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ── Actualizar foto de perfil ────────────────────────────────────
        public bool ActualizarFoto(int IdUsuario, string rutaFoto)
        {
            using (SqlConnection cn = _cn.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuario SET FotoPerfil=@f WHERE IdUsuario=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@f", (object)rutaFoto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", IdUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
