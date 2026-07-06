using System;
using System.Security.Cryptography;
using System.Text;

namespace EventPlanner.Web.Helpers
{
    /// <summary>
    /// Helper de encriptacion. Soporta verificar contrasenas en texto plano
    /// (datos semilla de la BD) y contrasenas PBKDF2 (nuevos registros).
    /// </summary>
    public class EncryptionHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contrasena no puede estar vacia");

            using (var rfc2898 = new Rfc2898DeriveBytes(password, 16, 10000))
            {
                byte[] hash = rfc2898.GetBytes(20);
                byte[] salt = rfc2898.Salt;
                byte[] hashWithSalt = new byte[36];
                Array.Copy(salt, 0, hashWithSalt, 0, 16);
                Array.Copy(hash, 0, hashWithSalt, 16, 20);
                return Convert.ToBase64String(hashWithSalt);
            }
        }

        public static bool VerifyPassword(string password, string hashGuardado)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashGuardado))
                return false;

            // --- Soporte para contrasenas en texto plano (datos semilla de la BD) ---
            // Si el valor guardado NO es un Base64 valido de 36 bytes, comparar directo
            try
            {
                byte[] decoded = Convert.FromBase64String(hashGuardado);
                if (decoded.Length != 36)
                {
                    // No es un hash PBKDF2, comparar como texto plano
                    return string.Equals(password, hashGuardado, StringComparison.Ordinal);
                }
            }
            catch
            {
                // No es Base64 en absoluto -> texto plano
                return string.Equals(password, hashGuardado, StringComparison.Ordinal);
            }

            // --- Verificacion PBKDF2 normal ---
            try
            {
                byte[] hashWithSalt = Convert.FromBase64String(hashGuardado);
                byte[] salt = new byte[16];
                Array.Copy(hashWithSalt, 0, salt, 0, 16);

                using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000))
                {
                    byte[] hash2 = rfc2898.GetBytes(20);
                    for (int i = 0; i < 20; i++)
                        if (hashWithSalt[i + 16] != hash2[i]) return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
