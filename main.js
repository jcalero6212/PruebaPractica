const apiBase = "https://localhost:7013/api";
const token = localStorage.getItem("token");

// Redirige si no hay token
if (!token) {
  window.location.href = "login.html";
}

// Función para cerrar sesión
function logout() {
  localStorage.removeItem("token");
  window.location.href = "login.html";
}

// Función para cargar artículos
async function cargarArticulos() {
  const res = await fetch(`${apiBase}/articulos`, {
    headers: { Authorization: `Bearer ${token}` }
  });

  if (!res.ok) {
    alert("No autorizado. Inicia sesión nuevamente.");
    logout();
    return;
  }

  const data = await res.json();
  const tbody = document.querySelector("#tablaArticulos tbody");
  tbody.innerHTML = "";

  data.forEach(a => {
    const fila = `
      <tr>
        <td>${a.id}</td>
        <td>${a.codigo}</td>
        <td>${a.nombre}</td>
        <td>${a.ubicacion}</td>
      </tr>
    `;
    tbody.innerHTML += fila;
  });
}

cargarArticulos();

