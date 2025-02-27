// Función para visualizar los detalles del envío
function viewShipmentDetails(orderId) {
    alert(`Visualizando detalles del envío para el pedido #${orderId}`);
    // Aquí puedes implementar una llamada a la API para obtener detalles del envío
}

// Función para manejar el formulario de política de envío
document.getElementById('shippingPolicyForm').addEventListener('submit', function(event) {
    event.preventDefault(); // Evita que el formulario se recargue

    const shippingMethod = document.getElementById('shipping-method').value;
    const shippingCost = parseFloat(document.getElementById('shipping-cost').value);
    const shippingDuration = document.getElementById('shipping-duration').value;

    if (!shippingMethod || !shippingCost || !shippingDuration) {
        alert('Por favor complete todos los campos correctamente.');
        return;
    }

    console.log('Política de Envío Creada:', {
        method: shippingMethod,
        cost: shippingCost,
        duration: shippingDuration
    });

    alert('Política de Envío guardada con éxito.');
    document.getElementById('shippingPolicyForm').reset();
});

// Función para manejar el formulario de asignación de transporte
document.getElementById('assignShippingForm').addEventListener('submit', function(event) {
    event.preventDefault(); // Evita el comportamiento por defecto del formulario

    const orderId = document.getElementById('order-id').value;
    const shippingCompany = document.getElementById('shipping-company').value;
    const trackingNumber = document.getElementById('tracking-number').value;

    if (!orderId || !shippingCompany || !trackingNumber) {
        alert('Por favor complete todos los campos correctamente.');
        return;
    }

    console.log('Asignación de Envío:', {
        orderId: orderId,
        shippingCompany: shippingCompany,
        trackingNumber: trackingNumber
    });

    alert(`Envío asignado para el pedido #${orderId} a la empresa ${shippingCompany} con tracking #${trackingNumber}`);
    document.getElementById('assignShippingForm').reset();
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
