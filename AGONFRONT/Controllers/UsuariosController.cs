using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AGONFRONT.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using static AGONFRONT.Controllers.HomeController;
using System.Globalization;
using AGONFRONT.Utils;
using AGONFRONT.Filters;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AGONFRONT.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly string apiUrl = ConfigurationManager.AppSettings["Api"].ToString();
        // GET: X
        public ActionResult Register()
        {

            return View();
        }

        public ActionResult RegistroCliente()
        {
            return View();
        }
        public ActionResult RegistroVendedor()
        {
            return View();
        }



        // GET: X/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Usuarios model)
        {
            try
            {
                // 🔒 Encriptar la contraseña antes de enviarla
                model.Contraseña = Encriptador.Encriptar(model.Contraseña);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Usuarios/PostUsuarios", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        // Puedes manejar la respuesta aquí si lo necesitas
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }
        }

        //Estos controladortes son de la vista de mi producto vendedor
        //-------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> Misproductosvendedor()
        {
            List<Productos> productos = new List<Productos>();

            // Verificar si el token está en las cookies o en la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            string token = tokenCookie?.Value ?? tokenSession;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener los datos de envíos.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res) ?? new List<Productos>();

                    // Obtener el ID del usuario desde el token
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Filtrar envíos que pertenecen al usuario autenticado
                    productos = productos.Where(p => p.VendedorId == int.Parse(userId)).ToList();

                    if (!productos.Any())
                    {
                        TempData["Error"] = "No se encontraron envíos para el usuario logueado.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(productos);
        }
        [AuthorizeByRole("Vendedor")]
        [HttpPost]
        public async Task<ActionResult> Editarmiproducto(Productos productos)
        {
            // Obtener el token de la cookie o la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar si el token está vacío
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(productos), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync($"api/Productos/PutProductos/{productos.Id}", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Producto actualizado correctamente.";
                    }
                    else
                    {
                        // Obtener el error detallado que devuelve la API
                        string errorMessage = await response.Content.ReadAsStringAsync();

                        // Mostrar el mensaje de error en TempData
                        TempData["Error"] = $"❌ Error al actualizar controlador:\n{errorMessage}";

                        // Imprimirlo en consola para depuración
                        Console.WriteLine($"[API ERROR]: {errorMessage}");

                        return RedirectToAction("Misproductosvendedor");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"⚠️ Excepción no controlada:\n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Misproductosvendedor");
            }

            return RedirectToAction("Misproductosvendedor");
        }

        [AuthorizeByRole("Vendedor")]
        [HttpPost]
        public async Task<ActionResult> EliminarMiProducto(int id)
        {
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Capturar detalles del error
                    Console.WriteLine($"[REQUEST] DELETE api/Productos/DeleteProductos/{id}");
                    Console.WriteLine($"[IN] Entró al método EliminarMiProducto con ID: {id}");


                    HttpResponseMessage response = await client.DeleteAsync($"api/Productos/DeleteProductos/{id}");

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[RESPONSE] Status: {response.StatusCode} | Content-Type: {response.Content.Headers.ContentType}");
                    Console.WriteLine($"[RESPONSE BODY] {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Producto eliminado correctamente.";
                    }
                    else
                    {
                        TempData["Error"] = $"❌ Error al eliminar el producto: {responseContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] {ex.Message}");
                TempData["Error"] = $"Ocurrió un error: {ex.Message}";
            }

            return RedirectToAction("Misproductosvendedor");
        }

        //--------------------------------------------------------------------------------------------------

        // Método para obtener el ID del usuario desde el token JWT
        public string GetLoggedInUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }
        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> UpdatePerfilVendedor()
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            // Verificar si el token está en las cookies
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;

            // Depuración para ver si las cookies están presentes
            Console.WriteLine("BearerToken Cookie en Request: " + tokenCookie?.Value);
            Console.WriteLine("TokenExpirationTime Cookie en Request: " + tokenExpirationCookie?.Value);

            // Si el token está ausente, redirigir al login
            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home"); // Redirige al login
            }

            // Si el token está presente en las cookies, continuar
            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar la fecha de expiración de la cookie
            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate) ? expiryDate : (DateTime?)null;
            }

            // Si la fecha de expiración ha pasado, eliminar la cookie y redirigir
            if (expirationTime.HasValue && DateTime.Now > expirationTime.Value)
            {
                HttpContext.Response.Cookies.Remove("BearerToken");
                HttpContext.Response.Cookies.Remove("TokenExpirationTime");

                TempData["Error"] = "La sesión ha expirado. Por favor inicia sesión nuevamente.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);

                HttpResponseMessage response = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los datos del usuario.";
                }
            }

            return View(usuarios);
        }

        [HttpPost]
       
        public async Task<ActionResult> UpdateUsuario(Usuarios usuario)
        {
            // Obtener el token de la cookie o la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            // Corregir la asignación del token
            string token = tokenCookie?.Value ?? tokenSession;

            // Verificar si el token está vacío
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync($"api/Usuarios/PutUsuarios/{usuario.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Usuario actualizado correctamente.";
                }
                else
                {
                    // Leer el contenido de la respuesta para ver los detalles del error
                    string errorMessage = await response.Content.ReadAsStringAsync();

                    // Guardar el mensaje detallado en TempData para mostrarlo en la vista
                    TempData["Error"] = $"Error al actualizar usuario: {errorMessage}";

                    // También puedes imprimirlo en la consola para depuración
                    Console.WriteLine($"Error en API: {errorMessage}");

                    // Regresar a la vista sin redirigir para mostrar los errores
                    return RedirectToAction("UpdatePerfilVendedor");
                }
            }

            return RedirectToAction("UpdatePerfilVendedor");
        }

        //Aca estan todos los controladores de gestionar productos
        //--------------------------------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> GestionarProductos()
        {
            List<Productos> productos = new List<Productos>();

            var tokenCookie = Request.Cookies["BearerToken"]?.Value; // ✅ Extraer el valor del cookie
            var tokenSession = Session["BearerToken"] as string;

            string token = tokenCookie ?? tokenSession;

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener los datos de envíos.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res) ?? new List<Productos>();

                    // ✅ Obtener el ID del usuario desde el token
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    ViewBag.UsuarioId = userId; // ✅ Pasar ID del usuario a la vista

                    // Filtrar productos del usuario autenticado
                    productos = productos.Where(p => p.VendedorId == int.Parse(userId)).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(productos);
        }
        private async Task<string> SubirImagenAzure(HttpPostedFileBase imagen)
        {
            string storageConnectionString = ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ConnectionString;
            string containerName = "imagenes-productos"; // tu contenedor

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            string nombreArchivo = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imagen.FileName);
            var blockBlob = container.GetBlockBlobReference(nombreArchivo);

            blockBlob.Properties.ContentType = imagen.ContentType;
            await blockBlob.UploadFromStreamAsync(imagen.InputStream);

            return blockBlob.Uri.AbsoluteUri;
        }

        [AuthorizeByRole("Vendedor")]
        [HttpPost]
        public async Task<ActionResult> AgregarProducto(Productos model, HttpPostedFileBase Imagen)
        {
            try
            {
                if (Imagen != null && Imagen.ContentLength > 0)
                {
                    // Subir imagen a Azure y obtener URL
                    var urlImagen = await SubirImagenAzure(Imagen);

                    // Guardar la URL en el producto
                    model.UrlImagen = urlImagen;
                }
                else
                {
                    TempData["Error"] = "Debes subir una imagen para el producto.";
                    return RedirectToAction("GestionarProductos");
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Productos/PostProductos", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("GestionarProductos");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
            }

            return RedirectToAction("GestionarProductos");
        }

        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                var response = await client.DeleteAsync($"api/Productos/DeleteProductos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto eliminado con éxito.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el producto.";
                }
            }
            return View("GestionarProductos");
        }

        [AuthorizeByRole("Vendedor")]
        private async Task<List<Productos>> ObtenerProductos()
        {
            List<Productos> productos = new List<Productos>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
            }
            return productos;
        }
        //--------------------------------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        private async Task<List<Categoria>> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync("api/Categorias/GetCategorias");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    categorias = JsonConvert.DeserializeObject<List<Categoria>>(res);
                }
            }
            return categorias;
        }
        [AuthorizeByRole("Vendedor")]
        private async Task<Productos> ObtenerProductoPorId(int id)
        {
            Productos producto = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                HttpResponseMessage response = await client.GetAsync($"api/Productos/GetProducto/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    producto = JsonConvert.DeserializeObject<Productos>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudo obtener el producto.";
                }
            }
            return producto;
        }
        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> GestionPedidos()
        {
            List<Pedidos> pedidos = new List<Pedidos>();
            List<Usuarios> clientes = new List<Usuarios>();
            List<Productos> productos = new List<Productos>();

            // Verificar si el token está en las cookies o en la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            string token = tokenCookie?.Value ?? tokenSession;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Obtener pedidos
                    HttpResponseMessage responsePedidos = await client.GetAsync("api/Pedidos/GetPedidos");
                    if (responsePedidos.IsSuccessStatusCode)
                    {
                        string jsonPedidos = await responsePedidos.Content.ReadAsStringAsync();
                        pedidos = JsonConvert.DeserializeObject<List<Pedidos>>(jsonPedidos) ?? new List<Pedidos>();
                    }

                    // Obtener clientes
                    HttpResponseMessage responseClientes = await client.GetAsync("api/Usuarios/GetUsuarios");
                    if (responseClientes.IsSuccessStatusCode)
                    {
                        string jsonClientes = await responseClientes.Content.ReadAsStringAsync();
                        clientes = JsonConvert.DeserializeObject<List<Usuarios>>(jsonClientes) ?? new List<Usuarios>();
                    }

                    // Obtener productos
                    HttpResponseMessage responseProductos = await client.GetAsync("api/Productos/GetProductos");
                    if (responseProductos.IsSuccessStatusCode)
                    {
                        string jsonProductos = await responseProductos.Content.ReadAsStringAsync();
                        productos = JsonConvert.DeserializeObject<List<Productos>>(jsonProductos) ?? new List<Productos>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            // Enviar datos a la vista usando ViewBag
            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;

            return View(pedidos);
        }

        //Esta es de la vista de descuentos
        //----------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> GestionDescuentos()
        {
            List<Descuentos> descuentos = new List<Descuentos>();

            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            string token = tokenCookie?.Value ?? tokenSession;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync("api/Descuentos/GetDescuentos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener las promociones.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    descuentos = JsonConvert.DeserializeObject<List<Descuentos>>(jsonResponse) ?? new List<Descuentos>();

                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    ViewBag.VendedorId = userId; // ✅ AQUÍ SE PASA A LA VISTA

                    descuentos = descuentos.Where(p => p.VendedorId == int.Parse(userId)).ToList();

                    if (!descuentos.Any())
                    {
                        TempData["Error"] = "No se encontraron promociones para el usuario logueado.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            return View(descuentos);
        }

        [HttpPost]
        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> AgregarDescuento(Descuentos model)
        {
            try
            {
                if (model.VendedorId == 0)
                {
                    int? userId = HomeController.TokenHelper.GetUserIdFromToken(HttpContext);
                    if (userId == null)
                    {
                        TempData["Error"] = "No se pudo obtener el ID del vendedor.";
                        return RedirectToAction("GestionDescuentos");
                    }
                    model.VendedorId = userId.Value;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Descuentos/PostDescuentos", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("GestionDescuentos");
                    }
                }

                TempData["Success"] = "Descuento agregado correctamente.";
                return RedirectToAction("GestionDescuentos");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("GestionDescuentos");
            }
        }

        [AuthorizeByRole("Vendedor")]
        public async Task<ActionResult> Dashboard()
        {
            int totalPedidos = 0;
            decimal totalIngresos = 0;
            List<Pedidos> pedidos = new List<Pedidos>();
            List<IngresosDiarios> ingresosDiarios = new List<IngresosDiarios>();
            List<ProductosMasVendidos> productosMasVendidos = new List<ProductosMasVendidos>();
            List<Usuarios> clientes = new List<Usuarios>();
            List<Productos> productos = new List<Productos>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);

                try
                {
                    pedidos = await ObtenerPedidos(client);
                    ingresosDiarios = await ObtenerIngresosDiarios(client);
                    productosMasVendidos = await ObtenerProductosMasVendidos(client);
                    clientes = await ObtenerClientes(client);
                    productos = await ObtenerProductos(client);

                    var pedidosCompletados = pedidos.Where(p => p.Estado == "Completado").ToList();
                    totalPedidos = pedidosCompletados.Count;
                    totalIngresos = pedidosCompletados.Sum(p => p.Total);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en DashboardController: {ex.Message}");
                }
            }

            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalIngresos = totalIngresos;
            ViewBag.IngresosDiarios = JsonConvert.SerializeObject(ingresosDiarios);
            ViewBag.ProductosMasVendidos = JsonConvert.SerializeObject(productosMasVendidos);
            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;

            return View(pedidos);
        }
        

        private async Task<List<IngresosDiarios>> ObtenerIngresosDiarios(HttpClient client)
        {
            var response = await client.GetAsync("api/Pedidos/GetIngresosPorDia");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<IngresosDiarios>>(json) ?? new List<IngresosDiarios>();
            }
            return new List<IngresosDiarios>();
        }

        private async Task<List<ProductosMasVendidos>> ObtenerProductosMasVendidos(HttpClient client)
        {
            var response = await client.GetAsync("api/Pedidos/GetProductosMasVendidos");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProductosMasVendidos>>(json) ?? new List<ProductosMasVendidos>();
            }
            return new List<ProductosMasVendidos>();
        }

        private async Task<List<Pedidos>> ObtenerPedidos(HttpClient client)
        {
            var response = await client.GetAsync("api/Pedidos/GetPedidos");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Pedidos>>(json) ?? new List<Pedidos>();
            }
            return new List<Pedidos>();
        }

        private async Task<List<Usuarios>> ObtenerClientes(HttpClient client)
        {
            var response = await client.GetAsync("api/Usuarios/GetUsuarios");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Usuarios>>(json) ?? new List<Usuarios>();
            }
            return new List<Usuarios>();
        }

        private async Task<List<Productos>> ObtenerProductos(HttpClient client)
        {
            var response = await client.GetAsync("api/Productos/GetProductos");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Productos>>(json) ?? new List<Productos>();
            }
            return new List<Productos>();
        }



        //-------------------------------------------------------------------------------------------------------
    }
}
