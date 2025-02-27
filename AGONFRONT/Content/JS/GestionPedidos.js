// Función para abrir el modal con los detalles del pedido
function viewOrderDetails(orderId) {
    // Aquí puedes simular la obtención de detalles del pedido
    const orderDetails = {
        12345: {
            client: 'Juan Pérez',
            products: ['Semillas de Maíz', 'Fertilizante NPK'],
            shippingAddress: 'Calle Ficticia 123, Ciudad',
            totalAmount: '$1,500',
            orderDate: '2024-11-26',
        },
        12346: {
            client: 'Ana Martínez',
            products: ['Tractor Modelo X'],
            shippingAddress: 'Avenida Real 456, Ciudad',
            totalAmount: '$25,000',
            orderDate: '2024-11-25',
        }
    };

    const order = orderDetails[orderId];
    if (order) {
        const modalContent = document.getElementById('order-details-content');
        modalContent.innerHTML = `
            <p><strong>Cliente:</strong> ${order.client}</p>
            <p><strong>Productos:</strong> ${order.products.join(', ')}</p>
            <p><strong>Dirección de Envío:</strong> ${order.shippingAddress}</p>
            <p><strong>Total:</strong> ${order.totalAmount}</p>
            <p><strong>Fecha del Pedido:</strong> ${order.orderDate}</p>
        `;
        document.getElementById('orderDetailsModal').style.display = 'block';
    }
}

// Función para cerrar el modal
function closeModal() {
    document.getElementById('orderDetailsModal').style.display = 'none';
}

// Función para manejar el cambio de estado del pedido
document.querySelectorAll('.order-status').forEach(select => {
    select.addEventListener('change', function() {
        alert(`Estado del pedido actualizado a: ${this.value}`);
        // Aquí puedes hacer una llamada al backend para actualizar el estado del pedido
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
