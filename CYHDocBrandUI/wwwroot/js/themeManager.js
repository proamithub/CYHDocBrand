/**
 * CYH Doctor Brand - User Theme Manager
 * Supports: Glass (Frosted Glassmorphism), Dark (High Contrast Obsidian), Light (Clean Daylight Slate)
 * Dynamically swaps the <link id="app-theme-css"> stylesheet and saves per user in localStorage.
 */
(function (window) {
    'use strict';

    function getThemeHref(themeName) {
        if (window.CYH_ASSETS && window.CYH_ASSETS.themes && window.CYH_ASSETS.themes[themeName]) {
            return window.CYH_ASSETS.themes[themeName];
        }
        var baseFiles = {
            'glass': 'css/glass-theme.css',
            'dark': 'css/dark-theme.css',
            'light': 'css/light-theme.css'
        };
        var file = baseFiles[themeName] || baseFiles['glass'];
        var v = (window.CYH_ASSETS && window.CYH_ASSETS.build) ? window.CYH_ASSETS.build : Date.now();
        return file + '?v=' + v;
    }

    var THEME_FILES = {
        'glass': 'css/glass-theme.css',
        'dark': 'css/dark-theme.css',
        'light': 'css/light-theme.css'
    };

    var ThemeManager = {
        getValidTheme: function (theme) {
            if (theme && THEME_FILES.hasOwnProperty(theme)) {
                return theme;
            }
            return 'glass';
        },

        getCurrentTheme: function (userId) {
            try {
                if (!userId) {
                    userId = localStorage.getItem('cyh_active_user_id');
                }
                if (userId) {
                    var userTheme = localStorage.getItem('cyh_theme_' + userId);
                    if (userTheme && THEME_FILES.hasOwnProperty(userTheme)) {
                        return userTheme;
                    }
                }
                var globalTheme = localStorage.getItem('cyh_theme');
                if (globalTheme && THEME_FILES.hasOwnProperty(globalTheme)) {
                    return globalTheme;
                }
            } catch (e) {
                console.warn('[ThemeManager] Storage access error:', e);
            }
            return 'glass';
        },

        setTheme: function (themeName, userId) {
            var validTheme = this.getValidTheme(themeName);
            var cssHref = getThemeHref(validTheme);

            // 1. Swap the active stylesheet link
            var themeLink = document.getElementById('app-theme-css');
            if (themeLink) {
                // Check if already pointing to the exact versioned theme to avoid unnecessary reload
                var currentHref = themeLink.getAttribute('href') || '';
                if (currentHref !== cssHref) {
                    themeLink.setAttribute('href', cssHref);
                }
            } else {
                // If link tag doesn't exist yet, create it
                var newLink = document.createElement('link');
                newLink.id = 'app-theme-css';
                newLink.rel = 'stylesheet';
                newLink.href = cssHref;
                document.head.appendChild(newLink);
            }

            // 2. Set data attributes on html and body
            document.documentElement.setAttribute('data-theme', validTheme);
            if (document.body) {
                document.body.setAttribute('data-theme', validTheme);
            }

            // 3. Update PWA meta theme-color
            var metaThemeColor = document.querySelector('meta[name="theme-color"]');
            if (metaThemeColor) {
                if (validTheme === 'light') {
                    metaThemeColor.setAttribute('content', '#f1f5f9');
                } else if (validTheme === 'dark') {
                    metaThemeColor.setAttribute('content', '#0b0f19');
                } else {
                    metaThemeColor.setAttribute('content', '#070d1e');
                }
            }

            // 4. Persist to localStorage
            try {
                localStorage.setItem('cyh_theme', validTheme);
                if (userId) {
                    localStorage.setItem('cyh_theme_' + userId, validTheme);
                } else {
                    var activeUserId = localStorage.getItem('cyh_active_user_id');
                    if (activeUserId) {
                        localStorage.setItem('cyh_theme_' + activeUserId, validTheme);
                    }
                }
            } catch (e) {
                console.warn('[ThemeManager] Could not save theme preference:', e);
            }

            // 5. Dispatch event for any other JS listeners
            try {
                window.dispatchEvent(new CustomEvent('cyh-theme-changed', {
                    detail: { theme: validTheme, userId: userId }
                }));
            } catch (e) {}

            return validTheme;
        },

        syncUserTheme: function (userId) {
            if (!userId) {
                userId = localStorage.getItem('cyh_active_user_id');
            }
            if (!userId) {
                return this.setTheme(this.getCurrentTheme(), null);
            }

            try {
                var userTheme = localStorage.getItem('cyh_theme_' + userId);
                if (userTheme && THEME_FILES.hasOwnProperty(userTheme)) {
                    return this.setTheme(userTheme, userId);
                } else {
                    // No theme chosen for this user yet; take the current global theme and bind to user
                    var current = this.getCurrentTheme(null);
                    localStorage.setItem('cyh_theme_' + userId, current);
                    return this.setTheme(current, userId);
                }
            } catch (e) {
                return this.setTheme('glass', userId);
            }
        }
    };

    window.cyhTheme = ThemeManager;

    // Immediately run on script load to ensure stylesheet is synchronized
    try {
        var initialTheme = ThemeManager.getCurrentTheme();
        ThemeManager.setTheme(initialTheme);
    } catch (e) {}

})(window);
