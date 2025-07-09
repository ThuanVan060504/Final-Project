document.getElementById("togglePassword").addEventListener("click", function () {
    const passwordInput = document.getElementById("password");
    const type = passwordInput.type === "password" ? "text" : "password";
    passwordInput.type = type;

    this.classList.toggle("ri-eye-fill");
    this.classList.toggle("ri-eye-off-fill");
});
