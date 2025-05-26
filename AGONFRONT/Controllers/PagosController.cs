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
            Pedidos pedido = new Pedidos();
            Pagos pago = new Pagos();
            List<Pedidos> pedidoget = new List<Pedidos>();
            List<Productos> productos = new List<Productos>();
            List<Usuarios> usuario = new List<Usuarios>();

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        TempData["Error"] = $"Campo: {key} - Error: {error.ErrorMessage}";
                    }
                }
                return RedirectToAction("Iniciar", "Home");
            }

            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No hay token de sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var userId = GetLoggedInUserId(token);

                // Obtener productos y usuarios
                var productosResp = await client.GetAsync("api/Productos/GetProductos");
                var usuariosResp = await client.GetAsync("api/Usuarios/GetUsuarios");

                productos = JsonConvert.DeserializeObject<List<Productos>>(await productosResp.Content.ReadAsStringAsync()) ?? new List<Productos>();
                usuario = JsonConvert.DeserializeObject<List<Usuarios>>(await usuariosResp.Content.ReadAsStringAsync()) ?? new List<Usuarios>();


                // Preparar el pedido
                pedido.ClienteId = int.Parse(userId);
                pedido.Estado = "pendiente_pago";


                foreach (var item in model)
                {
                    var producto = productos.FirstOrDefault(p => p.Id == item.ProductoId);
                    if (producto != null)
                    {
                        pedido.Total += producto.Precio * item.Cantidad;
                        pedido.ProductoId = item.ProductoId;
                        pedido.VendedorId = producto.VendedorId;
                        pedido.Cantidad = item.Cantidad;
                        pedido.PrecioUnitario = producto.Precio;
                        pedido.MetodoPago = "Epayco";
                        // Aquí puedes construir una lista de detalles del pedido si tienes una entidad tipo DetallePedido
                        // pedido.Detalles.Add(new DetallePedido { ProductoId = producto.Id, Cantidad = item.Cantidad, PrecioUnitario = producto.Precio });
                    }
                }

                string json = JsonConvert.SerializeObject(pedido);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/Pedidos/PostPedidos", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Pedido Creado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Fallo al crear pedido.";
                }

                // Redirigir a pasarela de pago
                string factura = $"{userId}AGON{DateTime.Now.Ticks}";
                string nombre = usuario.FirstOrDefault(u => u.Id == pedido.ClienteId)?.Nombre ?? "Cliente";
                string email = usuario.FirstOrDefault(u => u.Id == pedido.ClienteId)?.Correo ?? "cliente@email.com";

                //Post Pagos
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var pedidoCreado = JsonConvert.DeserializeObject<Pedidos>(jsonResponse);

                    pago.PedidoId = pedidoCreado.Id;
                }
                pago.Monto = pedido.Total;
                pago.Factura = factura;
                pago.EstadoTransaccion = pedido.Estado;

                string jsonpago = JsonConvert.SerializeObject(pago);
                var pagocontent = new StringContent(jsonpago, Encoding.UTF8, "application/json");

                HttpResponseMessage pagorespuesta = await client.PostAsync("api/Pagos/PostPagos", pagocontent);
                if (pagorespuesta.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Pedido Creado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Fallo al crear pedido.";
                }

                // Aquí podrías hacer un POST del pedido si ya tienes el API que lo reciba


                string urlRespuesta = Url.Action("Iniciar", "Home");
                string urlConfirmacion = Url.Action("Iniciar", "Home");
                TempData["SuccessMessage"] = "¡Compra exitosamente!";

                return RedirectToAction("Productos", "Productos");
            }
        }


        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }
    }

}
