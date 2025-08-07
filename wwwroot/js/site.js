// site.js – Cosmic UI & Interaction Script
document.addEventListener("DOMContentLoaded", () => {
    const container = document.getElementById("cosmic-background");
    const nebulaColors = [
        'var(--nebula-1)',
        'var(--nebula-2)',
        'var(--nebula-3)',
        'var(--nebula-4)'
    ];

    /* 🌫 1. Khởi tạo Nebula nền động */
    nebulaColors.forEach(color => {
        const nebula = document.createElement("div");
        nebula.className = "nebula";
        nebula.style.left = `${Math.random() * 50}vw`;
        nebula.style.top = `${Math.random() * 10}vh`;
        nebula.style.setProperty('--nebula-color', color);
        container.appendChild(nebula);
    });

    /* 🌠 2. Mưa sao băng liên tục */
    function spawnStars() {
        for (let i = 0; i < 12; i++) {
            const star = document.createElement("div");
            star.className = "shooting-star";
            star.style.left = Math.random() * window.innerWidth + 'px';
            star.style.top = '-' + (Math.random() * 250 + 150) + 'px';
            star.style.animationDuration = (Math.random() * 6 + 4) + 's';
            container.appendChild(star);
            setTimeout(() => star.remove(), 1000);
        }
    }
    spawnStars();
    setInterval(spawnStars, 1000);

    /* 🍽️ 3. Tương tác khi chọn món ăn */
    document.querySelectorAll(".card, .add-to-cart-form").forEach(el => {
        el.addEventListener("click", (event) => {
            // Tránh xử lý nếu click vào nút hoặc form
            if (["BUTTON", "INPUT", "FORM"].includes(event.target.tagName)) return;

            const wrapper = el.closest(".aurora-card") || el.closest(".card") || el;
            if (!wrapper) return;

            // 🌠 Sao băng định hướng đáp vào món
            const rect = wrapper.getBoundingClientRect();
            const star = document.createElement("div");
            star.className = "shooting-star";
            star.style.position = "absolute";
            star.style.left = rect.left + rect.width / 2 + "px";
            star.style.top = "-80px";
            star.style.animationDuration = "4s";
            star.style.pointerEvents = "none";
            container.appendChild(star);
            setTimeout(() => star.remove(), 2000);

            // ⚡ Flash sáng tại món
            const flash = document.createElement("div");
            flash.className = "cosmic-flash";
            wrapper.appendChild(flash);
            setTimeout(() => flash.remove(), 500);

            // ✨ Glow ánh sáng vùng món ăn
            const glow = document.createElement("div");
            glow.className = "cosmic-glow";
            wrapper.style.position = "relative";
            wrapper.appendChild(glow);
            setTimeout(() => glow.remove(), 1000);
        });
    });

    /* 🔊 4. Âm thanh khi click món ăn */
    document.querySelectorAll(".card").forEach(card => {
        card.addEventListener("click", () => {
            card.classList.toggle("selected");
            const audio = document.getElementById("ting-audio");
            if (audio) {
                audio.currentTime = 0;
                audio.play();
            }
        });
    });

    /* 🔔 5. Toast thông báo tự động */
    ["login-toast", "order-toast", "success-toast"].forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            const toast = new bootstrap.Toast(el);
            toast.show();
            setTimeout(() => el.classList.add("hide"), 3000);
        }
    });

    /* ✅ 5.1 Phát âm thanh khi có thông báo thành công */
    const successAlert = document.querySelector(".alert-success");
    if (successAlert) {
        const audio = document.getElementById("ting-audio");
        if (audio) {
            audio.currentTime = 0;
            audio.play();
        }
    }

    /* 📱 6. Tự động đóng navbar mobile */
    document.querySelectorAll(".navbar-nav .nav-link").forEach(link => {
        link.addEventListener("click", () => {
            const navbarCollapse = document.querySelector(".navbar-collapse.show");
            if (navbarCollapse) {
                new bootstrap.Collapse(navbarCollapse, { toggle: true }).hide();
            }
        });
    });

    function flyToCart(imageSelector, cartSelector) {
        const image = document.querySelector(imageSelector);
        const cart = document.querySelector(cartSelector);

        if (!image || !cart) return;

        const imgRect = image.getBoundingClientRect();
        const cartRect = cart.getBoundingClientRect();

        const clone = image.cloneNode(true);
        clone.style.position = "fixed";
        clone.style.left = imgRect.left + "px";
        clone.style.top = imgRect.top + "px";
        clone.style.width = imgRect.width + "px";
        clone.style.height = imgRect.height + "px";
        clone.style.zIndex = 9999;
        clone.style.transition = "all 1s ease-in-out";
        clone.style.borderRadius = "50%";
        clone.style.opacity = "0.8";

        document.body.appendChild(clone);

        setTimeout(() => {
            clone.style.left = cartRect.left + cartRect.width / 2 + "px";
            clone.style.top = cartRect.top + cartRect.height / 2 + "px";
            clone.style.width = "30px";
            clone.style.height = "30px";
            clone.style.opacity = "0";
        }, 50);

        setTimeout(() => clone.remove(), 1000);
    }

    function shakeCartIcon() {
        const cartIcon = document.querySelector('.nav-link[href*=Cart]');
        if (cartIcon) {
            cartIcon.classList.add('cart-shake');
            setTimeout(() => cartIcon.classList.remove('cart-shake'), 600);
        }
    }

    function showDiscountPopup() {
        const popup = document.getElementById("discount-popup");
        if (popup) {
            const toast = new bootstrap.Toast(popup);
            toast.show();
            setTimeout(() => popup.classList.add("hide"), 3000);
        }
    }


});
