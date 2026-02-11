using Microsoft.JSInterop;

namespace Store.Web
{
    public static class Notificaciones
    {
        // Método para mostrar un mensaje tipo Toast(flotante y temporal)
        // Recibe el motor de JS, el título, el mensaje y el tipo de icono (por defecto error)
        public static async Task MostrarToast(IJSRuntime js, string titulo, string mensaje, string icono = "error")
        {
            // Usamos InvokeVoidAsync porque no necesitamos que JavaScript nos devuelva ningún valor
            // Usamos 'eval' para ejecutar código JS directamente desde C#
            await js.InvokeVoidAsync("eval", $@"
            // Verificamos si ya hemos inyectado el estilo CSS de las animaciones
            if (!document.getElementById('sweetalert-custom-fade')) {{
                const style = document.createElement('style');
                style.id = 'sweetalert-custom-fade';
                // Definimos animaciones de opacidad pura (Fade In/Out)
                style.innerHTML = `
                    .my-fade-in {{ animation: fadeIn 0.6s !important; }}
                    .my-fade-out {{ animation: fadeOut 0.6s !important; }}
                    @keyframes fadeIn {{ from {{ opacity: 0; }} to {{ opacity: 1; }} }}
                    @keyframes fadeOut {{ from {{ opacity: 1; }} to {{ opacity: 0; }} }}
                `;
                document.head.appendChild(style);
            }}

            // Ejecutamos la librería SweetAlert2 con configuración de Toast
            Swal.fire({{
                toast: true,
                position: 'center', // Aparecerá en el centro de la pantalla
                icon: '{icono}',
                title: '{titulo}',
                text: '{mensaje}',
                showConfirmButton: false, // Sin botones para que sea menos intrusivo
                timer: 3000, // Se cierra solo a los 3 segundos
                timerProgressBar: true, // Barra visual de tiempo restante
                showClass: {{ popup: 'my-fade-in' }}, // Animación de entrada personalizada
                hideClass: {{ popup: 'my-fade-out' }} // Animación de salida personalizada
            }});
        ");
        }
    }
}
