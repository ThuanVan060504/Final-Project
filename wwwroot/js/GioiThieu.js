document.addEventListener("DOMContentLoaded", function () {

    // --- 1. Thêm CSS cho hiệu ứng động (ẩn đi lúc đầu) ---
    // Chúng ta thêm CSS này bằng JS để đảm bảo nó không ảnh hưởng
    // đến trình duyệt nếu JS bị tắt.
    const style = document.createElement('style');
    style.innerHTML = `
        /* Trạng thái ban đầu: ẩn và dịch xuống */
        .vmv-block, .team, .facilities, .cta {
            opacity: 0;
            transform: translateY(40px);
            transition: opacity 0.8s ease-out, transform 0.8s ease-out;
            visibility: hidden;
        }

        /* Trạng thái khi xuất hiện */
        .vmv-block.is-visible,
        .team.is-visible,
        .facilities.is-visible,
        .cta.is-visible {
            opacity: 1;
            transform: translateY(0);
            visibility: visible;
        }
    `;
    document.head.appendChild(style);

    // --- 2. Thiết lập Intersection Observer ---
    const sections = document.querySelectorAll('.vmv-block, .team, .facilities, .cta');

    const options = {
        root: null, // Sử dụng viewport làm gốc
        rootMargin: '0px',
        threshold: 0.1 // Kích hoạt khi 10% phần tử xuất hiện
    };

    const observer = new IntersectionObserver(function (entries, observer) {
        entries.forEach(entry => {
            // Khi phần tử đi vào viewport
            if (entry.isIntersecting) {
                // Thêm class 'is-visible' để kích hoạt hiệu ứng CSS
                entry.target.classList.add('is-visible');

                // Ngừng theo dõi phần tử này (để hiệu ứng chỉ chạy 1 lần)
                observer.unobserve(entry.target);
            }
        });
    }, options);

    // Bắt đầu theo dõi tất cả các section
    sections.forEach(section => {
        observer.observe(section);
    });

});