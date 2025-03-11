// Esperar a que el DOM esté completamente cargado
document.addEventListener("DOMContentLoaded", function () {
    if (typeof VentasPorSemana !== "undefined" && VentasPorSemana.length > 0) {
        actualizarGraficoVentas(VentasPorSemana);
    }
    if (typeof ProductoMasVendido !== "undefined" && ProductoMasVendido.length > 0) {
        actualizarGraficoProductos(ProductoMasVendido);
    }
});

// Función para actualizar el gráfico de ventas
function actualizarGraficoVentas(VentasPorSemana) {
    var fechas = VentasPorSemana.map(venta => venta.semana);
    var ingresos = VentasPorSemana.map(venta => venta.totalVentas);

    var ctx = document.getElementById('salesPerformanceChart').getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ["Semana 1", "Semana 2", "Semana 3", "Semana 4" ],
            datasets: [{
                label: 'Ingresos Mensuales',
                backgroundColor: 'rgba(39, 174, 96, 0.2)',
                borderColor: 'rgba(39, 174, 96, 1)',
                data: ingresos,
                borderWidth: 2,
                fill: true
            }]
        }
    });
}

// Función para actualizar el gráfico de productos más vendidos
function actualizarGraficoProductos(productosMasVendidos) {
    var nombres = productosMasVendidos.map(producto => producto.Producto);
    var cantidades = productosMasVendidos.map(producto => producto.totalVendido);

    var ctx = document.getElementById('topSellingProductsChart').getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: nombres,
            datasets: [{
                label: 'Unidades Vendidas',
                data: cantidades,
                backgroundColor: '#3498db',
                borderColor: '#2980b9',
                borderWidth: 1
            }]
        }
    });
}
