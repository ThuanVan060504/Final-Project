function updateCountdowns() {
    const countdownElements = document.querySelectorAll('.countdown');

    countdownElements.forEach(el => {
        const endTime = new Date(el.getAttribute('data-endtime'));
        const now = new Date();
        const diff = endTime - now;

        el.classList.remove("text-warning", "text-danger", "text-muted");

        if (diff <= 0) {
            el.innerText = "Đã kết thúc";
            el.classList.add("text-muted");
            return;
        }

        const seconds = Math.floor((diff / 1000) % 60);
        const minutes = Math.floor((diff / 1000 / 60) % 60);
        const hours = Math.floor((diff / 1000 / 60 / 60) % 24);
        const days = Math.floor(diff / 1000 / 60 / 60 / 24);

        let countdownText = '';
        if (days > 0) {
            countdownText += `${days}d `;
        }
        countdownText += `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

        // Thêm màu theo thời gian còn lại
        if (diff < 10 * 60 * 1000) { // dưới 10 phút
            el.classList.add("text-danger");
        } else if (diff < 60 * 60 * 1000) { // dưới 1 tiếng
            el.classList.add("text-warning");
        } else {
            el.classList.add("text-success");
        }

        el.innerText = countdownText;
        el.setAttribute('title', `Kết thúc lúc: ${endTime.toLocaleString()}`);
    });
}

document.addEventListener("DOMContentLoaded", function () {
    updateCountdowns();
    setInterval(updateCountdowns, 1000);
});
