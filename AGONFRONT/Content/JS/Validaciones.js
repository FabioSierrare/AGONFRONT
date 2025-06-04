    function validarCampos() {
    // Validar campos con clase .sn (solo números)
    const snCampos = document.querySelectorAll('.sn');
    snCampos.forEach(campo => {
        campo.addEventListener('input', () => {
            campo.value = campo.value.replace(/[^0-9]/g, '');
        });
    });

    // Validar campos con clase .nce (sin caracteres especiales)
    const nceCampos = document.querySelectorAll('.nce');
    nceCampos.forEach(campo => {
        campo.addEventListener('input', () => {
            campo.value = campo.value.replace(/[^a-zA-Z0-9]/g, '');
        });
    });
  }

function validarSoloLetras() {
    const campos = document.querySelectorAll('.sl');

    campos.forEach(campo => {
        campo.addEventListener('input', () => {
            // Reemplaza todo lo que no sea letra (a-z, A-Z)
            campo.value = campo.value.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '');
        });
    });
    // Ejecutar validación cuando cargue la página
    window.addEventListener('DOMContentLoaded', validarCampos);
