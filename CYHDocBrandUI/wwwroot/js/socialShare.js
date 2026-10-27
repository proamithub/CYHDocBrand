window.cyhSocial = {
    openWhatsApp: function (phone, text) {
        let cleanPhone = phone ? phone.replace(/[^0-9]/g, '') : '';
        let encodedText = encodeURIComponent(text || '');
        let url = cleanPhone 
            ? `https://api.whatsapp.com/send?phone=${cleanPhone}&text=${encodedText}`
            : `https://api.whatsapp.com/send?text=${encodedText}`;
        window.open(url, '_blank');
    },

    openFacebookShare: function (url, quote) {
        let targetUrl = url || window.location.origin;
        let shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(targetUrl)}&quote=${encodeURIComponent(quote || '')}`;
        window.open(shareUrl, 'fb_share', 'width=650,height=550,menubar=no,toolbar=no,resizable=yes');
    },

    openLinkedInShare: function (url) {
        let targetUrl = url || window.location.origin;
        let shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(targetUrl)}`;
        window.open(shareUrl, 'li_share', 'width=650,height=550,menubar=no,toolbar=no,resizable=yes');
    },

    openTwitterShare: function (text, url) {
        let targetUrl = url || '';
        let shareUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text || '')}&url=${encodeURIComponent(targetUrl)}`;
        window.open(shareUrl, 'tw_share', 'width=600,height=450,menubar=no,toolbar=no,resizable=yes');
    },

    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (e) {
            let ta = document.createElement('textarea');
            ta.value = text;
            document.body.appendChild(ta);
            ta.select();
            document.execCommand('copy');
            document.body.removeChild(ta);
            return true;
        }
    },

    requestNotificationPermission: function () {
        if ('Notification' in window && Notification.permission === 'default') {
            Notification.requestPermission();
        }
    },

    showDesktopNotification: function (title, body, iconUrl) {
        if ('Notification' in window && Notification.permission === 'granted') {
            try {
                let notif = new Notification(title, {
                    body: body,
                    icon: iconUrl || '/favicon.png',
                    badge: '/favicon.png',
                    tag: 'cyh-schedule-alert'
                });
                notif.onclick = function () {
                    window.focus();
                    this.close();
                };
            } catch (e) { }
        }
    }
};
