// CYH Doctor Brand - Progressive Web App (PWA) Service Worker
const CACHE_NAME = 'cyh-pwa-cache-v1.0';

const PRECACHE_ASSETS = [
    '/',
    '/app.css',
    '/css/glass-theme.css',
    '/favicon.svg',
    '/favicon.png',
    '/images/branding/cyh-logo.svg',
    '/images/branding/icon-192.png',
    '/images/branding/icon-512.png',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/js/bannerStudio.js'
];

// Install: Cache critical shell assets
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(PRECACHE_ASSETS).catch((err) => {
                console.warn('PWA Precache warning (some assets may load on demand):', err);
            });
        }).then(() => self.skipWaiting())
    );
});

// Activate: Clean up old caches
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) => {
            return Promise.all(
                keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key))
            );
        }).then(() => self.clients.claim())
    );
});

// Fetch: Network-first for navigation & Blazor SignalR, Cache-first for static assets
self.addEventListener('fetch', (event) => {
    const req = event.request;
    const url = new URL(req.url);

    // Bypass SignalR Blazor websocket negotiations and API POST calls
    if (req.method !== 'GET' || url.pathname.includes('/_blazor') || url.pathname.includes('/api/')) {
        return;
    }

    // For HTML navigation requests, use Network First, fallback to cache
    if (req.mode === 'navigate') {
        event.respondWith(
            fetch(req)
                .then((networkResponse) => {
                    return networkResponse;
                })
                .catch(async () => {
                    const cache = await caches.open(CACHE_NAME);
                    const cachedResponse = await cache.match('/');
                    return cachedResponse || new Response('App Offline. Please connect to internet.', {
                        headers: { 'Content-Type': 'text/html' }
                    });
                })
        );
        return;
    }

    // For static images, CSS, fonts, and scripts: Stale-While-Revalidate
    if (url.origin === self.location.origin) {
        event.respondWith(
            caches.match(req).then((cachedResponse) => {
                const fetchPromise = fetch(req).then((networkResponse) => {
                    if (networkResponse && networkResponse.status === 200) {
                        const responseToCache = networkResponse.clone();
                        caches.open(CACHE_NAME).then((cache) => {
                            cache.put(req, responseToCache);
                        });
                    }
                    return networkResponse;
                }).catch(() => cachedResponse);

                return cachedResponse || fetchPromise;
            })
        );
    }
});


// ==============================================================================
// Web Push Notifications: Works even when browser is completely CLOSED!
// ==============================================================================
self.addEventListener('push', function (event) {
    if (!event.data) {
        console.log('[SW] Push received with no payload');
        return;
    }

    try {
        const payload = event.data.json();
        const title = payload.title || '⏰ Scheduled Post Reminder';
        const options = {
            body: payload.body || 'Aapka scheduled post ready hai!',
            icon: payload.icon || '/images/branding/icon-192.png',
            badge: payload.badge || '/favicon.png',
            image: payload.image || undefined,
            tag: payload.tag || 'cyh-banner-schedule',
            requireInteraction: true,
            vibrate: [200, 100, 200, 100, 200],
            data: payload.data || {},
            actions: [
                {
                    action: 'open-whatsapp',
                    title: '🟢 Send on WhatsApp',
                    icon: '/favicon.png'
                },
                {
                    action: 'open-app',
                    title: '📱 View Post',
                    icon: '/favicon.png'
                }
            ]
        };

        event.waitUntil(self.registration.showNotification(title, options));
    } catch (e) {
        console.error('[SW] Error parsing push payload:', e);
    }
});

// Click on Notification (when user taps the notification or action buttons)
self.addEventListener('notificationclick', function (event) {
    event.notification.close();

    const data = event.notification.data || {};
    const action = event.action;

    let targetUrl = '/doctor/schedules';

    if (action === 'open-whatsapp' && data.whatsappUrl) {
        targetUrl = data.whatsappUrl;
    } else if (data.actionUrl) {
        targetUrl = data.actionUrl;
    }

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(function (clientList) {
            for (let i = 0; i < clientList.length; i++) {
                const client = clientList[i];
                if ('focus' in client && client.url.includes(self.location.origin)) {
                    if (action === 'open-whatsapp' && data.whatsappUrl) {
                        return clients.openWindow(data.whatsappUrl);
                    }
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow(targetUrl);
            }
        })
    );
});
