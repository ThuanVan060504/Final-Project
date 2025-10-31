// ========================== 💬 FAQ Accordion Script ========================== //
document.addEventListener("DOMContentLoaded", () => {
    const accordionButtons = document.querySelectorAll(".accordion-button");

    accordionButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            const content = btn.nextElementSibling;

            // Ẩn tất cả các accordion khác trước khi mở cái được click
            document.querySelectorAll(".accordion-content").forEach((item) => {
                if (item !== content) {
                    item.style.maxHeight = null;
                    item.previousElementSibling.classList.remove("active");
                }
            });

            // Toggle mở/đóng nội dung
            btn.classList.toggle("active");
            if (content.style.maxHeight) {
                content.style.maxHeight = null;
            } else {
                content.style.maxHeight = content.scrollHeight + "px";
            }
        });
    });
});

// ========================== 🎨 Hiệu ứng nhỏ khi cuộn ========================== //
window.addEventListener("scroll", () => {
    const header = document.querySelector("header");
    if (!header) return;

    if (window.scrollY > 100) {
        header.classList.add("scrolled");
    } else {
        header.classList.remove("scrolled");
    }
});

// ========================== ⏫ Cuộn lên đầu trang ========================== //
const backToTopBtn = document.createElement("button");
backToTopBtn.innerHTML = "⬆️";
backToTopBtn.className = "back-to-top";
document.body.appendChild(backToTopBtn);

backToTopBtn.addEventListener("click", () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
});

window.addEventListener("scroll", () => {
    if (window.scrollY > 500) {
        backToTopBtn.style.display = "block";
    } else {
        backToTopBtn.style.display = "none";
    }
});
