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

    // Global Project Cache Reset Functionality for Admin
    window.cyhResetProjectCache = async function () {
        let result = {
            cachesCleared: 0,
            swUnregistered: false,
            storageItemsCleared: 0
        };

        // 1. Purge CacheStorage (All Service Worker static caches)
        if ('caches' in window) {
            try {
                const cacheNames = await caches.keys();
                for (const name of cacheNames) {
                    await caches.delete(name);
                    result.cachesCleared++;
                }
            } catch (e) {
                console.warn('CacheStorage purge warning:', e);
            }
        }

        // 2. Unregister Active Service Workers
        if ('serviceWorker' in navigator) {
            try {
                const registrations = await navigator.serviceWorker.getRegistrations();
                for (const reg of registrations) {
                    await reg.unregister();
                    result.swUnregistered = true;
                }
            } catch (e) {
                console.warn('ServiceWorker unregister warning:', e);
            }
        }

        // 3. Purge sessionStorage
        try {
            sessionStorage.clear();
        } catch (e) {
            console.warn('SessionStorage clear warning:', e);
        }

        // 4. Purge cached items in localStorage while preserving essentials
        try {
            const keysToRemove = [];
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && (key.includes('cache') || key.includes('template') || key.includes('banner_temp') || key.includes('cyh_cache'))) {
                    keysToRemove.push(key);
                }
            }
            keysToRemove.forEach(k => {
                localStorage.removeItem(k);
                result.storageItemsCleared++;
            });
        } catch (e) {
            console.warn('LocalStorage clear warning:', e);
        }

        return result;
    };

    window.cyhHardReload = function () {
        window.location.reload();
    };

    window.cyhPwa.init();
})();
