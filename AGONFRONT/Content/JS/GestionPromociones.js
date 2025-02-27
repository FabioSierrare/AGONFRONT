// Función para manejar la creación de promociones
document.getElementById('promotionForm').addEventListener('submit', function(event) {
    event.preventDefault(); // Evita el comportamiento por defecto del formulario

    // Obtención de los datos del formulario
    const promotionName = document.getElementById('promotion-name').value;
    const discountType = document.getElementById('discount-type').value;
    const discountValue = parseFloat(document.getElementById('discount-value').value);
    const selectedProducts = Array.from(document.getElementById('products').selectedOptions).map(option => option.value);

    // Validación simple
    if (!promotionName || !discountValue || selectedProducts.length === 0) {
        alert('Por favor complete todos los campos correctamente.');
        return;
    }

    // Mostrar los datos de la promoción (puedes enviarlos a tu servidor)
    console.log('Promoción Creada:', {
        name: promotionName,
        discountType: discountType,
        discountValue: discountValue,
        products: selectedProducts,
    });

    // Limpiar el formulario después de crear la promoción
    document.getElementById('promotionForm').reset();
    alert('Promoción creada con éxito.');
});

// Función para editar una promoción
document.querySelectorAll('.edit-btn').forEach(btn => {
    btn.addEventListener('click', function() {
        const row = this.closest('tr');
        const promotionName = row.cells[0].textContent;
        const discount = row.cells[1].textContent;
        const products = row.cells[2].textContent;

        // Mostrar los datos de la promoción para editar (aquí podrías cargar los datos en un formulario para editar)
        alert(`Editando promoción: ${promotionName} con descuento ${discount} para los productos ${products}`);
    });
});

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
