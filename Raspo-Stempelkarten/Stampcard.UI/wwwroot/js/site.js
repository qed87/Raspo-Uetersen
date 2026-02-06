// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('a[data-loading="true"]').forEach(link => {
        link.addEventListener('click', function(e) {
            // e.preventDefault();   ← nur wenn du AJAX machst und nicht navigieren willst

            document.getElementById('loadingOverlay').style.display = 'flex';

            // Optional: Button/Link visuell deaktivieren
            this.setAttribute('aria-busy', 'true');
            this.style.opacity = '0.6';
            this.style.cursor = 'wait';
        });
    });
    
    // Alle Formulare abfangen
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function () {
            // Spinner anzeigen
            document.getElementById('loadingOverlay').style.display = 'flex';

            // Optional: Submit-Button deaktivieren
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = 'Wird gesendet...';
            }
        });
    });
});