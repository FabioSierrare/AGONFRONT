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


        /// <summary>
        /// Crea un nuevo usuario enviando la información al microservicio de usuarios.
        /// </summary>
        /// <param name="model">El modelo de usuario con la información ingresada.</param>
        /// <returns>Redirige a la vista correspondiente según el resultado de la operación.</returns>
        [HttpPost]
        public async Task<ActionResult> Create(Usuarios model)
        {
            try
            {
                // 🔒 Encripta la contraseña antes de enviarla al servicio
                model.Contraseña = Encriptador.Encriptar(model.Contraseña);

                using (var client = new HttpClient())
                {
                    // Establece la URL base de la API
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    // Serializa el modelo a formato JSON
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Envía la solicitud POST al microservicio de usuarios
                    HttpResponseMessage response = await client.PostAsync("api/Usuarios/PostUsuarios", content);

                    // Verifica si la respuesta fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        var res = await response.Content.ReadAsStringAsync();
                        // Aquí puedes manejar la respuesta si es necesario
                    }
                    else
                    {
                        // Captura errores y los guarda en TempData para mostrarlos en la vista
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error de API: {response.StatusCode} - {errorContent}";
                        return RedirectToAction("Iniciar", "Home");
                    }
                }

                // Redirige al usuario a la vista principal si todo fue exitoso
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción y muestra un mensaje de error
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }
        }


        //Estos controladortes son de la vista de mi producto vendedor
        //------------------------------------------------------------- 
        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Obtiene los productos del vendedor autenticado desde la API y los muestra en la vista.
        /// </summary>
        /// <returns>Vista con la lista de productos del vendedor autenticado.</returns>
        public async Task<ActionResult> Misproductosvendedor()
        {
            // Lista para almacenar los productos recuperados
            List<Productos> productos = new List<Productos>();

            // 🔐 Verificar si el token está disponible en cookies o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                // Si no hay token, se redirige al inicio de sesión
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // Obtiene el token desde la cookie o sesión
            string token = tokenCookie?.Value ?? tokenSession;

            try
            {
                using (var client = new HttpClient())
                {
                    // Establece la URL base y el encabezado de autorización con el token
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Solicita todos los productos a la API
                    HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener los datos de productos.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Lee y deserializa el contenido de la respuesta
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res) ?? new List<Productos>();

                    // 🔍 Obtiene el ID del usuario desde el token JWT
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Filtra los productos que pertenecen al vendedor autenticado
                    productos = productos.Where(p => p.VendedorId == int.Parse(userId)).ToList();

                    if (!productos.Any())
                    {
                        TempData["Error"] = "No se encontraron productos para el usuario logueado.";
                    }
                }
            }
            catch (Exception ex)
            {
                // 🛠 Manejo de errores generales
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            // Devuelve la vista con la lista de productos del vendedor
            return View(productos);
        }

        [AuthorizeByRole("Vendedor")]
        [HttpPost]
        /// <summary>
        /// Envía una solicitud a la API para actualizar la información de un producto del vendedor.
        /// </summary>
        /// <param name="productos">Objeto con la información actualizada del producto.</param>
        /// <returns>Redirige a la vista de productos del vendedor, con mensaje de éxito o error.</returns>
        public async Task<ActionResult> Editarmiproducto(Productos productos)
        {
            // 🔐 Obtener el token JWT desde las cookies o la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // 🚫 Validar que el token no esté vacío
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // Configurar la URL base y la autenticación con el token
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    // 📦 Serializar el objeto producto a JSON y empaquetarlo como contenido HTTP
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(productos), Encoding.UTF8, "application/json");

                    // 🔁 Enviar solicitud PUT a la API para actualizar el producto
                    HttpResponseMessage response = await client.PutAsync($"api/Productos/PutProductos/{productos.Id}", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        // ✅ Si la respuesta es exitosa, mostrar mensaje de éxito
                        TempData["Success"] = "Producto actualizado correctamente.";
                    }
                    else
                    {
                        // ❌ Si falla, leer y mostrar el mensaje de error de la API
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"❌ Error al actualizar controlador:\n{errorMessage}";

                        // 🛠 Imprimir en consola para depuración
                        Console.WriteLine($"[API ERROR]: {errorMessage}");

                        return RedirectToAction("Misproductosvendedor");
                    }
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Manejo de excepciones generales
                TempData["Error"] = $"⚠️ Excepción no controlada:\n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Misproductosvendedor");
            }

            // Redirigir a la vista de productos del vendedor
            return RedirectToAction("Misproductosvendedor");
        }


        [AuthorizeByRole("Vendedor")]
        [HttpPost]
        /// <summary>
        /// Elimina un producto del vendedor a través de la API, usando su ID.
        /// </summary>
        /// <param name="id">ID del producto que se desea eliminar.</param>
        /// <returns>Redirige a la vista de productos del vendedor, con mensaje de éxito o error.</returns>
        public async Task<ActionResult> EliminarMiProducto(int id)
        {
            // 🔐 Obtener el token JWT desde cookies o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie?.Value ?? tokenSession;

            // 🚫 Validar que el token exista
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // 📡 Configurar base URL y encabezado de autorización
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // 🛠 Logs de diagnóstico para seguimiento del proceso
                    Console.WriteLine($"[REQUEST] DELETE api/Productos/DeleteProductos/{id}");
                    Console.WriteLine($"[IN] Entró al método EliminarMiProducto con ID: {id}");

                    // 🧹 Realizar la solicitud DELETE a la API
                    HttpResponseMessage response = await client.DeleteAsync($"api/Productos/DeleteProductos/{id}");

                    // 🧾 Leer y mostrar el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[RESPONSE] Status: {response.StatusCode} | Content-Type: {response.Content.Headers.ContentType}");
                    Console.WriteLine($"[RESPONSE BODY] {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        // ✅ Producto eliminado con éxito
                        TempData["Success"] = "Producto eliminado correctamente.";
                    }
                    else
                    {
                        // ❌ Mostrar error de API
                        TempData["Error"] = $"❌ Error al eliminar el producto: {responseContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Capturar y mostrar errores no controlados
                Console.WriteLine($"[EXCEPTION] {ex.Message}");
                TempData["Error"] = $"Ocurrió un error: {ex.Message}";
            }

            // 🔁 Redirigir de vuelta a la lista de productos del vendedor
            return RedirectToAction("Misproductosvendedor");
        }


        //--------------------------------------------------------------------------------------------------

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

        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Acción que permite obtener la lista de usuarios para actualizar el perfil del vendedor.
        /// Verifica el token JWT en cookies o sesión, así como su expiración.
        /// </summary>
        /// <returns>Vista con la lista de usuarios o redirección al login en caso de error.</returns>
        public async Task<ActionResult> UpdatePerfilVendedor()
        {
            List<Usuarios> usuarios = new List<Usuarios>();

            // 🔐 Obtener el token JWT y la fecha de expiración desde cookies o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenExpirationCookie = Request.Cookies["TokenExpirationTime"];
            var tokenSession = Session["BearerToken"] as string;

            // 🧪 Debug: imprimir valores en consola para verificar
            Console.WriteLine("BearerToken Cookie en Request: " + tokenCookie?.Value);
            Console.WriteLine("TokenExpirationTime Cookie en Request: " + tokenExpirationCookie?.Value);

            // 🚫 Si no hay token en cookies ni sesión, redirigir al login
            if (tokenCookie == null && string.IsNullOrEmpty(tokenSession))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // ✅ Obtener el valor del token desde cookie o sesión
            string token = tokenCookie?.Value ?? tokenSession;

            // ⏰ Verificar si el token ha expirado
            DateTime? expirationTime = null;
            if (tokenExpirationCookie != null)
            {
                expirationTime = DateTime.TryParse(tokenExpirationCookie.Value, out var expiryDate)
                                 ? expiryDate : (DateTime?)null;
            }

            // ⚠️ Si el token ha expirado, eliminar cookies y redirigir al login
            if (expirationTime.HasValue && DateTime.Now > expirationTime.Value)
            {
                HttpContext.Response.Cookies.Remove("BearerToken");
                HttpContext.Response.Cookies.Remove("TokenExpirationTime");

                TempData["Error"] = "La sesión ha expirado. Por favor inicia sesión nuevamente.";
                return RedirectToAction("Iniciar", "Home");
            }

            // 🌐 Realizar llamada HTTP a la API de usuarios
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);

                HttpResponseMessage response = await client.GetAsync("api/Usuarios/GetUsuarios");

                if (response.IsSuccessStatusCode)
                {
                    // 📥 Leer el contenido de la respuesta y deserializarlo a lista de usuarios
                    var res = await response.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuarios>>(res);
                }
                else
                {
                    TempData["Error"] = "No se pudieron obtener los datos del usuario.";
                }
            }

            // 📤 Devolver la vista con los datos de usuario cargados
            return View(usuarios);
        }


        [HttpPost]

        /// <summary>
        /// Acción para actualizar la información de un usuario mediante una llamada PUT a la API.
        /// Requiere un token JWT válido almacenado en cookie o sesión.
        /// </summary>
        /// <param name="usuario">Objeto Usuarios con la información actualizada.</param>
        /// <returns>Redirección a la vista de perfil del vendedor con mensajes de éxito o error.</returns>
        public async Task<ActionResult> UpdateUsuario(Usuarios usuario)
        {
            // 🔐 Obtener el token JWT desde cookie o sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            // Asignar el token desde la cookie si existe, de lo contrario desde la sesión
            string token = tokenCookie?.Value ?? tokenSession;

            // 🚫 Si no hay token, redirigir al login
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta acción. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            // 🌐 Enviar solicitud PUT a la API con el objeto usuario
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 📦 Serializar el objeto usuario a JSON
                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(usuario),
                    Encoding.UTF8,
                    "application/json"
                );

                // 🔁 Realizar solicitud PUT a la API para actualizar el usuario
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Usuarios/PutUsuarios/{usuario.Id}",
                    jsonContent
                );

                // ✅ Si la respuesta es exitosa
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Usuario actualizado correctamente.";
                }
                else
                {
                    // ⚠️ Si hay error, leer el contenido del error
                    string errorMessage = await response.Content.ReadAsStringAsync();

                    TempData["Error"] = $"Error al actualizar usuario: {errorMessage}";
                    Console.WriteLine($"Error en API: {errorMessage}");

                    // Regresar a la misma vista para que el usuario vea el error
                    return RedirectToAction("UpdatePerfilVendedor");
                }
            }

            // 🔄 Redirigir de nuevo a la vista de perfil después de la actualización
            return RedirectToAction("UpdatePerfilVendedor");
        }


        //Aca estan todos los controladores de gestionar productos
        //--------------------------------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Acción para obtener y mostrar los productos del vendedor autenticado.
        /// Valida el token JWT y consulta los productos desde la API.
        /// </summary>
        /// <returns>Vista con la lista de productos del vendedor autenticado.</returns>
        public async Task<ActionResult> GestionarProductos()
        {
            List<Productos> productos = new List<Productos>();

            // 🔐 Obtener el token desde cookie o sesión
            var tokenCookie = Request.Cookies["BearerToken"]?.Value;
            var tokenSession = Session["BearerToken"] as string;
            string token = tokenCookie ?? tokenSession;

            // 🚫 Verificar si el token está vacío
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No tienes acceso a esta página. Por favor inicia sesión.";
                return RedirectToAction("Iniciar", "Home");
            }

            try
            {
                // 🌐 Realizar solicitud GET a la API de productos
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                    // ⚠️ Si la respuesta no es exitosa, redirigir con mensaje de error
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener los datos de productos.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // ✅ Leer y deserializar los productos
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res) ?? new List<Productos>();

                    // ✅ Obtener el ID del usuario desde el token
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // ✅ Pasar el ID del usuario a la vista si es necesario
                    ViewBag.UsuarioId = userId;


                    // 📋 Filtrar productos que pertenecen al vendedor autenticado
                    productos = productos
                        .Where(p => p.VendedorId == int.Parse(userId))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Manejo de errores en caso de fallo de red o excepción inesperada
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            // ✅ Mostrar la vista con los productos filtrados
            return View(productos);
        }


        /// <summary>
        /// Sube una imagen a un contenedor de blobs de Azure Storage y devuelve la URL pública de la imagen subida.
        /// </summary>
        /// <param name="imagen">El archivo de imagen a subir.</param>
        /// <returns>Una tarea asíncrona que retorna la URL absoluta de la imagen almacenada en Azure Blob Storage.</returns>
        private async Task<string> SubirImagenAzure(HttpPostedFileBase imagen)
        {
            // Cadena de conexión al almacenamiento de Azure configurada en Web.config o appsettings
            string storageConnectionString = ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ConnectionString;

            // Nombre del contenedor donde se almacenarán las imágenes
            string containerName = "imagenesproductos";

            // Crear la cuenta de almacenamiento usando la cadena de conexión
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Crear cliente para acceder a blobs
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Obtener referencia al contenedor específico
            var container = blobClient.GetContainerReference(containerName);

            // Crear el contenedor si no existe y establecer permisos públicos para blobs
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Generar un nombre único para el archivo usando un GUID y conservar la extensión original
            string nombreArchivo = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(imagen.FileName);

            // Obtener referencia al blob donde se guardará la imagen
            var blockBlob = container.GetBlockBlobReference(nombreArchivo);

            // Establecer el tipo de contenido del blob (por ejemplo, image/jpeg)
            blockBlob.Properties.ContentType = imagen.ContentType;

            // Subir la imagen desde el stream del archivo recibido
            await blockBlob.UploadFromStreamAsync(imagen.InputStream);

            // Retornar la URL pública de la imagen almacenada
            return blockBlob.Uri.AbsoluteUri;
        }


        [AuthorizeByRole("Vendedor")]
        [HttpPost]
        /// <summary>
        /// Agrega un nuevo producto con una imagen subida a Azure Storage.
        /// </summary>
        /// <param name="model">El objeto Producto que contiene los datos del producto a agregar.</param>
        /// <param name="Imagen">El archivo de imagen que se subirá y asociará al producto.</param>
        /// <returns>Una tarea asíncrona que retorna una acción que redirige a la vista de gestión de productos.</returns>
        public async Task<ActionResult> AgregarProducto(Productos model, HttpPostedFileBase Imagen)
        {
            try
            {
                // Verificar que se haya proporcionado una imagen válida
                if (Imagen != null && Imagen.ContentLength > 0)
                {
                    // Subir imagen a Azure y obtener la URL pública
                    var urlImagen = await SubirImagenAzure(Imagen);

                    // Asignar la URL de la imagen al modelo del producto
                    model.UrlImagen = urlImagen;
                }
                else
                {
                    // Si no hay imagen, mostrar error y redirigir a la gestión de productos
                    TempData["Error"] = "Debes subir una imagen para el producto.";
                    return RedirectToAction("GestionarProductos");
                }

                using (var client = new HttpClient())
                {
                    // Establecer la dirección base del cliente HTTP
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Clear();

                    // Serializar el modelo del producto a JSON
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Enviar petición POST a la API para agregar el producto
                    HttpResponseMessage response = await client.PostAsync("api/Productos/PostProductos", content);

                    // Si la respuesta no es exitosa, capturar el error y mostrarlo
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
                // Capturar cualquier excepción y mostrar el mensaje de error
                TempData["Error"] = $"Hubo un error al procesar la solicitud: {ex.Message}";
            }

            // Redirigir a la vista de gestión de productos
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
        /// <summary>
        /// Obtiene la lista completa de productos desde la API.
        /// </summary>
        /// <returns>Una tarea asíncrona que retorna una lista de objetos <see cref="Productos"/>.</returns>
        private async Task<List<Productos>> ObtenerProductos()
        {
            List<Productos> productos = new List<Productos>();

            using (var client = new HttpClient())
            {
                // Establece la URL base del cliente HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Envía una solicitud GET para obtener los productos
                HttpResponseMessage response = await client.GetAsync("api/Productos/GetProductos");

                // Si la respuesta es exitosa, deserializa el contenido a la lista de productos
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    productos = JsonConvert.DeserializeObject<List<Productos>>(res);
                }
            }

            // Retorna la lista de productos (vacía si hubo error)
            return productos;
        }

        //--------------------------------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Obtiene la lista completa de categorías desde la API.
        /// </summary>
        /// <returns>Una tarea asíncrona que retorna una lista de objetos <see cref="Categoria"/>.</returns>
        private async Task<List<Categoria>> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            using (var client = new HttpClient())
            {
                // Establece la URL base del cliente HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Envía una solicitud GET para obtener las categorías
                HttpResponseMessage response = await client.GetAsync("api/Categorias/GetCategorias");

                // Si la respuesta es exitosa, deserializa el contenido a la lista de categorías
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    categorias = JsonConvert.DeserializeObject<List<Categoria>>(res);
                }
            }

            // Retorna la lista de categorías (vacía si hubo error)
            return categorias;
        }

        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Obtiene un producto específico por su ID desde la API.
        /// </summary>
        /// <param name="id">El identificador único del producto.</param>
        /// <returns>Una tarea asíncrona que retorna el objeto <see cref="Productos"/> correspondiente al ID, o null si no se encuentra.</returns>
        private async Task<Productos> ObtenerProductoPorId(int id)
        {
            Productos producto = null;

            using (var client = new HttpClient())
            {
                // Establece la URL base del cliente HTTP
                client.BaseAddress = new Uri(apiUrl);

                // Envía una solicitud GET para obtener el producto por su ID
                HttpResponseMessage response = await client.GetAsync($"api/Productos/GetProducto/{id}");

                // Si la respuesta es exitosa, deserializa el contenido a un objeto Productos
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    producto = JsonConvert.DeserializeObject<Productos>(res);
                }
                else
                {
                    // En caso de error, guarda el mensaje para mostrar en la vista
                    TempData["Error"] = "No se pudo obtener el producto.";
                }
            }

            // Retorna el producto obtenido o null si hubo error
            return producto;
        }

        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Obtiene y muestra la lista de pedidos junto con sus clientes y productos relacionados.
        /// </summary>
        /// <returns>
        /// Una tarea asíncrona que retorna una acción para renderizar la vista con la lista de pedidos.
        /// Si no se encuentra el token de autenticación, redirige a la acción "Iniciar" del controlador "Home".
        /// </returns>
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
                    // Configurar la base URL y el encabezado de autorización con el token Bearer
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Obtener la lista de pedidos desde la API
                    HttpResponseMessage responsePedidos = await client.GetAsync("api/Pedidos/GetPedidos");
                    if (responsePedidos.IsSuccessStatusCode)
                    {
                        string jsonPedidos = await responsePedidos.Content.ReadAsStringAsync();
                        pedidos = JsonConvert.DeserializeObject<List<Pedidos>>(jsonPedidos) ?? new List<Pedidos>();
                    }

                    // Obtener la lista de clientes desde la API
                    HttpResponseMessage responseClientes = await client.GetAsync("api/Usuarios/GetUsuarios");
                    if (responseClientes.IsSuccessStatusCode)
                    {
                        string jsonClientes = await responseClientes.Content.ReadAsStringAsync();
                        clientes = JsonConvert.DeserializeObject<List<Usuarios>>(jsonClientes) ?? new List<Usuarios>();
                    }

                    // Obtener la lista de productos desde la API
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
                // En caso de error, registrar y mostrar mensaje al usuario
                Console.WriteLine($"[ERROR] {ex.Message}");
                TempData["Error"] = $"Error en la conexión con la API: {ex.Message}";
                return RedirectToAction("Iniciar", "Home");
            }

            // Pasar las listas de clientes y productos a la vista a través de ViewBag
            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;

            // Retornar la vista con la lista de pedidos
            return View(pedidos);
        }


        //Esta es de la vista de descuentos
        //----------------------------------------------------------------
        [AuthorizeByRole("Vendedor")]
        /// <summary>
        /// Obtiene y muestra la lista de descuentos/promociones asociados al usuario vendedor autenticado.
        /// </summary>
        /// <returns>
        /// Una tarea asíncrona que retorna una acción para renderizar la vista con la lista de descuentos filtrados por el usuario.
        /// Si no se encuentra el token de autenticación o no se puede obtener información del usuario, redirige a la acción "Iniciar" del controlador "Home".
        /// </returns>
        public async Task<ActionResult> GestionDescuentos()
        {
            List<Descuentos> descuentos = new List<Descuentos>();

            // Obtener token de las cookies o la sesión
            var tokenCookie = Request.Cookies["BearerToken"];
            var tokenSession = Session["BearerToken"] as string;

            // Validar que el token exista
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
                    // Configurar cliente HTTP con la URL base y token Bearer
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Obtener descuentos desde la API
                    HttpResponseMessage response = await client.GetAsync("api/Descuentos/GetDescuentos");

                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = "No se pudieron obtener las promociones.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Deserializar la respuesta JSON
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    descuentos = JsonConvert.DeserializeObject<List<Descuentos>>(jsonResponse) ?? new List<Descuentos>();

                    // Obtener el Id del usuario logueado desde el token
                    string userId = GetLoggedInUserId(token);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Iniciar", "Home");
                    }

                    // Pasar el Id del vendedor a la vista mediante ViewBag
                    ViewBag.VendedorId = userId;

                    // Filtrar descuentos para mostrar solo los del vendedor logueado
                    descuentos = descuentos.Where(p => p.VendedorId == int.Parse(userId)).ToList();

                    // Si no hay descuentos para el usuario, mostrar mensaje de error
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
        /// <summary>
        /// Agrega un nuevo descuento para un vendedor específico.
        /// </summary>
        /// <param name="model">Objeto <see cref="Descuentos"/> que contiene los datos del descuento a agregar.</param>
        /// <returns>
        /// Una tarea asíncrona que retorna una acción <see cref="ActionResult"/> que redirige a la vista de gestión de descuentos.
        /// En caso de error, muestra mensajes en <see cref="TempData"/> y redirige a la misma vista.
        /// </returns>
        public async Task<ActionResult> AgregarDescuento(Descuentos model)
        {
            try
            {
                // Si el Id del vendedor no está asignado, intentar obtenerlo desde el token en el contexto HTTP
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

                // Enviar solicitud POST a la API para agregar el descuento
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Descuentos/PostDescuentos", content);

                    // Manejar error en la respuesta de la API
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
        /// <summary>
        /// Obtiene y prepara los datos necesarios para mostrar el panel de control (Dashboard) de la aplicación.
        /// </summary>
        /// <returns>
        /// Una tarea asíncrona que retorna una acción <see cref="ActionResult"/> con la vista que contiene los datos
        /// de pedidos, ingresos, productos más vendidos, clientes y productos.
        /// </returns>
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
                    // Obtener listas desde la API mediante métodos auxiliares
                    pedidos = await ObtenerPedidos(client);
                    ingresosDiarios = await ObtenerIngresosDiarios(client);
                    productosMasVendidos = await ObtenerProductosMasVendidos(client);
                    clientes = await ObtenerClientes(client);
                    productos = await ObtenerProductos(client);

                    // Filtrar pedidos completados y calcular totales
                    var pedidosCompletados = pedidos.Where(p => p.Estado == "Completado").ToList();
                    totalPedidos = pedidosCompletados.Count;
                    totalIngresos = pedidosCompletados.Sum(p => p.Total);
                }
                catch (Exception ex)
                {
                    // Registrar error para depuración
                    System.Diagnostics.Debug.WriteLine($"Error en DashboardController: {ex.Message}");
                }
            }

            // Pasar datos a la vista usando ViewBag
            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalIngresos = totalIngresos;
            ViewBag.IngresosDiarios = JsonConvert.SerializeObject(ingresosDiarios);
            ViewBag.ProductosMasVendidos = JsonConvert.SerializeObject(productosMasVendidos);
            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;

            return View(pedidos);
        }



        /// <summary>
        /// Obtiene la lista de ingresos diarios desde la API.
        /// </summary>
        /// <param name="client">Instancia de <see cref="HttpClient"/> para realizar la solicitud HTTP.</param>
        /// <returns>
        /// Una tarea que representa la operación asincrónica.
        /// El resultado contiene una lista de objetos <see cref="IngresosDiarios"/> con los ingresos por día.
        /// Si la respuesta no es exitosa o la deserialización falla, retorna una lista vacía.
        /// </returns>
        /// <summary>
        /// Obtiene la lista de ingresos diarios desde la API.
        /// </summary>
        /// <param name="client">Instancia de HttpClient para realizar la solicitud HTTP.</param>
        /// <returns>Lista de ingresos diarios o lista vacía en caso de error.</returns>
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

        /// <summary>
        /// Obtiene la lista de productos más vendidos desde la API.
        /// </summary>
        /// <param name="client">Instancia de HttpClient para realizar la solicitud HTTP.</param>
        /// <returns>Lista de productos más vendidos o lista vacía en caso de error.</returns>
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

        /// <summary>
        /// Obtiene la lista de pedidos desde la API.
        /// </summary>
        /// <param name="client">Instancia de HttpClient para realizar la solicitud HTTP.</param>
        /// <returns>Lista de pedidos o lista vacía en caso de error.</returns>
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

        /// <summary>
        /// Obtiene la lista de clientes desde la API.
        /// </summary>
        /// <param name="client">Instancia de HttpClient para realizar la solicitud HTTP.</param>
        /// <returns>Lista de usuarios o lista vacía en caso de error.</returns>
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

        /// <summary>
        /// Obtiene la lista de productos desde la API.
        /// </summary>
        /// <param name="client">Instancia de HttpClient para realizar la solicitud HTTP.</param>
        /// <returns>Lista de productos o lista vacía en caso de error.</returns>
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
