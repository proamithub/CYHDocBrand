// CYH Doctor Brand - PWA Registration & Install Handler
(function () {
    let deferredPrompt = null;

    function setButtonsVisibility(visible) {
        const buttons = document.querySelectorAll('.cyh-pwa-install-btn');
        buttons.forEach(btn => {
            btn.style.display = visible ? 'inline-flex' : 'none';
        });
    }

    window.cyhPwa = {
        canInstall: false,
        
        init: function () {
            if ('serviceWorker' in navigator) {
                window.addEventListener('load', () => {
                    navigator.serviceWorker.register('/service-worker.js')
                        .then((reg) => {
                            console.log('CYH PWA ServiceWorker registered with scope:', reg.scope);
                        })
                        .catch((err) => {
                            console.warn('CYH PWA ServiceWorker registration failed:', err);
                        });
                });
            }

            window.addEventListener('beforeinstallprompt', (e) => {
                // Prevent mini-infobar from appearing on mobile
                e.preventDefault();
                deferredPrompt = e;
                window.cyhPwa.canInstall = true;

                // Dispatch custom event for UI components
                window.dispatchEvent(new CustomEvent('cyh-pwa-installable'));
                
                if (!window.cyhPwa.isStandalone()) {
                    setButtonsVisibility(true);
                }
            });

            window.addEventListener('appinstalled', () => {
                deferredPrompt = null;
                window.cyhPwa.canInstall = false;
                setButtonsVisibility(false);
                console.log('CYH Doctor Brand App was installed successfully!');
            });

            // Initial check on DOM load
            document.addEventListener('DOMContentLoaded', () => {
                if (window.cyhPwa.isStandalone()) {
                    setButtonsVisibility(false);
                }
            });
        },

        promptInstall: async function () {
            if (deferredPrompt) {
                deferredPrompt.prompt();
                const { outcome } = await deferredPrompt.userChoice;
                deferredPrompt = null;
                window.cyhPwa.canInstall = false;
                if (outcome === 'accepted') {
                    setButtonsVisibility(false);
                }
                return { success: outcome === 'accepted', outcome: outcome };
            }

            // Check iOS Safari
            const isIos = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
            if (isIos) {
                alert("iPhone / iPad par App install karne ke liye:\n1. Neeche Safari ke 'Share' (↑) icon par tap karein.\n2. List me 'Add to Home Screen' (+) select karein.");
                return { success: false, reason: 'ios_safari' };
            }

            alert("Browser menu (top-right 3 dots) par click karke 'Install App' ya 'Add to Phone' select karein.");
            return { success: false, reason: 'manual_instructions' };
        },

        isStandalone: function () {
            return (window.matchMedia('(display-mode: standalone)').matches) || 
                   (window.navigator.standalone === true);
        }
    };

    window.cyhPwa.init();
})();
