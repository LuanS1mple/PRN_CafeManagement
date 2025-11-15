// ================== Avatar Preview ==================
document.addEventListener("DOMContentLoaded", function () {
    const fileInput = document.getElementById("AvatarFile");
    const avatar = document.getElementById("avatarPreview");

    if (fileInput && avatar) {
        fileInput.addEventListener("change", (e) => {
            const f = e.target.files && e.target.files[0];
            if (!f) return;
            const url = URL.createObjectURL(f);
            avatar.src = url;
        });
    }

    // ================== Mask input dd/MM/yyyy ==================
    function attachDdMmYyyyMask() {
        document.querySelectorAll(".date-ddmmyyyy").forEach(input => {
            input.addEventListener("input", function () {
                let v = this.value.replace(/\D/g, ""); // chỉ giữ số

                if (v.length > 8) v = v.slice(0, 8);

                if (v.length >= 5) {
                    this.value = v.slice(0, 2) + "/" + v.slice(2, 4) + "/" + v.slice(4, 8);
                } else if (v.length >= 3) {
                    this.value = v.slice(0, 2) + "/" + v.slice(2, 4);
                } else {
                    this.value = v;
                }
            });
        });
    }

    // ================== Format tiền VNĐ khi nhập ==================
    function attachMoneyFormat() {
        const moneyInputs = document.querySelectorAll(".money-vnd");

        moneyInputs.forEach(input => {
            input.addEventListener("input", function () {
                let raw = this.value.replace(/[^\d]/g, ""); // bỏ hết ký tự không phải số

                if (!raw) {
                    this.value = "";
                    return;
                }

                // parse số, format kiểu Việt Nam
                const num = parseInt(raw, 10);
                if (isNaN(num)) {
                    this.value = "";
                    return;
                }

                const formatted = num.toLocaleString("vi-VN");
                this.value = formatted + " VNĐ";
            });

            // nếu có value server trả về (ví dụ ModelState lỗi) thì re-format
            if (input.value && /\d/.test(input.value)) {
                let raw = input.value.replace(/[^\d]/g, "");
                if (raw) {
                    const num = parseInt(raw, 10);
                    if (!isNaN(num)) {
                        input.value = num.toLocaleString("vi-VN") + " VNĐ";
                    }
                }
            }
        });
    }

    // ================== Convert trước khi submit ==================
    function prepareBeforeSubmit(form) {
        form.addEventListener("submit", function () {
            // 1) Convert dd/MM/yyyy -> yyyy-MM-dd
            const dateInputs = form.querySelectorAll(".date-ddmmyyyy");
            dateInputs.forEach(input => {
                let v = input.value.trim();
                if (!v) return;

                const parts = v.split("/");
                if (parts.length === 3) {
                    const dd = parts[0].padStart(2, "0");
                    const mm = parts[1].padStart(2, "0");
                    const yyyy = parts[2];

                    // đơn giản: không validate lại ở client, server đã validate thêm rồi
                    input.value = `${yyyy}-${mm}-${dd}`;
                }
            });

            // 2) Convert tiền "1.000.000 VNĐ" -> "1000000"
            const moneyInputs = form.querySelectorAll(".money-vnd");
            moneyInputs.forEach(input => {
                let raw = input.value.replace(/[^\d]/g, "");
                if (!raw) {
                    input.value = ""; // để ModelState báo lỗi nếu required
                    return;
                }

                // để binder decimal parse được: "1000000"
                input.value = raw;
            });
        });
    }

    // ================== Init ==================
    attachDdMmYyyyMask();
    attachMoneyFormat();

    const form = document.querySelector("form");
    if (form) {
        prepareBeforeSubmit(form);
    }
});
