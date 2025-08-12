document.addEventListener("DOMContentLoaded", function () {
    const emailInput = document.querySelector("input[name='Email']");
    const passInput = document.querySelector("input[name='MatKhau']");

    const emailError = document.createElement("div");
    emailError.classList.add("text-danger", "small");
    emailInput.parentElement.appendChild(emailError);

    const passError = document.createElement("div");
    passError.classList.add("text-danger", "small");
    passInput.parentElement.appendChild(passError);

    // Regex check Gmail
    const emailPattern = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;

    emailInput.addEventListener("input", function () {
        if (emailInput.value && !emailPattern.test(emailInput.value)) {
            emailError.textContent = "Email phải đúng định dạng @gmail.com";
        } else {
            emailError.textContent = "";
        }
    });

    passInput.addEventListener("input", function () {
        if (passInput.value && passInput.value.length < 6) {
            passError.textContent = "Mật khẩu phải ít nhất 6 ký tự";
        } else if (passInput.value && (!/[A-Z]/.test(passInput.value) || !/[a-z]/.test(passInput.value))) {
            passError.textContent = "Mật khẩu phải có chữ hoa và chữ thường";
        } else {
            passError.textContent = "";
        }
    });
});
