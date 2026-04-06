// SIDEBAR
window.toggleSidebar = function () {
    if (!document.body) return;
    document.body.classList.toggle("sidebar-collapsed");
    const isCollapsed = document.body.classList.contains("sidebar-collapsed");
    localStorage.setItem("sidebarCollapsed", isCollapsed ? "1" : "0");
};

// FULLSCREEN
window.toggleFullscreenJS = function () {
    if (!document.documentElement) return;
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
        localStorage.setItem("fs", "1");
    } else {
        document.exitFullscreen();
        localStorage.setItem("fs", "0");
    }
    syncFullscreen();
};

function syncFullscreen() {
    if (!document.body) return;
    const isFs = document.fullscreenElement || localStorage.getItem("fs") === "1";
    if (isFs) {
        document.body.classList.add("is-fullscreen");
    } else {
        document.body.classList.remove("is-fullscreen");
    }
    
    // Actualizar ícono del botón fullscreen
    const btn = document.querySelector('.btn-fullscreen');
    if (btn) {
        const icon = btn.querySelector('i');
        if (icon) {
            if (isFs) {
                icon.className = 'bi bi-fullscreen-exit';
            } else {
                icon.className = 'bi bi-fullscreen';
            }
        }
    }
}

// Detectar cambios fullscreen
document.addEventListener("fullscreenchange", function () {
    if (document.fullscreenElement) {
        localStorage.setItem("fs", "1");
    } else {
        localStorage.setItem("fs", "0");
    }
    syncFullscreen();
});

// CARGAR ESTADOS
document.addEventListener("DOMContentLoaded", function () {
    if (!document.body) return;
    
    // Sidebar
    const savedSidebar = localStorage.getItem("sidebarCollapsed");
    if (savedSidebar === "1") {
        document.body.classList.add("sidebar-collapsed");
    }
    
    // Fullscreen
    syncFullscreen();
    
    // Tooltips
    const links = document.querySelectorAll(".sidebar a");
    links.forEach(link => {
        const text = link.querySelector(".menu-text");
        if (text) {
            link.setAttribute("title", text.innerText);
        }
    });
});

// Anti bug (opcional)
setInterval(syncFullscreen, 500);