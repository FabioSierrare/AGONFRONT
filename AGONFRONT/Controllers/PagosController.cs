using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using AGONFRONT.Models;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace AGONFRONT.Controllers
{
    public class PagosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();

        public async Task<ActionResult> Redirigir(List<ProductoEnCarrito> model)
        {
            // Instanciamos objetos para pedido, pago y listas para almacenar datos
            Pedidos pedido = new Pedidos();
            Pagos pago = new Pagos();
            List<Pedidos> pedidoget = new List<Pedidos>(); // No se usa en el código actual
            List<Productos> productos = new List<Productos>();
            List<Usuarios> usuario = new List<Usuarios>();

            // Validar si el modelo recibido es válido
            if (!ModelState.IsValid)
            {
                // Si no es válido, recorremos los errores y los almacenamos en TempData para mostrar en la vista
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        TempData["Error"] = $"Campo: {key} - Error: {error.ErrorMessage}";
                    }
                }
                // Redirigimos al usuario a la acción Iniciar del controlador Home
                return RedirectToAction("Iniciar", "Home");
            }

            // Obtenemos el token de autenticación desde cookies o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // Validamos que el token no sea nulo o vacío
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No hay token de sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Crear un cliente HTTP para consumir la API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl); // Asignamos la URL base
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Autenticamos la solicitud con el token

                // Obtener el Id del usuario logueado desde el token (suponiendo que existe el método GetLoggedInUserId)
                var userId = GetLoggedInUserId(token);

                // Obtener lista de productos desde la API
                var productosResp = await client.GetAsync("api/Productos/GetProductos");
                // Obtener lista de usuarios desde la API
                var usuariosResp = await client.GetAsync("api/Usuarios/GetUsuarios");

                // Deserializar las respuestas JSON a listas de objetos correspondientes
                productos = JsonConvert.DeserializeObject<List<Productos>>(await productosResp.Content.ReadAsStringAsync()) ?? new List<Productos>();
                usuario = JsonConvert.DeserializeObject<List<Usuarios>>(await usuariosResp.Content.ReadAsStringAsync()) ?? new List<Usuarios>();

                // Preparar el objeto pedido con los datos básicos
                pedido.ClienteId = int.Parse(userId);
                pedido.Estado = "pendiente_pago";

                // Recorrer cada producto en el carrito recibido (model)
                foreach (var item in model)
                {
                    // Buscar el producto en la lista obtenida de la API según el Id
                    var producto = productos.FirstOrDefault(p => p.Id == item.ProductoId);
                    if (producto != null)
                    {
                        // Calcular el total sumando el precio por la cantidad
                        pedido.Total += producto.Precio * item.Cantidad;
                        // Asignar otros datos relevantes del pedido (último producto sobreescribe estos valores)
                        pedido.ProductoId = item.ProductoId;
                        pedido.VendedorId = producto.VendedorId;
                        pedido.Cantidad = item.Cantidad;
                        pedido.PrecioUnitario = producto.Precio;
                        pedido.MetodoPago = "Epayco";
                        // Comentado: aquí podrías construir detalles del pedido si tienes la entidad DetallePedido
                    }
                }

                // Serializar el pedido a JSON para enviarlo en la solicitud POST
                string json = JsonConvert.SerializeObject(pedido);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Enviar el pedido a la API
                HttpResponseMessage response = await client.PostAsync("api/Pedidos/PostPedidos", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Pedido Creado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Fallo al crear pedido.";
                }

                // Preparar datos para la pasarela de pago
                string factura = $"{userId}AGON{DateTime.Now.Ticks}";
                string nombre = usuario.FirstOrDefault(u => u.Id == pedido.ClienteId)?.Nombre ?? "Cliente";
                string email = usuario.FirstOrDefault(u => u.Id == pedido.ClienteId)?.Correo ?? "cliente@email.com";

                // Si el pedido fue creado exitosamente, obtenemos el Id del pedido creado desde la respuesta
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var pedidoCreado = JsonConvert.DeserializeObject<Pedidos>(jsonResponse);

                    pago.PedidoId = pedidoCreado.Id;
                }
                // Asignar datos para el pago
                pago.Monto = pedido.Total;
                pago.Factura = factura;
                pago.EstadoTransaccion = pedido.Estado;

                // Serializar el pago a JSON
                string jsonpago = JsonConvert.SerializeObject(pago);
                var pagocontent = new StringContent(jsonpago, Encoding.UTF8, "application/json");

                // Enviar el pago a la API
                HttpResponseMessage pagorespuesta = await client.PostAsync("api/Pagos/PostPagos", pagocontent);
                if (pagorespuesta.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Pedido Creado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Fallo al crear pedido.";
                }

                // URLs de redirección (no se usan en esta versión, pero podrían usarse para pagos o confirmaciones)
                string urlRespuesta = Url.Action("Iniciar", "Home");
                string urlConfirmacion = Url.Action("Iniciar", "Home");
                TempData["SuccessMessage"] = "¡Compra exitosamente!";

                // Finalmente, redirigimos a la vista de productos
                return RedirectToAction("Productos", "Productos");
            }
        }



        // Método para obtener el ID del usuario desde el token JWT
        /// <summary>
        /// Extrae el ID del usuario autenticado desde el token JWT.
        /// </summary>
        /// <param name="token">Token JWT del cual se extraerá el ID del usuario.</param>
        /// <returns>El ID del usuario si se encuentra en el token, o null si no existe.</returns>
        public string GetLoggedInUserId(string token)
        {
            // 🛡️ Crear una instancia para manejar el token JWT
            var handler = new JwtSecurityTokenHandler();

            // 📦 Leer y deserializar el token JWT
            var jwtToken = handler.ReadJwtToken(token);

            // 🔍 Buscar el claim que contiene el ID del usuario (por convención es NameIdentifier)
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            // 📤 Devolver el valor del claim (ID del usuario) o null si no se encontró
            return userIdClaim?.Value;
        }
    }

}
