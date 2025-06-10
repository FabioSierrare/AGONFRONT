using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using AGONFRONT.Filters;
using AGONFRONT.Models;
using Newtonsoft.Json;

namespace AGONFRONT.Controllers
{
    [AuthorizeByRole("3")]
    public class AdminController : Controller
    {
        private readonly string apiUrl = System.Configuration.ConfigurationManager.AppSettings["Api"];

        // =======================
        // 1. Dashboard
        // =======================
        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            return View();
        }

        // =======================
        // 2. Gestionar Usuarios
        // =======================
        // GET: Admin/Usuarios
        public async Task<ActionResult> Usuarios()
        {
            var lista = new List<Usuarios>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    // Esperamos la respuesta del API
                    var response = await client.GetAsync("api/Usuarios/GetUsuarios");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        lista = JsonConvert.DeserializeObject<List<Usuarios>>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No fue posible obtener la lista de usuarios.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
            }
            return View(lista);
        }

        // GET: Admin/EditarUsuario/5
        public async Task<ActionResult> EditarUsuario(int id)
        {
            Usuarios model = null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync($"api/Usuarios/GetUsuarios/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        model = JsonConvert.DeserializeObject<Usuarios>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No se encontró el usuario.";
                        return RedirectToAction("Usuarios");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Usuarios");
            }

            ViewBag.TiposUsuario = await ObtenerTiposUsuario();
            return View(model);
        }

        // POST: Admin/EditarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarUsuario(Usuarios model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TiposUsuario = await ObtenerTiposUsuario();
                return View(model);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"api/Usuarios/PutUsuarios/{model.Id}", content);

                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Usuario actualizado correctamente.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"API devolvió error: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }

            return RedirectToAction("Usuarios");
        }

        // POST: Admin/EliminarUsuario/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarUsuario(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.DeleteAsync($"api/Usuarios/DeleteUsuarios/{id}");
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Usuario eliminado.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error al eliminar usuario: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }
            return RedirectToAction("Usuarios");
        }

        // =======================
        // 3. Gestionar Comentarios
        // =======================
        // GET: Admin/Comentarios
        public async Task<ActionResult> Comentarios()
        {
            var lista = new List<Comentarios>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync("api/Comentarios/GetComentarios");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        lista = JsonConvert.DeserializeObject<List<Comentarios>>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No se pudieron cargar los comentarios.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
            }
            return View(lista);
        }

        // POST: Admin/EliminarComentario/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarComentario(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.DeleteAsync($"api/Comentarios/DeleteComentarios/{id}");
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Comentario eliminado correctamente.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error al eliminar el comentario: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }
            return RedirectToAction("Comentarios");
        }

        // =======================
        // 4. Gestionar Productos
        // =======================
        // GET: Admin/Productos
        public async Task<ActionResult> Productos()
        {
            var lista = new List<Productos>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync("api/Productos/GetProductos");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        lista = JsonConvert.DeserializeObject<List<Productos>>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No se pudieron cargar los productos.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
            }
            return View(lista);
        }

        // GET: Admin/EditarProducto/5
        public async Task<ActionResult> EditarProducto(int id)
        {
            Productos model = null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync($"api/Productos/GetProducto/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        model = JsonConvert.DeserializeObject<Productos>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No se encontró el producto.";
                        return RedirectToAction("Productos");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Productos");
            }

            ViewBag.Categoria = await ObtenerCategorias();
            ViewBag.Vendedores = await ObtenerVendedores();
            return View(model);
        }

        // POST: Admin/EditarProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarProducto(Productos model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategorias();
                ViewBag.Vendedores = await ObtenerVendedores();
                return View(model);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"api/Productos/PutProductos/{model.Id}", content);

                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Producto actualizado correctamente.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"API devolvió error: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }
            return RedirectToAction("Productos");
        }

        // POST: Admin/EliminarProducto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.DeleteAsync($"api/Productos/DeleteProductos/{id}");
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Producto eliminado correctamente.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error al eliminar el producto: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }
            return RedirectToAction("Productos");
        }

        // =======================
        // 5. Gestionar Categorías
        // =======================
        // GET: Admin/Categorias
        public async Task<ActionResult> Categorias()
        {
            var lista = new List<Categoria>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync("api/Categorias/GetCategoria");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        lista = JsonConvert.DeserializeObject<List<Categoria>>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No se pudieron cargar las categorías.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
            }
            return View(lista);
        }

        // GET: Admin/CrearCategoria
        public ActionResult CrearCategoria()
        {
            return View();
        }

        // POST: Admin/CrearCategoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearCategoria(Categoria model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/Categorias/PostCategoria", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Categoría creada correctamente.";
                        return RedirectToAction("Categorias");
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error al crear categoría: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }

            return View(model);
        }

        // GET: Admin/EditarCategoria/5
        public async Task<ActionResult> EditarCategoria(int id)
        {
            Categoria model = null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.GetAsync($"api/Categorias/GetCategoria/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        model = JsonConvert.DeserializeObject<Categoria>(json);
                    }
                    else
                    {
                        TempData["Error"] = "No se encontró la categoría.";
                        return RedirectToAction("Categorias");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Categorias");
            }

            return View(model);
        }

        // POST: Admin/EditarCategoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarCategoria(Categoria model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"api/Categorias/PutCategorias/{model.Id}", content);
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Categoría actualizada correctamente.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error al actualizar categoría: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }
            return RedirectToAction("Categorias");
        }

        // POST: Admin/EliminarCategoria/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var response = await client.DeleteAsync($"api/Categorias/DeleteCategoria/{id}");
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Categoría eliminada correctamente.";
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Error al eliminar categoría: {err}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Excepción: {ex.Message}";
            }
            return RedirectToAction("Categorias");
        }

        // =======================
        // 6. Helpers para dropdowns
        // =======================
        private async Task<IEnumerable<TipoUsuarios>> ObtenerTiposUsuario()
        {
            var tipos = new List<TipoUsuarios>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var resp = await client.GetAsync("api/TipoUsuarios/GetTipoUsuarios");
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        tipos = JsonConvert.DeserializeObject<List<TipoUsuarios>>(json);
                    }
                }
            }
            catch { }
            return tipos;
        }

        private async Task<IEnumerable<Categoria>> ObtenerCategorias()
        {
            var lista = new List<Categoria>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var resp = await client.GetAsync("api/Categorias/GetCategoria");
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        lista = JsonConvert.DeserializeObject<List<Categoria>>(json);
                    }
                }
            }
            catch { }
            return lista;
        }

        private async Task<IEnumerable<Usuarios>> ObtenerVendedores()
        {
            var vendedores = new List<Usuarios>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apiUrl);
                    var resp = await client.GetAsync("api/Usuarios/GetUsuarios");
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var todos = JsonConvert.DeserializeObject<List<Usuarios>>(json);
                        vendedores = todos.Where(u => u.TipoUsuarioId == 2).ToList();
                    }
                }
            }
            catch { }
            return vendedores;
        }
    }
}
