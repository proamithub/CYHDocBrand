/**
 * CYH DoctorHub - AI Banner Studio JavaScript Interop
 * Features:
 * - High-DPI Canvas Rendering & Compositing
 * - Interactive Mouse Drag & Drop (Heading, Subtitle, Doctor Card, Badges, Overlay Images)
 * - Multiple Overlay Images Support (Doctor Photo, Stickers, Logos)
 * - Strict Lock in Published Mode (Zero Edits / Zero Drag)
 * - PNG Download & Native Social Media Sharing
 */

window.bannerStudio = window.bannerStudio || {
    cachedImages: {},
    renderTimeout: null,
    dotNetRef: null,
    currentState: null,
    currentCanvasId: null,

    // Drag State
    elements: [],
    draggingElement: null,
    dragOffsetX: 0,
    dragOffsetY: 0,
    attachedCanvas: null,
    _cleanupListeners: null,

    isInitialized: function () {
        return true;
    },

    init: function (canvasId, dotNetHelper) {
        this.dotNetRef = dotNetHelper;
        this.currentCanvasId = canvasId;
        this.attachMouseListeners(canvasId);
    },

    loadImage: function (src) {
        return new Promise((resolve) => {
            if (!src) return resolve(null);
            if (this.cachedImages[src]) return resolve(this.cachedImages[src]);

            const cachedKeys = Object.keys(this.cachedImages);
            if (cachedKeys.length > 40) {
                for (let i = 0; i < 20; i++) {
                    delete this.cachedImages[cachedKeys[i]];
                }
            }

            const img = new Image();
            if (src.startsWith('http://') || src.startsWith('https://')) {
                try {
                    const parsedUrl = new URL(src, window.location.href);
                    if (parsedUrl.origin !== window.location.origin) {
                        img.crossOrigin = "anonymous";
                    }
                } catch {
                    img.crossOrigin = "anonymous";
                }
            }
            img.onload = () => {
                this.cachedImages[src] = img;
                resolve(img);
            };
            img.onerror = (err) => {
                if (img.crossOrigin) {
                    const fallbackImg = new Image();
                    fallbackImg.onload = () => {
                        this.cachedImages[src] = fallbackImg;
                        resolve(fallbackImg);
                    };
                    fallbackImg.onerror = () => {
                        console.warn("Failed to load image for banner canvas:", src);
                        resolve(null);
                    };
                    fallbackImg.src = src;
                } else {
                    console.warn("Failed to load image for banner canvas:", src);
                    resolve(null);
                }
            };
            img.src = src;
        });
    },

    getDimensions: function (aspectRatio) {
        switch (aspectRatio) {
            case "4:5":
                return { width: 1080, height: 1350 };
            case "9:16":
                return { width: 1080, height: 1920 };
            case "16:9":
                return { width: 1920, height: 1080 };
            case "1:1":
            default:
                return { width: 1080, height: 1080 };
        }
    },

    getFontStack: function (customFont, isHeading = false) {
        const devanagariFallbacks = "'Noto Sans Devanagari', 'Poppins', 'Mukta', 'Hind', 'Segoe UI', -apple-system, sans-serif";
        if (!customFont || customFont === 'inherit') {
            return isHeading ? `'Poppins', ${devanagariFallbacks}` : `'Noto Sans Devanagari', 'Poppins', ${devanagariFallbacks}`;
        }
        if (customFont.includes(',')) {
            return customFont;
        }
        return `"${customFont}", ${devanagariFallbacks}`;
    },

    renderBanner: function (canvasId, state) {
        if (!state) return Promise.resolve();

        if (canvasId === 'bannerCanvas') {
            this.currentState = state;
            this.currentCanvasId = canvasId;
            this.attachMouseListeners(canvasId);
        }

        if (this.renderTimeout) {
            cancelAnimationFrame(this.renderTimeout);
        }

        return new Promise((resolve) => {
            this.renderTimeout = requestAnimationFrame(async () => {
                await this._executeRender(canvasId, state);
                resolve();
            });
        });
    },

    _executeRender: async function (canvasId, state) {
        let canvas = document.getElementById(canvasId);
        if (!canvas) {
            await new Promise(r => setTimeout(r, 60));
            canvas = document.getElementById(canvasId);
            if (!canvas) return;
        }

        // Ensure English & Hindi web fonts are completely loaded before measuring/rendering
        if (document.fonts && document.fonts.ready) {
            try {
                await document.fonts.ready;
            } catch (fontErr) {
                // Ignore font loading errors
            }
        }

        // Preload any uncached images in parallel
        const preloadTasks = [];
        if (state.backgroundImageUrl && !this.cachedImages[state.backgroundImageUrl]) {
            preloadTasks.push(this.loadImage(state.backgroundImageUrl));
        }
        const logoUrl = state.logoUrl || "images/branding/cyh-logo.svg";
        if (logoUrl && !this.cachedImages[logoUrl]) {
            preloadTasks.push(this.loadImage(logoUrl));
        }
        if (state.overlayImages) {
            for (let i = 0; i < state.overlayImages.length; i++) {
                const ov = state.overlayImages[i];
                if (ov && ov.imageUrl && !this.cachedImages[ov.imageUrl]) {
                    preloadTasks.push(this.loadImage(ov.imageUrl));
                }
            }
        }
        if (preloadTasks.length > 0) {
            await Promise.all(preloadTasks);
        }

        this.elements = []; // Reset hit test elements

        const ctx = canvas.getContext('2d', { alpha: false });
        const dims = this.getDimensions(state.aspectRatio);

        if (canvas.width !== dims.width || canvas.height !== dims.height) {
            canvas.width = dims.width;
            canvas.height = dims.height;
        }

        const W = dims.width;
        const H = dims.height;
        const isPublished = (state.status === "Published");

        // 1. Draw Background (AI Image or Medical Gradient)
        let bgImg = null;
        if (state.backgroundImageUrl) {
            bgImg = this.cachedImages[state.backgroundImageUrl] || null;
        }

        if (bgImg) {
            const imgRatio = bgImg.width / bgImg.height;
            const canvasRatio = W / H;
            let dw, dh, dx, dy;
            if (imgRatio > canvasRatio) {
                dh = H;
                dw = H * imgRatio;
                dx = (W - dw) / 2;
                dy = 0;
            } else {
                dw = W;
                dh = W / imgRatio;
                dx = 0;
                dy = (H - dh) / 2;
            }
            ctx.drawImage(bgImg, dx, dy, dw, dh);

            // Dark vignette overlay
            const overlayGrad = ctx.createLinearGradient(0, 0, 0, H);
            overlayGrad.addColorStop(0, 'rgba(7, 13, 30, 0.78)');
            overlayGrad.addColorStop(0.35, 'rgba(7, 13, 30, 0.45)');
            overlayGrad.addColorStop(0.70, 'rgba(7, 13, 30, 0.85)');
            overlayGrad.addColorStop(1, 'rgba(7, 13, 30, 0.96)');
            ctx.fillStyle = overlayGrad;
            ctx.fillRect(0, 0, W, H);
        } else {
            const grad = ctx.createRadialGradient(W * 0.2, H * 0.2, 50, W * 0.5, H * 0.5, W);
            grad.addColorStop(0, '#102a45');
            grad.addColorStop(0.5, '#0b162c');
            grad.addColorStop(1, '#050a17');
            ctx.fillStyle = grad;
            ctx.fillRect(0, 0, W, H);

            ctx.save();
            ctx.fillStyle = 'rgba(0, 242, 254, 0.06)';
            ctx.beginPath();
            ctx.arc(W * 0.85, H * 0.2, W * 0.35, 0, Math.PI * 2);
            ctx.fill();

            ctx.fillStyle = 'rgba(79, 172, 254, 0.05)';
            ctx.beginPath();
            ctx.arc(W * 0.15, H * 0.7, W * 0.3, 0, Math.PI * 2);
            ctx.fill();
            ctx.restore();
        }

        const padding = Math.round(W * 0.065);
        let defaultY = padding;

        // 2. Category Badge (Interactive Position or Default)
        if (state.badgeText) {
            ctx.save();
            const badgeFontFamily = this.getFontStack(state.fontFamily || state.FontFamily, false);
            ctx.font = `700 ${Math.round(W * 0.022)}px ${badgeFontFamily}`;
            const badgeTxt = state.badgeText.toUpperCase();
            const badgeMetrics = ctx.measureText(badgeTxt);
            const bWidth = badgeMetrics.width + 36;
            const bHeight = Math.round(W * 0.045);

            const bx = (state.badgeX !== null && state.badgeX !== undefined) ? state.badgeX : (state.BadgeX !== null && state.BadgeX !== undefined ? state.BadgeX : padding);
            const by = (state.badgeY !== null && state.badgeY !== undefined) ? state.badgeY : (state.BadgeY !== null && state.BadgeY !== undefined ? state.BadgeY : defaultY);

            ctx.fillStyle = 'rgba(0, 242, 254, 0.15)';
            ctx.strokeStyle = state.primaryColor || '#00f2fe';
            ctx.lineWidth = 2;
            this.drawRoundedRect(ctx, bx, by, bWidth, bHeight, bHeight / 2);
            ctx.fill();
            ctx.stroke();

            ctx.fillStyle = state.primaryColor || '#00f2fe';
            ctx.textBaseline = 'middle';
            ctx.fillText(badgeTxt, bx + 18, by + bHeight / 2);
            ctx.restore();

            this.registerHitElement('badge', bx, by, bWidth, bHeight, true);

            if (state.badgeY === null || state.badgeY === undefined) {
                defaultY = by + bHeight + 24;
            }
        }

        // 3. Hospital Name Header (Interactive or Default)
        if (state.hospitalName) {
            ctx.save();
            const fontSize = Math.round(W * 0.028);
            const hospFontFamily = this.getFontStack(state.headingFontFamily || state.HeadingFontFamily || state.fontFamily || state.FontFamily, true);
            ctx.font = `600 ${fontSize}px ${hospFontFamily}`;
            const hospMetrics = ctx.measureText(state.hospitalName.toUpperCase());
            const hw = hospMetrics.width;
            const hh = fontSize * 1.3;

            const hx = (state.hospitalNameX !== null && state.hospitalNameX !== undefined) ? state.hospitalNameX : (state.HospitalNameX !== null && state.HospitalNameX !== undefined ? state.HospitalNameX : padding);
            const hy = (state.hospitalNameY !== null && state.hospitalNameY !== undefined) ? state.hospitalNameY : (state.HospitalNameY !== null && state.HospitalNameY !== undefined ? state.HospitalNameY : defaultY);

            ctx.fillStyle = 'rgba(255, 255, 255, 0.85)';
            ctx.textBaseline = 'top';
            ctx.fillText(state.hospitalName.toUpperCase(), hx, hy);
            ctx.restore();

            this.registerHitElement('hospitalName', hx, hy, hw, hh, true);

            if (state.hospitalNameY === null || state.hospitalNameY === undefined) {
                defaultY = hy + hh + 20;
            }
        }

        // 4. Main Heading (Interactive or Default)
        ctx.save();
        const headingFontSize = (state.headingFontSize || state.HeadingFontSize) ? Math.round(state.headingFontSize || state.HeadingFontSize) : Math.round(W * 0.065);
        const headingFontFamily = this.getFontStack(state.headingFontFamily || state.HeadingFontFamily || state.fontFamily || state.FontFamily, true);
        ctx.font = `800 ${headingFontSize}px ${headingFontFamily}`;
        ctx.fillStyle = state.textColor || '#ffffff';
        ctx.textBaseline = 'top';
        const headingLineHeight = Math.round(headingFontSize * 1.25);
        const maxTextWidth = W - (padding * 2);

        const headingLines = this.wrapText(ctx, state.heading || "Specialist Health Consultation", maxTextWidth);
        const headingHeight = headingLines.length * headingLineHeight;

        const headX = (state.headingX !== null && state.headingX !== undefined) ? state.headingX : (state.HeadingX !== null && state.HeadingX !== undefined ? state.HeadingX : padding);
        const headY = (state.headingY !== null && state.headingY !== undefined) ? state.headingY : (state.HeadingY !== null && state.HeadingY !== undefined ? state.HeadingY : defaultY);

        let curHeadY = headY;
        let actualHeadWidth = 0;
        for (const line of headingLines) {
            ctx.fillText(line, headX, curHeadY);
            const lw = ctx.measureText(line).width;
            if (lw > actualHeadWidth) actualHeadWidth = lw;
            curHeadY += headingLineHeight;
        }
        ctx.restore();

        actualHeadWidth = Math.min(maxTextWidth, Math.max(120, actualHeadWidth + 30));
        this.registerHitElement('heading', headX, headY, actualHeadWidth, headingHeight, true);

        if (state.headingY === null || state.headingY === undefined) {
            defaultY = headY + headingHeight + 16;
        }

        // 5. Subtitle (Interactive or Default)
        if (state.subtitle) {
            ctx.save();
            const subFontSize = (state.subtitleFontSize || state.SubtitleFontSize) ? Math.round(state.subtitleFontSize || state.SubtitleFontSize) : Math.round(W * 0.032);
            const subFontWeight = state.subtitleFontWeight || state.SubtitleFontWeight || '400';
            const subFontFamily = this.getFontStack(state.subtitleFontFamily || state.SubtitleFontFamily || state.fontFamily || state.FontFamily, false);
            ctx.font = `${subFontWeight} ${subFontSize}px ${subFontFamily}`;
            ctx.fillStyle = 'rgba(203, 213, 225, 0.9)';
            ctx.textBaseline = 'top';
            const subLineHeight = (state.subtitleLineHeight || state.SubtitleLineHeight) ? Math.round(state.subtitleLineHeight || state.SubtitleLineHeight) : Math.round(subFontSize * 1.4);
            const subLines = this.wrapText(ctx, state.subtitle, maxTextWidth);
            const subTotalHeight = subLines.length * subLineHeight;

            const subX = (state.subtitleX !== null && state.subtitleX !== undefined) ? state.subtitleX : (state.SubtitleX !== null && state.SubtitleX !== undefined ? state.SubtitleX : padding);
            const subY = (state.subtitleY !== null && state.subtitleY !== undefined) ? state.subtitleY : (state.SubtitleY !== null && state.SubtitleY !== undefined ? state.SubtitleY : defaultY);

            let curSubY = subY;
            let actualSubWidth = 0;
            for (const line of subLines.slice(0, 3)) {
                ctx.fillText(line, subX, curSubY);
                const sw = ctx.measureText(line).width;
                if (sw > actualSubWidth) actualSubWidth = sw;
                curSubY += subLineHeight;
            }
            ctx.restore();

            actualSubWidth = Math.min(maxTextWidth, Math.max(100, actualSubWidth + 25));
            this.registerHitElement('subtitle', subX, subY, actualSubWidth, subTotalHeight, true);
        }

        // 6. MULTIPLE OVERLAY IMAGES (Doctor Photo, Stickers, Badges)
        if (state.overlayImages && state.overlayImages.length > 0) {
            for (let i = 0; i < state.overlayImages.length; i++) {
                const imgData = state.overlayImages[i];
                if (!imgData.isVisible || !imgData.imageUrl) continue;

                const oImg = this.cachedImages[imgData.imageUrl] || await this.loadImage(imgData.imageUrl);
                if (oImg) {
                    const ox = imgData.x !== undefined ? imgData.x : 100 + (i * 40);
                    const oy = imgData.y !== undefined ? imgData.y : 450;
                    const ow = imgData.width || 180;
                    const oh = imgData.height || 180;

                    ctx.save();
                    if (imgData.isCircle) {
                        // Circular clip with glowing ring
                        ctx.beginPath();
                        const cx = ox + (ow / 2);
                        const cy = oy + (oh / 2);
                        const r = Math.min(ow, oh) / 2;
                        ctx.arc(cx, cy, r, 0, Math.PI * 2);
                        ctx.closePath();
                        ctx.clip();
                        ctx.drawImage(oImg, ox, oy, ow, oh);
                        ctx.restore();

                        // Border ring
                        ctx.save();
                        ctx.strokeStyle = state.primaryColor || '#00f2fe';
                        ctx.lineWidth = 4;
                        ctx.beginPath();
                        ctx.arc(cx, cy, r, 0, Math.PI * 2);
                        ctx.stroke();
                        ctx.restore();
                    } else {
                        // Rounded Rect
                        this.drawRoundedRect(ctx, ox, oy, ow, oh, 16);
                        ctx.clip();
                        ctx.drawImage(oImg, ox, oy, ow, oh);
                        ctx.restore();

                        ctx.save();
                        ctx.strokeStyle = 'rgba(255, 255, 255, 0.2)';
                        ctx.lineWidth = 2;
                        this.drawRoundedRect(ctx, ox, oy, ow, oh, 16);
                        ctx.stroke();
                        ctx.restore();
                    }

                    // Register for mouse drag
                    this.registerHitElement('overlay_' + imgData.id, ox, oy, ow, oh, true, imgData);
                }
            }
        }

        // 7. Doctor Info Card (Interactive or Default)
        const cardScale = state.doctorCardScale || state.DoctorCardScale || 1.0;
        const defaultCardWidth = maxTextWidth;
        const doctorCardWidth = (state.doctorCardWidth || state.DoctorCardWidth) ? Math.round(state.doctorCardWidth || state.DoctorCardWidth) : Math.round(defaultCardWidth * cardScale);
        const defaultCardHeight = Math.round(H * 0.13);
        const doctorCardHeight = (state.doctorCardHeight || state.DoctorCardHeight) ? Math.round(state.doctorCardHeight || state.DoctorCardHeight) : Math.round(defaultCardHeight * cardScale);
        const cardRadius = state.doctorCardBorderRadius || state.DoctorCardBorderRadius || 20;
        const cardBg = state.doctorCardBgColor || state.DoctorCardBgColor || 'rgba(15, 23, 42, 0.85)';
        const cardBorder = state.doctorCardBorderColor || state.DoctorCardBorderColor || 'rgba(255, 255, 255, 0.15)';
        const cardPadding = state.doctorCardPadding || state.DoctorCardPadding || 20;
        const docFontFamily = this.getFontStack(state.doctorCardFontFamily || state.DoctorCardFontFamily || state.fontFamily || state.FontFamily, false);
        const nameFontSize = (state.doctorNameFontSize || state.DoctorNameFontSize) ? Math.round(state.doctorNameFontSize || state.DoctorNameFontSize) : Math.round(34 * cardScale);
        const specFontSize = (state.doctorSpecFontSize || state.DoctorSpecFontSize) ? Math.round(state.doctorSpecFontSize || state.DoctorSpecFontSize) : Math.round(22 * cardScale);
        const docX = (state.doctorCardX !== null && state.doctorCardX !== undefined) ? state.doctorCardX : (state.DoctorCardX !== null && state.DoctorCardX !== undefined ? state.DoctorCardX : padding);
        const docY = (state.doctorCardY !== null && state.doctorCardY !== undefined) ? state.doctorCardY : (state.DoctorCardY !== null && state.DoctorCardY !== undefined ? state.DoctorCardY : (H - (H * 0.32)));

        ctx.save();
        ctx.fillStyle = cardBg;
        ctx.strokeStyle = cardBorder;
        ctx.lineWidth = 2;
        this.drawRoundedRect(ctx, docX, docY, doctorCardWidth, doctorCardHeight, cardRadius);
        ctx.fill();
        ctx.stroke();

        // Accent indicator bar
        if (state.showDoctorCardAccentBar !== false && state.ShowDoctorCardAccentBar !== false) {
            ctx.fillStyle = state.primaryColor || '#00f2fe';
            this.drawRoundedRect(ctx, docX + 16, docY + 16, 6, doctorCardHeight - 32, 3);
            ctx.fill();
        }

        ctx.font = `800 ${nameFontSize}px ${docFontFamily}`;
        ctx.fillStyle = '#ffffff';
        ctx.textBaseline = 'top';
        ctx.fillText(state.doctorName || "Dr. Medical Specialist", docX + 34, docY + 20);

        ctx.font = `500 ${specFontSize}px ${docFontFamily}`;
        ctx.fillStyle = state.primaryColor || '#00f2fe';
        ctx.fillText(state.specialization || "Senior Consultant & Surgeon", docX + 34, docY + 22 + Math.round(W * 0.042));

        if (state.contactPhone) {
            ctx.font = `700 ${Math.round(specFontSize * 1.05)}px ${docFontFamily}`;
            ctx.fillStyle = '#ffffff';
            ctx.textAlign = 'right';
            ctx.fillText("\uD83D\uDCDE " + state.contactPhone, docX + doctorCardWidth - cardPadding, docY + 24);

            if (state.emergencyPhone) {
                ctx.font = `500 ${Math.round(specFontSize * 0.9)}px ${docFontFamily}`;
                ctx.fillStyle = '#f59e0b';
                ctx.fillText(state.emergencyPhone, docX + doctorCardWidth - cardPadding, docY + 24 + Math.round(W * 0.038));
            }
        }
        ctx.restore();

        this.registerHitElement('doctorCard', docX, docY, doctorCardWidth, doctorCardHeight, true);

        // 8. FIXED BOTTOM BRANDING SECTION
        const finalLogoUrl = state.logoUrl || "images/branding/cyh-logo.svg";
        const logoImg = this.cachedImages[finalLogoUrl] || await this.loadImage(finalLogoUrl);

        const bottomPadding = state.logoPaddingBottom || 35;
        const logoWidth = Math.round(W * ((state.logoSizePercent || 15) / 100) * 2.8);
        const logoHeight = Math.round(logoWidth * (80 / 360));

        let logoX = (W - logoWidth) / 2;
        if (state.logoPosition === 'bottom-left') {
            logoX = padding;
        } else if (state.logoPosition === 'bottom-right') {
            logoX = W - padding - logoWidth;
        }
        const logoY = H - bottomPadding - logoHeight;

        if (logoImg) {
            if (state.showLogoBadge) {
                ctx.save();
                const badgePadX = 20;
                const badgePadY = 12;
                ctx.fillStyle = 'rgba(7, 13, 30, 0.88)';
                ctx.strokeStyle = 'rgba(0, 242, 254, 0.3)';
                ctx.lineWidth = 1.5;
                this.drawRoundedRect(
                    ctx,
                    logoX - badgePadX,
                    logoY - badgePadY,
                    logoWidth + (badgePadX * 2),
                    logoHeight + (badgePadY * 2),
                    (logoHeight + (badgePadY * 2)) / 2
                );
                ctx.fill();
                ctx.stroke();
                ctx.restore();
            }

            ctx.drawImage(logoImg, logoX, logoY, logoWidth, logoHeight);
        }

        const websiteUrl = state.website || state.Website;
        if (websiteUrl) {
            ctx.save();
            ctx.font = `600 ${Math.round(W * 0.02)}px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif`;
            ctx.fillStyle = state.primaryColor || '#00f2fe';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            ctx.fillText("🌐 " + websiteUrl, W / 2, H - (state.logoBadgeText ? 28 : 10));
            ctx.restore();
        }

        if (state.logoBadgeText) {
            ctx.save();
            ctx.font = `600 ${Math.round(W * 0.018)}px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif`;
            ctx.fillStyle = 'rgba(148, 163, 184, 0.75)';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            ctx.letterSpacing = '1.5px';
            ctx.fillText(state.logoBadgeText.toUpperCase(), W / 2, H - 8);
            ctx.restore();
        }

        // Draw Active Drag / Selection Box
        if (canvasId === 'bannerCanvas' && !isPublished && this.draggingElement) {
            const dEl = this.draggingElement;
            ctx.save();
            ctx.strokeStyle = '#00f2fe';
            ctx.lineWidth = 4;
            ctx.setLineDash([8, 6]);
            ctx.strokeRect(dEl.x - 4, dEl.y - 4, dEl.w + 8, dEl.h + 8);

            // 4 corner handles
            const hSize = 14;
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(dEl.x - 4 - (hSize / 2), dEl.y - 4 - (hSize / 2), hSize, hSize);
            ctx.fillRect(dEl.x + dEl.w + 4 - (hSize / 2), dEl.y - 4 - (hSize / 2), hSize, hSize);
            ctx.fillRect(dEl.x - 4 - (hSize / 2), dEl.y + dEl.h + 4 - (hSize / 2), hSize, hSize);
            ctx.fillRect(dEl.x + dEl.w + 4 - (hSize / 2), dEl.y + dEl.h + 4 - (hSize / 2), hSize, hSize);

            // Drag Pill Badge Tag with Coordinates
            const coordText = dEl.id.toUpperCase() + "  (" + Math.round(dEl.x) + ", " + Math.round(dEl.y) + ")";
            ctx.font = 'bold 16px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif';
            const textWidth = ctx.measureText(coordText).width;
            const tagW = Math.max(140, textWidth + 24);
            const tagH = 34;
            const tagY = Math.max(8, dEl.y - 44);

            ctx.fillStyle = 'rgba(0, 242, 254, 0.95)';
            this.drawRoundedRect(ctx, dEl.x - 4, tagY, tagW, tagH, 8);
            ctx.fill();

            ctx.fillStyle = '#070d1e';
            ctx.textBaseline = 'middle';
            ctx.fillText(coordText, dEl.x + 8, tagY + (tagH / 2));
            ctx.restore();
        }
    },

    registerHitElement: function (id, x, y, w, h, isDraggable = true, extraData = null) {
        this.elements.push({ id, x, y, w, h, isDraggable: true, extraData });
    },

    attachMouseListeners: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            let retryCount = 0;
            const checkTimer = setInterval(() => {
                retryCount++;
                const c = document.getElementById(canvasId);
                if (c) {
                    clearInterval(checkTimer);
                    this.attachMouseListeners(canvasId);
                } else if (retryCount > 15) {
                    clearInterval(checkTimer);
                }
            }, 60);
            return;
        }

        if (this.attachedCanvas === canvas) return;

        if (this.attachedCanvas && this._cleanupListeners) {
            try { this._cleanupListeners(); } catch { }
        }

        this.attachedCanvas = canvas;
        canvas.style.touchAction = 'none';

        const getCanvasCoords = (e) => {
            const rect = canvas.getBoundingClientRect();
            if (!rect.width || !rect.height) return { x: 0, y: 0 };
            const scaleX = canvas.width / rect.width;
            const scaleY = canvas.height / rect.height;

            let clientX = e.clientX;
            let clientY = e.clientY;

            if (clientX === undefined || clientY === undefined) {
                if (e.touches && e.touches.length > 0) {
                    clientX = e.touches[0].clientX;
                    clientY = e.touches[0].clientY;
                } else if (e.changedTouches && e.changedTouches.length > 0) {
                    clientX = e.changedTouches[0].clientX;
                    clientY = e.changedTouches[0].clientY;
                } else {
                    clientX = 0;
                    clientY = 0;
                }
            }

            return {
                x: (clientX - rect.left) * scaleX,
                y: (clientY - rect.top) * scaleY
            };
        };

        const onPointerDown = (e) => {
            if (!this.currentState) return;
            const isPublished = Boolean(this.currentState.status === "Published" || this.currentState.Status === "Published");
            if (isPublished) {
                return;
            }
            const coords = getCanvasCoords(e);
            const isTouch = e.pointerType === 'touch' || (!e.pointerType && e.touches);
            const tolerance = isTouch ? 25 : 10;

            for (let i = this.elements.length - 1; i >= 0; i--) {
                const el = this.elements[i];
                if (coords.x >= (el.x - tolerance) && coords.x <= (el.x + el.w + tolerance) &&
                    coords.y >= (el.y - tolerance) && coords.y <= (el.y + el.h + tolerance)) {
                    this.draggingElement = el;
                    this.dragOffsetX = coords.x - el.x;
                    this.dragOffsetY = coords.y - el.y;
                    canvas.style.cursor = 'grabbing';
                    try {
                        if (e.pointerId !== undefined && canvas.setPointerCapture) {
                            canvas.setPointerCapture(e.pointerId);
                        }
                    } catch { }
                    if (e.cancelable) e.preventDefault();

                    this.renderBanner(canvasId, this.currentState);
                    break;
                }
            }
        };

        const onPointerMove = (e) => {
            if (!this.currentState) return;
            const isPublished = Boolean(this.currentState.status === "Published" || this.currentState.Status === "Published");
            if (isPublished) {
                canvas.style.cursor = 'default';
                return;
            }
            const coords = getCanvasCoords(e);

            if (canvasId === 'bannerCanvas' && !isPublished && this.draggingElement) {
                if (e.cancelable) e.preventDefault();
                const dims = this.getDimensions(this.currentState.aspectRatio);
                const W = dims.width;
                const H = dims.height;

                let newX = Math.round(coords.x - this.dragOffsetX);
                let newY = Math.round(coords.y - this.dragOffsetY);

                // Boundary protection: keep inside canvas
                newX = Math.max(-50, Math.min(W - 60, newX));
                newY = Math.max(0, Math.min(H - 40, newY));

                this.draggingElement.x = newX;
                this.draggingElement.y = newY;

                // Sync with currentState (both camelCase and PascalCase)
                const elId = this.draggingElement.id;
                if (elId === 'heading') {
                    this.currentState.headingX = newX;
                    this.currentState.headingY = newY;
                    this.currentState.HeadingX = newX;
                    this.currentState.HeadingY = newY;
                } else if (elId === 'subtitle') {
                    this.currentState.subtitleX = newX;
                    this.currentState.subtitleY = newY;
                    this.currentState.SubtitleX = newX;
                    this.currentState.SubtitleY = newY;
                } else if (elId === 'doctorCard') {
                    this.currentState.doctorCardX = newX;
                    this.currentState.doctorCardY = newY;
                    this.currentState.DoctorCardX = newX;
                    this.currentState.DoctorCardY = newY;
                } else if (elId === 'badge') {
                    this.currentState.badgeX = newX;
                    this.currentState.badgeY = newY;
                    this.currentState.BadgeX = newX;
                    this.currentState.BadgeY = newY;
                } else if (elId === 'hospitalName') {
                    this.currentState.hospitalNameX = newX;
                    this.currentState.hospitalNameY = newY;
                    this.currentState.HospitalNameX = newX;
                    this.currentState.HospitalNameY = newY;
                } else if (elId.startsWith('overlay_') && this.draggingElement.extraData) {
                    this.draggingElement.extraData.x = newX;
                    this.draggingElement.extraData.y = newY;
                    this.draggingElement.extraData.X = newX;
                    this.draggingElement.extraData.Y = newY;
                }

                if (this.currentState.status === "Published") {
                    this.currentState.status = "Draft";
                }
                if (this.currentState.Status === "Published") {
                    this.currentState.Status = "Draft";
                }

                this.renderBanner(canvasId, this.currentState);
            } else {
                // Hover hit test
                let hit = false;
                const tolerance = 10;
                for (let i = this.elements.length - 1; i >= 0; i--) {
                    const el = this.elements[i];
                    if (coords.x >= (el.x - tolerance) && coords.x <= (el.x + el.w + tolerance) &&
                        coords.y >= (el.y - tolerance) && coords.y <= (el.y + el.h + tolerance)) {
                        hit = true;
                        break;
                    }
                }
                canvas.style.cursor = hit ? 'grab' : 'default';
            }
        };

        const onPointerUp = (e) => {
            const isPublished = Boolean(this.currentState && (this.currentState.status === "Published" || this.currentState.Status === "Published"));
            if (canvasId === 'bannerCanvas' && !isPublished && this.draggingElement) {
                const el = this.draggingElement;
                this.draggingElement = null;
                canvas.style.cursor = 'grab';
                try {
                    if (e.pointerId !== undefined && canvas.releasePointerCapture) {
                        canvas.releasePointerCapture(e.pointerId);
                    }
                } catch { }

                // Notify Blazor of updated position
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnElementPositionChanged', el.id, el.x, el.y);
                }

                this.renderBanner(canvasId, this.currentState);
            }
        };

        canvas.addEventListener('pointerdown', onPointerDown);
        window.addEventListener('pointermove', onPointerMove, { passive: false });
        window.addEventListener('pointerup', onPointerUp);
        window.addEventListener('pointercancel', onPointerUp);

        // Touch fallbacks for mobile browsers
        const onTouchStart = (e) => {
            if (!this.currentState) return;
            const isPublished = Boolean(this.currentState.status === "Published" || this.currentState.Status === "Published");
            if (isPublished) return;
            const coords = getCanvasCoords(e);
            for (let i = this.elements.length - 1; i >= 0; i--) {
                const el = this.elements[i];
                if (coords.x >= (el.x - 30) && coords.x <= (el.x + el.w + 30) &&
                    coords.y >= (el.y - 30) && coords.y <= (el.y + el.h + 30)) {
                    if (e.cancelable) e.preventDefault();
                    break;
                }
            }
        };
        const onTouchMove = (e) => {
            if (this.draggingElement && e.cancelable) {
                e.preventDefault();
            }
        };
        canvas.addEventListener('touchstart', onTouchStart, { passive: false });
        canvas.addEventListener('touchmove', onTouchMove, { passive: false });

        this._cleanupListeners = () => {
            canvas.removeEventListener('pointerdown', onPointerDown);
            window.removeEventListener('pointermove', onPointerMove);
            window.removeEventListener('pointerup', onPointerUp);
            window.removeEventListener('pointercancel', onPointerUp);
            canvas.removeEventListener('touchstart', onTouchStart);
            canvas.removeEventListener('touchmove', onTouchMove);
        };
    },

    drawRoundedRect: function (ctx, x, y, width, height, radius) {
        ctx.beginPath();
        ctx.moveTo(x + radius, y);
        ctx.lineTo(x + width - radius, y);
        ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
        ctx.lineTo(x + width, y + height - radius);
        ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
        ctx.lineTo(x + radius, y + height);
        ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
        ctx.lineTo(x, y + radius);
        ctx.quadraticCurveTo(x, y, x + radius, y);
        ctx.closePath();
    },

    wrapText: function (ctx, text, maxWidth) {
        const words = (text || "").split(' ');
        const lines = [];
        let currentLine = words[0] || "";

        for (let i = 1; i < words.length; i++) {
            const word = words[i];
            const width = ctx.measureText(currentLine + " " + word).width;
            if (width < maxWidth) {
                currentLine += " " + word;
            } else {
                lines.push(currentLine);
                currentLine = word;
            }
        }
        if (currentLine) {
            lines.push(currentLine);
        }
        return lines;
    },

    downloadBanner: function (canvasId, fileName) {
        try {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                console.error("downloadBanner: Canvas not found", canvasId);
                return;
            }

            const link = document.createElement('a');
            link.download = fileName || `CYH_Banner_${Date.now()}.png`;

            try {
                link.href = canvas.toDataURL('image/png');
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } catch (secErr) {
                console.warn("toDataURL fallback to toBlob:", secErr);
                canvas.toBlob((blob) => {
                    if (!blob) return;
                    const url = URL.createObjectURL(blob);
                    link.href = url;
                    document.body.appendChild(link);
                    link.click();
                    setTimeout(() => {
                        document.body.removeChild(link);
                        URL.revokeObjectURL(url);
                    }, 200);
                }, 'image/png');
            }
        } catch (ex) {
            console.error("Error in downloadBanner:", ex);
        }
    },

    shareBanner: async function (canvasId, title, text) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return { success: false, message: "Canvas element not found" };

        try {
            return new Promise((resolve) => {
                canvas.toBlob(async (blob) => {
                    if (!blob) {
                        return resolve({ success: false, message: "Failed to generate image blob" });
                    }

                    const file = new File([blob], `CYH_Banner_${Date.now()}.png`, { type: 'image/png' });

                    if (navigator.canShare && navigator.canShare({ files: [file] })) {
                        try {
                            await navigator.share({
                                files: [file],
                                title: title || "CYH DoctorHub Health Banner",
                                text: text || "Check out our latest medical awareness banner."
                            });
                            return resolve({ success: true, message: "Shared successfully via Web Share" });
                        } catch (err) {
                            if (err.name === 'AbortError') {
                                return resolve({ success: false, message: "Share dismissed by user" });
                            }
                            console.warn("Share failed, falling back to download:", err);
                        }
                    }

                    const a = document.createElement('a');
                    a.href = URL.createObjectURL(blob);
                    a.download = `CYH_Banner_${Date.now()}.png`;
                    a.click();
                    URL.revokeObjectURL(a.href);
                    resolve({ success: true, message: "Downloaded directly (Web Share not supported on this browser)" });
                }, 'image/png');
            });
        } catch (ex) {
            console.error("Error in shareBanner:", ex);
            return { success: false, message: ex.message };
        }
    },
    compressImageUnder2MB: async function (file) {
        try {
            if (!file) return { success: false, message: "No file provided" };
            const maxBytes = 2 * 1024 * 1024; // 2 MB strict limit

            return new Promise((resolve) => {
                const reader = new FileReader();
                reader.onload = (e) => {
                    const img = new Image();
                    img.onload = () => {
                        let width = img.width;
                        let height = img.height;
                        const maxDim = 1920;

                        if (width > maxDim || height > maxDim) {
                            if (width > height) {
                                height = Math.round((height * maxDim) / width);
                                width = maxDim;
                            } else {
                                width = Math.round((width * maxDim) / height);
                                height = maxDim;
                            }
                        }

                        const canvas = document.createElement('canvas');
                        canvas.width = width;
                        canvas.height = height;
                        const ctx = canvas.getContext('2d');
                        ctx.drawImage(img, 0, 0, width, height);

                        let quality = 0.90;
                        let dataUrl = canvas.toDataURL('image/jpeg', quality);
                        let byteLength = Math.round((dataUrl.length - 22) * 3 / 4);

                        // Iteratively compress until < 2 MB
                        while (byteLength > maxBytes && quality > 0.40) {
                            quality -= 0.08;
                            dataUrl = canvas.toDataURL('image/jpeg', quality);
                            byteLength = Math.round((dataUrl.length - 22) * 3 / 4);
                        }

                        const sizeInKb = Math.round(byteLength / 1024);
                        const sizeFormatted = sizeInKb > 1024 ? (sizeInKb / 1024).toFixed(2) + ' MB' : sizeInKb + ' KB';

                        resolve({
                            success: true,
                            dataUrl: dataUrl,
                            fileName: file.name,
                            sizeBytes: byteLength,
                            sizeFormatted: sizeFormatted,
                            width: width,
                            height: height,
                            isUnder2MB: byteLength <= maxBytes
                        });
                    };
                    img.onerror = () => resolve({ success: false, message: "Failed to decode image" });
                    img.src = e.target.result;
                };
                reader.onerror = () => resolve({ success: false, message: "Failed to read file" });
                reader.readAsDataURL(file);
            });
        } catch (err) {
            return { success: false, message: err.message };
        }
    },

    compressBase64Under2MB: async function (base64Data, filename = "background.jpg") {
        try {
            const maxBytes = 2 * 1024 * 1024; // 2 MB limit
            return new Promise((resolve) => {
                const img = new Image();
                img.onload = () => {
                    let width = img.width;
                    let height = img.height;
                    const maxDim = 1920;

                    if (width > maxDim || height > maxDim) {
                        if (width > height) {
                            height = Math.round((height * maxDim) / width);
                            width = maxDim;
                        } else {
                            width = Math.round((width * maxDim) / height);
                            height = maxDim;
                        }
                    }

                    const canvas = document.createElement('canvas');
                    canvas.width = width;
                    canvas.height = height;
                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(img, 0, 0, width, height);

                    let quality = 0.90;
                    let dataUrl = canvas.toDataURL('image/jpeg', quality);
                    let byteLength = Math.round((dataUrl.length - 22) * 3 / 4);

                    while (byteLength > maxBytes && quality > 0.40) {
                        quality -= 0.08;
                        dataUrl = canvas.toDataURL('image/jpeg', quality);
                        byteLength = Math.round((dataUrl.length - 22) * 3 / 4);
                    }

                    const sizeInKb = Math.round(byteLength / 1024);
                    const sizeFormatted = sizeInKb > 1024 ? (sizeInKb / 1024).toFixed(2) + ' MB' : sizeInKb + ' KB';

                    resolve({
                        success: true,
                        dataUrl: dataUrl,
                        fileName: filename,
                        sizeBytes: byteLength,
                        sizeFormatted: sizeFormatted,
                        width: width,
                        height: height,
                        isUnder2MB: byteLength <= maxBytes
                    });
                };
                img.onerror = () => resolve({ success: false, message: "Failed to load image for compression" });
                img.src = base64Data;
            });
        } catch (err) {
            return { success: false, message: err.message };
        }
    }
};