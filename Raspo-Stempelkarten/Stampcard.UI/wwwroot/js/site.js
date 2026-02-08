// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(function () {
    setTimeout(function () {

        $('form').each(function () {
            const $form = $(this);

            // Nur Formulare behandeln, die tatsächlich validiert werden (data-val=true irgendwo drin)
            if ($form.find('[data-val="true"]').length === 0) {
                return; // ← überspringen, wenn kein validierbares Feld vorhanden
            }

            // Validator holen (wird von jquery.validate.unobtrusive gesetzt)
            const validator = $form.data('validator');          

            if (validator) {
                validator.settings.submitHandler = function (form) {
                    const $form = $(form);

                    // Den zuvor gemerkten Button holen
                    const $clickedButton = $form.data('lastClickedSubmitButton');

                    if (!$clickedButton || !$clickedButton.length) {
                        console.warn("Kein Submit-Button erkannt → Fallback");
                        form.submit();
                        return;
                    }

                    // Lade-Indikator + Button deaktivieren
                    $('#loadingOverlay').css('display', 'flex');
                    $clickedButton.prop('disabled', true).html('Wird gesendet …');

                    // WICHTIG: formaction des Buttons wieder setzen!
                    const originalFormAction = $clickedButton.attr('formaction');
                    if (originalFormAction) {
                        $form.attr('action', originalFormAction);   // ← das repariert den Handler!
                    }

                    // Jetzt wirklich absenden (native, ohne Loop)
                    form.submit();

                    // Optional: Fallback-Timeout (z. B. 15 Sekunden), falls Server hängt
                    setTimeout(() => {
                        $('#loadingOverlay').css('display', 'none');
                        $submitBtn.prop('disabled', false).html('Absenden');
                    }, 15000);
                };
            }
        });        
    }, 50);
});

// Sehr wichtig: Klick merken, BEVOR Validate losläuft
$('form').on('click', 'button[type="submit"], input[type="submit"]', function (e) {
    const $this = $(this);
    const $form = $this.closest('form');

    // Original-Text merken (falls du ihn später brauchst)
    if (!$this.data('originalText')) {
        $this.data('originalText', $this.html() || $this.val());
    }

    // Button merken
    $form.data('lastClickedSubmitButton', $this);

    // Optional: formaction schon hier setzen (manchmal hilft es)
    const fa = $this.attr('formaction');
    if (fa) {
        $form.attr('action', fa);
    }
});

$(function() {
    $('a[data-loading="true"]').on('click', function(e) {
        // e.preventDefault();   // nur auskommentieren, wenn du AJAX machst und keine normale Navigation willst

        $('#loadingOverlay').css('display', 'flex');

        // Optional: Link visuell "busy" machen
        $(this)
            .attr('aria-busy', 'true')
            .css({
                opacity: '0.6',
                cursor: 'wait'
            });
    });
});