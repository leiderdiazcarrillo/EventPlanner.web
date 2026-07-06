using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace EventPlanner.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private static readonly string API_KEY =
            !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["GeminiApiKey"])
            ? System.Configuration.ConfigurationManager.AppSettings["GeminiApiKey"]
            : API_KEY;

        private static readonly string SYSTEM_PROMPT =
            "Eres el asistente virtual de EventPlanner, el sistema de gestion de eventos del " +
            "Centro para la Industria Petroquimica del SENA en Cartagena, Colombia. " +
            "Tu funcion es ayudar a administradores y aprendices con preguntas sobre: " +
            "1) Inscripcion a eventos (como inscribirse, cancelar, ver eventos disponibles). " +
            "2) Gestion de eventos (crear, editar, activar/desactivar, programar fechas de inscripcion). " +
            "3) Equipos (como se crean equipos, como unirse a un equipo en eventos grupales). " +
            "4) Reportes (como ver reportes de eventos e inscripciones). " +
            "5) Cuenta de usuario (registro, login, recuperar contrasena). " +
            "6) Reglas del sistema: validacion de correo institucional @soy.sena.edu.co, " +
            "   control de cupos, validacion de cruces de horario, roles ADMIN y APRENDIZ. " +
            "Responde siempre en espanol, de forma clara, breve y amable. " +
            "Si la pregunta no tiene relacion con EventPlanner o el SENA, indica amablemente " +
            "que solo puedes ayudar con temas del sistema.";

        [HttpPost]
        public JsonResult Responder(string pregunta)
        {
            if (string.IsNullOrWhiteSpace(pregunta))
                return Json(new { respuesta = "Por favor escribe tu pregunta." });

            if (string.IsNullOrEmpty(API_KEY))
                return Json(new { respuesta = "Error: Falta configurar la GeminiApiKey en el archivo Web.config." });

            try
            {
                var jsonObject = new
                {
                    model = "llama-3.1-8b-instant",
                    messages = new[]
                    {
                        new { role = "system", content = SYSTEM_PROMPT },
                        new { role = "user", content = pregunta }
                    }
                };

                string body = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsonObject);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(API_URL);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Headers["Authorization"] = "Bearer " + API_KEY;
                req.Timeout = 30000;

                byte[] data = Encoding.UTF8.GetBytes(body);
                req.ContentLength = data.Length;
                using (Stream s = req.GetRequestStream())
                    s.Write(data, 0, data.Length);

                string raw;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    raw = sr.ReadToEnd();

                string text = "";
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                var result = serializer.Deserialize<dynamic>(raw);

                try
                {
                    var choices = result["choices"] as IList<object>;
                    if (choices != null && choices.Count > 0)
                    {
                        var message = (choices[0] as Dictionary<string, object>)["message"]
                                      as Dictionary<string, object>;
                        text = message["content"].ToString();
                    }
                }
                catch
                {
                    text = "";
                }

                if (string.IsNullOrWhiteSpace(text))
                    text = "Hola, recibi tu mensaje pero no pude procesar la respuesta. Intenta de nuevo.";

                return Json(new { respuesta = text });
            }
            catch (WebException webEx) when (webEx.Response != null)
            {
                using (var sr = new StreamReader(webEx.Response.GetResponseStream()))
                {
                    string jsonError = sr.ReadToEnd();
                    return Json(new { respuesta = "Error del servidor: " + jsonError });
                }
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = "Error al conectar: " + ex.Message });
            }
        }
    }
}
