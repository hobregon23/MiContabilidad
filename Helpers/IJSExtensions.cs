using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace MiContabilidad.Helpers
{
    public static class IJSExtensions
    {
        public static Task MostrarMensaje(this IJSRuntime js, string mensaje)
        {
            SweetAlertService Swal = new SweetAlertService(js);
            return Swal.FireAsync(mensaje);
        }
        public static Task MostrarMensaje(this IJSRuntime js, string titulo, string mensaje, TipoMensajeSweetAlert tipo)
        {
            SweetAlertService Swal = new SweetAlertService(js);
            return Swal.FireAsync(titulo, mensaje, Convert.ToString(tipo));
        }

        public async static Task<bool> ConfirmSinResp(this IJSRuntime js, string titulo, string mensaje)
        {
            SweetAlertService Swal = new SweetAlertService(js);
            SweetAlertResult result = await Swal.FireAsync(new SweetAlertOptions
            {
                Title = titulo,
                Text = mensaje,
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true,
                ConfirmButtonText = "Confirmar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                return true;
            }
            return false;
        }

        public async static Task<bool> Confirm(this IJSRuntime js, string titulo, string mensaje)
        {
            SweetAlertService Swal = new SweetAlertService(js);
            SweetAlertResult result = await Swal.FireAsync(new SweetAlertOptions
            {
                Title = titulo,
                Text = mensaje,
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true,
                ConfirmButtonText = "Confirmar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await Swal.FireAsync(
                    titulo,
                    "Se ha realizado exitosamente.",
                    SweetAlertIcon.Success
                    );
                return true;
            }
            //else if (result.Dismiss == DismissReason.Cancel)
            //{
            //    await Swal.FireAsync(
            //        "Cancelado",
            //        "Todo sigue igual :)",
            //        SweetAlertIcon.Info
            //        );
            //}
            return false;
        }

        public enum TipoMensajeSweetAlert
        {
            question, warning, error, success, info
        }

        public static ValueTask<object> SetInLocalStorage(this IJSRuntime js, string key, string content)
            => js.InvokeAsync<object>(
                "localStorage.setItem",
                key, content
                );

        public static ValueTask<string> GetFromLocalStorage(this IJSRuntime js, string key)
            => js.InvokeAsync<string>(
                "localStorage.getItem",
                key
                );

        public static ValueTask<object> RemoveItem(this IJSRuntime js, string key)
            => js.InvokeAsync<object>(
                "localStorage.removeItem",
                key);
    }
}
