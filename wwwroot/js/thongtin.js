document.addEventListener("DOMContentLoaded", function () {

    const accordionButtons = document.querySelectorAll('.accordion-button');

    accordionButtons.forEach(clickedButton => {
        clickedButton.addEventListener('click', () => {

            // 1. Kiểm tra xem mục vừa click có đang mở hay không
            const isAlreadyOpen = clickedButton.classList.contains('active');

            // 2. Chạy vòng lặp qua TẤT CẢ các nút
            accordionButtons.forEach(button => {
                const content = button.nextElementSibling;

                if (button === clickedButton) {
                    // Xử lý mục vừa được click
                    if (isAlreadyOpen) {
                        // Nếu nó đang mở -> đóng nó lại
                        button.classList.remove('active');
                        content.style.maxHeight = null;
                    } else {
                        // Nếu nó đang đóng -> mở nó ra
                        button.classList.add('active');
                        content.style.maxHeight = content.scrollHeight + "px";
                    }
                } else {
                    // Xử lý các mục KHÁC -> luôn luôn đóng
                    button.classList.remove('active');
                    content.style.maxHeight = null;
                }
            });
        });
    });

});