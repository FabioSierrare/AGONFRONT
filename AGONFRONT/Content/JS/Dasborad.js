// Gráfico de ingresos por día
var incomeChartCtx = document.getElementById('incomeChart').getContext('2d');
var incomeChart = new Chart(incomeChartCtx, {
    type: 'line',
    data: {
        labels: ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo'],
        datasets: [{
            label: 'Ingresos',
            data: [1000, 2000, 1500, 2500, 3000, 3500, 4000],
            backgroundColor: 'rgba(39, 174, 96, 0.2)',
            borderColor: 'rgba(39, 174, 96, 1)',
            borderWidth: 2,
            fill: true
        }]
    }
});

// Gráfico de productos más vendidos
var productsChartCtx = document.getElementById('productsChart').getContext('2d');
var productsChart = new Chart(productsChartCtx, {
    type: 'bar',
    data: {
        labels: ['Maíz', 'Fertilizantes', 'Tractor', 'Semillas Trigo', 'Arado'],
        datasets: [{
            label: 'Ventas',
            data: [500, 300, 200, 150, 100],
            backgroundColor: '#3498db',
            borderColor: '#2980b9',
            borderWidth: 1
        }]
    }
});

// Función para cargar el contenido dinámicamente en el dashboard
function loadPage(page) {
    const content = document.getElementById('main-content');
    switch (page) {
        case 'dashboard':
            content.innerHTML = <h2>Resumen de Ventas</h2><p>Aquí se mostrarán las métricas clave del vendedor...</p>;
            break;
        case 'mis-productos':
            content.innerHTML = <h2>Mis Productos</h2><p>Aquí podrás agregar, editar y eliminar productos...</p>;
            break;
        case 'estadisticas':
            content.innerHTML = <h2>Estadísticas de Ventas</h2><p>Gráficos y reportes detallados sobre las ventas...</p>;
            break;
        case 'pedidos':
            content.innerHTML = <h2>Gestión de Pedidos</h2><p>Aquí podrás ver y actualizar el estado de los pedidos...</p>;
            break;
        case 'promociones':
            content.innerHTML = <h2>Promociones y Descuentos</h2><p>Gestiona tus descuentos y promociones aquí...</p>;
            break;
        case 'envios':
            content.innerHTML = <h2>Gestión de Envíos</h2><p>Visualiza y ajusta tus políticas de envío...</p>;
            break;
        case 'perfil':
            content.innerHTML = <h2>Perfil de Vendedor</h2><p>Aquí puedes ver y actualizar tu información personal...</p>;
            break;
        default:
            content.innerHTML = <h2>Bienvenido</h2><p>Selecciona una sección del menú para comenzar.</p>;
            break;
    }
}

// Cargar la página de inicio por defecto
document.addEventListener('DOMContentLoaded', function () {
    loadPage('dashboard');
});