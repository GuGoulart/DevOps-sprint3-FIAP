using Microsoft.AspNetCore.Mvc;

namespace MottuProjeto.Infrastructure
{
    /// <summary>Helpers para montar objetos com links HATEOAS.</summary>
    public static class HateoasExtensions
    {
        /// <summary>
        /// Envelopa um recurso com links básicos: self, update e delete.
        /// </summary>
        public static object WithLinks<T>(
            this ControllerBase ctrl,
            T data,
            string getAction,
            string updateAction,
            string deleteAction,
            object routeValues)
        {
            return new
            {
                data,
                _links = new
                {
                    self = ctrl.Url.Action(getAction, routeValues),
                    update = ctrl.Url.Action(updateAction, routeValues),
                    delete = ctrl.Url.Action(deleteAction, routeValues)
                }
            };
        }

        /// <summary>Cria um link (href) para uma action específica.</summary>
        public static string? LinkTo(this ControllerBase ctrl, string action, object? values = null)
            => ctrl.Url.Action(action, values);
    }
}
