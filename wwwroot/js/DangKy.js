document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form"); // form đăng ký
    const inputs = {
        HoTen: document.querySelector("#HoTen"),
        Email: document.querySelector("#Email"),
        SDT: document.querySelector("#SDT"),
        DiaChi: document.querySelector("#DiaChi"),
        MatKhau: document.querySelector("#MatKhau"),
        XacNhanMatKhau: document.querySelector("#XacNhanMatKhau")
    };

    const errors = {
        HoTen: document.querySelector("[data-valmsg-for='HoTen']"),
        Email: document.querySelector("[data-valmsg-for='Email']"),
        SDT: document.querySelector("[data-valmsg-for='SDT']"),
        DiaChi: document.querySelector("[data-valmsg-for='DiaChi']"),
        MatKhau: document.querySelector("[data-valmsg-for='MatKhau']"),
        XacNhanMatKhau: document.querySelector("[data-valmsg-for='XacNhanMatKhau']")
    };

    // Hàm validate từng trường
    function validateField(name, value) {
        let message = "";

        switch (name) {
            case "HoTen":
                if (!value.trim()) {
                    message = "";
                } else if (!/^[\p{L}\s]+$/u.test(value)) {
                    message = "Họ tên chỉ được chứa chữ cái và khoảng trắng";
                } else if (value.trim().length < 8) {
                    message = "Họ tên phải có ít nhất 8 ký tự";
                }
                break;

            case "Email":
                if (!value.trim()) {
                    message = "";
                } else if (!/^[a-zA-Z0-9._%+-]+@gmail\.com$/.test(value)) {
                    message = "Email phải đúng định dạng @gmail.com";
                }
                break;

            case "SDT":
                if (!value.trim()) {
                    message = "";
                } else if (!/^\d{11}$/.test(value)) {
                    message = "Số điện thoại phải đủ 11 số";
                }
                break;

            case "DiaChi":
                if (!value.trim()) {
                    message = "";
                } else if (value.trim().length < 10) {
                    message = "Địa chỉ phải có ít nhất 10 ký tự";
                }
                break;

            case "MatKhau":
                if (value.length < 6) {
                    message = "Mật khẩu phải có ít nhất 6 ký tự";
                } else if (!/[A-Z]/.test(value) || !/[a-z]/.test(value) || !/\d/.test(value)) {
                    message = "Mật khẩu phải gồm chữ hoa, chữ thường và số";
                }
                break;

            case "XacNhanMatKhau":
                if (value !== inputs.MatKhau.value) {
                    message = "Mật khẩu xác nhận không khớp";
                }
                break;
        }

        errors[name].innerText = message;
        return message === "";
    }

    // Lắng nghe khi nhập dữ liệu
    Object.keys(inputs).forEach((key) => {
        if (inputs[key]) {
            inputs[key].addEventListener("input", () => validateField(key, inputs[key].value));
        }
    });

    // Validate trước khi submit
    form.addEventListener("submit", function (e) {
        let valid = true;
        Object.keys(inputs).forEach((key) => {
            if (!validateField(key, inputs[key].value)) {
                valid = false;
            }
        });

        if (!valid) {
            e.preventDefault();
        }
    });
});
