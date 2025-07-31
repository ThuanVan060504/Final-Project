window.addEventListener('load', function () {
    const splash = document.getElementById('splash-screen');

    setTimeout(() => {
        // 1. Ẩn splash
        if (splash) {
            splash.classList.add('hidden');
        }

        // 2. Sau đó apply hiệu ứng cho trang chính
        setTimeout(() => {
            document.body.classList.add('body-appear');
        }, 400); // delay nhẹ sau khi splash ẩn (0.4s)
    }, 1200); // thời gian chờ splash (1.4s)
});
