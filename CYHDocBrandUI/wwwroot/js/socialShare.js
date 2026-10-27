// CYH Doctor Brand - 100% Without-API Social Sharing Engine
window.cyhSocial = {
    // 1. WhatsApp 1-Click (WhatsApp Web or Mobile WhatsApp App)
    openWhatsApp: function (phone, text) {
        let cleanPhone = phone ? phone.replace(/[^0-9]/g, '') : '';
        let encodedText = encodeURIComponent(text || '');
        let url = cleanPhone 
            ? `https://api.whatsapp.com/send?phone=${cleanPhone}&text=${encodedText}`
            : `https://api.whatsapp.com/send?text=${encodedText}`;
        window.open(url, '_blank');
    },

    // 2. Facebook 1-Click Sharer Popup (No API / No Developer Key needed)
    openFacebookShare: function (url, quote) {
        let targetUrl = url || window.location.href;
        let shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(targetUrl)}&quote=${encodeURIComponent(quote || '')}`;
        window.open(shareUrl, 'fb_share', 'width=650,height=550,menubar=no,toolbar=no,resizable=yes');
    },

    // 3. Instagram 1-Click (Copies Caption + Downloads Image + Opens Instagram)
    openInstagram: async function (caption, imageUrl) {
        if (caption) {
            await window.cyhSocial.copyToClipboard(caption);
        }
        if (imageUrl) {
            window.cyhSocial.downloadImage(imageUrl, 'CYH_Banner.png');
        }
        window.open('https://www.instagram.com/', '_blank');
    },

    // 4. LinkedIn 1-Click Share (No API needed)
    openLinkedInShare: function (url) {
        let targetUrl = url || window.location.href;
        let shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(targetUrl)}`;
        window.open(shareUrl, 'li_share', 'width=650,height=550,menubar=no,toolbar=no,resizable=yes');
    },

    // 5. Twitter / X 1-Click Share (No API needed)
    openTwitterShare: function (text, url) {
        let targetUrl = url || '';
        let shareUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text || '')}&url=${encodeURIComponent(targetUrl)}`;
        window.open(shareUrl, 'tw_share', 'width=600,height=450,menubar=no,toolbar=no,resizable=yes');
    },

    // 6. Native Device Share (Mobile Android/iOS/Windows share sheet)
    nativeShare: async function (title, text, url) {
        if (navigator.share) {
            try {
                await navigator.share({
                    title: title || 'Medical Banner',
                    text: text || '',
                    url: url || window.location.href
                });
                return true;
            } catch (err) {
                console.log('Native share canceled:', err);
            }
        }
        // Fallback to WhatsApp
        window.cyhSocial.openWhatsApp('', (text ? text + '\n\n' : '') + (url || ''));
        return true;
    },

    // Helper: Download Image directly
    downloadImage: function (url, filename) {
        let a = document.createElement('a');
        a.href = url;
        a.download = filename || 'Banner.png';
        a.target = '_blank';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    },

    // Helper: Robust Clipboard copy
    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (e) {
            let ta = document.createElement('textarea');
            ta.value = text;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.select();
            document.execCommand('copy');
            document.body.removeChild(ta);
            return true;
        }
    },

    // Desktop Notification Setup
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
