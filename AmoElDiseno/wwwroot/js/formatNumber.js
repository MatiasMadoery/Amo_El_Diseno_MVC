//function formatNumber(input) {
//    // Removemos caracteres no numéricos (excepto coma y punto)
//    let value = input.value.replace(/[^0-9]/g, "");

//    // Aplicamos el formato de miles
//    value = new Intl.NumberFormat("es-AR").format(value);

//    // Asignamos el valor formateado al input
//    input.value = value;
//}

function formatNumber(input) {
    // Guarda la posición del cursor
    const cursorPosition = input.selectionStart;

    // Remover caracteres no numéricos excepto puntos y comas
    let value = input.value.replace(/[^0-9.,]/g, "");

    // Si hay decimales, dividimos en parte entera y decimal
    let parts = value.split(/[.,]/);
    let integerPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, "."); // Formatear miles
    let decimalPart = parts[1] || ""; // Parte decimal permanece intacta

    // Reconstruimos el número con la parte decimal (si existe)
    if (decimalPart) {
        input.value = `${integerPart},${decimalPart}`;
    } else {
        input.value = integerPart;
    }

    // Restaurar la posición del cursor para evitar problemas
    input.setSelectionRange(cursorPosition, cursorPosition);
}