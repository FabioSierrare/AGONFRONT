// Array para almacenar los productos
let products = [];

// Función para mostrar el formulario de agregar producto
function showAddProductForm() {
  document.getElementById('add-product-form').style.display = 'block';
}

// Función para ocultar el formulario de agregar producto
function hideAddProductForm() {
  document.getElementById('add-product-form').style.display = 'none';
}

// Función para agregar un nuevo producto
document.getElementById('product-form').addEventListener('submit', function (e) {
  e.preventDefault();

  const newProduct = {
    name: document.getElementById('product-name').value,
    description: document.getElementById('product-description').value,
    category: document.getElementById('product-category').value,
    price: parseFloat(document.getElementById('product-price').value),
    stock: parseInt(document.getElementById('product-stock').value),
    image: document.getElementById('product-image').files[0] ? document.getElementById('product-image').files[0].name : 'default.jpg'
  };

  products.push(newProduct);
  hideAddProductForm();
  renderProductTable();
});

// Función para renderizar la tabla de productos
function renderProductTable() {
  const tbody = document.getElementById('product-table').getElementsByTagName('tbody')[0];
  tbody.innerHTML = ''; // Limpiar tabla

  products.forEach((product, index) => {
    const row = tbody.insertRow();
    row.innerHTML = `
      <td>${product.name}</td>
      <td>${product.description}</td>
      <td>${product.category}</td>
      <td>$${product.price.toFixed(2)}</td>
      <td>${product.stock}</td>
      <td>
        <button class="edit-btn" onclick="editProduct(${index})">Editar</button>
        <button class="delete-btn" onclick="deleteProduct(${index})">Eliminar</button>
      </td>
    `;
  });
}

// Función para eliminar un producto
function deleteProduct(index) {
  products.splice(index, 1);  // Eliminar el producto del array
  renderProductTable();        // Re-renderizar la tabla
}

// Función para editar un producto (se puede extender según necesidades)
function editProduct(index) {
  alert(`Editar producto: ${products[index].name}`);
  // Aquí puedes agregar una lógica para abrir el formulario y pre-cargar los datos
}

// Función para cargar el contenido dinámicamente en el dashboard
function loadPage(page) {
  const content = document.getElementById('main-content');
  switch (page) {
      case 'dashboard':
          content.innerHTML = `<h2>Resumen de Ventas</h2><p>Aquí se mostrarán las métricas clave del vendedor...</p>`;
          break;
      case 'mis-productos':
          content.innerHTML = `<h2>Mis Productos</h2><p>Aquí podrás agregar, editar y eliminar productos...</p>`;
          break;
      case 'estadisticas':
          content.innerHTML = `<h2>Estadísticas de Ventas</h2><p>Gráficos y reportes detallados sobre las ventas...</p>`;
          break;
      case 'pedidos':
          content.innerHTML = `<h2>Gestión de Pedidos</h2><p>Aquí podrás ver y actualizar el estado de los pedidos...</p>`;
          break;
      case 'promociones':
          content.innerHTML = `<h2>Promociones y Descuentos</h2><p>Gestiona tus descuentos y promociones aquí...</p>`;
          break;
      case 'envios':
          content.innerHTML = `<h2>Gestión de Envíos</h2><p>Visualiza y ajusta tus políticas de envío...</p>`;
          break;
      case 'perfil':
          content.innerHTML = `<h2>Perfil de Vendedor</h2><p>Aquí puedes ver y actualizar tu información personal...</p>`;
          break;
      default:
          content.innerHTML = `<h2>Bienvenido</h2><p>Selecciona una sección del menú para comenzar.</p>`;
          break;
  }
}

// Cargar la página de inicio por defecto
document.addEventListener('DOMContentLoaded', function() {
  loadPage('dashboard');
});
