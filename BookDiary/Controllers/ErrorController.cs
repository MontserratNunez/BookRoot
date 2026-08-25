using Microsoft.AspNetCore.Mvc;

namespace BookDiary.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            ViewBag.Code = statusCode;

            switch (statusCode)
            {
                case 404:
                    return View("NotFound");

                case 500:
                    ViewBag.Title = "Error interno del servidor";
                    ViewBag.Text = "Ocurrió un error inesperado. Inténtalo más tarde.";
                    break;

                case 403:
                    ViewBag.Title = "Acceso denegado";
                    ViewBag.Text = "No tienes permisos para acceder a este recurso.";
                    break;

                case 401:
                    ViewBag.Title = "No autorizado";
                    ViewBag.Text = "Debes iniciar sesión para acceder.";
                    break;

                default:
                    ViewBag.Title = "Error";
                    ViewBag.Text = "Ocurrió un error inesperado.";
                    break;
            }

            return View("Error");
        }
    }
}
