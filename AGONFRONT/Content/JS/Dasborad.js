// Gráfico de ingresos por día
document.addEventListener("DOMContentLoaded", function () {
    function cargarGrafico(id, dataKey, type, label, bgColor, borderColor) {
        var canvas = document.getElementById(id);
        if (canvas) {
            try {
                var parsedData = JSON.parse(canvas.getAttribute("data-" + dataKey));
                if (parsedData.length > 0) {
                    new Chart(canvas.getContext("2d"), {
                        type: type,
                        data: {
                            labels: parsedData.map(item => item.Fecha || item.Producto),
                            datasets: [{
                                label: label,
                                data: parsedData.map(item => item.TotalIngresos || item.CantidadVendida),
                                backgroundColor: bgColor,
                                borderColor: borderColor,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false, // Esto impide que Chart.js fuerce una relación de aspecto
                            scales: {
                                y: {
                                    beginAtZero: true,
                                    max: Math.max(...parsedData.map(item => item.TotalIngresos || item.CantidadVendida)) * 1.2
                                }
                            }
                        }
                    });
                }
            } catch (e) {
                console.error(`Error al parsear datos de ${id}:`, e);
            }
        }
    }

    // Cargar gráficos al iniciar
    cargarGrafico("ingresosChart", "ingresos", "line", "Ingresos", "rgba(52, 152, 219, 0.5)", "#3498db");
    cargarGrafico("productsChart", "productos", "bar", "Unidades Vendidas", "#e74c3c", "#c0392b");

    // Verificar y actualizar gráfico de productos
    var productosCanvas = document.getElementById("productsChart");
    if (productosCanvas) {
        var productosData = productosCanvas.getAttribute("data-productos");
        if (productosData) {
            try {
                var productos = JSON.parse(productosData);
                actualizarGraficoProductos(productos);
            } catch (e) {
                console.error("Error al parsear productos:", e);
            }
        }
    }
});

// Función para actualizar el gráfico de ingresos
function actualizarGraficoIngresos(ingresos) {
    var ctx = document.getElementById("ingresosChart").getContext("2d");
    new Chart(ctx, {
        type: "line",
        data: {
            labels: ingresos.map(i => i.Fecha),
            datasets: [{
                label: "Ingresos",
                data: ingresos.map(i => i.TotalIngresos),
                backgroundColor: "rgba(52, 152, 219, 0.5)",
                borderColor: "#3498db",
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
}

// Función para actualizar el gráfico de productos
function actualizarGraficoProductos(productos) {
    var ctx = document.getElementById("productsChart").getContext("2d");
    new Chart(ctx, {
        type: "bar",
        data: {
            labels: productos.map(p => p.Producto),
            datasets: [{
                label: "Unidades Vendidas",
                data: productos.map(p => p.CantidadVendida),
                backgroundColor: "#e74c3c",
                borderColor: "#c0392b",
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
}