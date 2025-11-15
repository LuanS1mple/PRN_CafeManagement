// ================== Avatar Preview ==================
const fileInput = document.getElementById('avatarFile');
const avatar = document.getElementById('avatarPreview');

if (fileInput && avatar) {
    fileInput.addEventListener('change', (e) => {
        const f = e.target.files?.[0];
        if (!f) return;
        const url = URL.createObjectURL(f);
        avatar.src = url;
    });
}

// ================== Mask input dd/MM/yyyy ==================
function attachDdMmYyyyMask() {
    document.querySelectorAll(".date-ddmmyyyy").forEach(input => {
        input.addEventListener("input", function () {
            let v = this.value.replace(/\D/g, "");
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

// ================== Convert dd/MM/yyyy -> yyyy-MM-dd trước khi submit ==================
function convertDatesBeforeSubmit(form) {
    form.addEventListener("submit", function () {
        const inputs = form.querySelectorAll(".date-ddmmyyyy");

        inputs.forEach(input => {
            let v = input.value.trim();
            if (!v) return;

            const parts = v.split("/");
            if (parts.length === 3) {
                const dd = parts[0].padStart(2, "0");
                const mm = parts[1].padStart(2, "0");
                const yyyy = parts[2];

                input.value = `${yyyy}-${mm}-${dd}`;
            }
        });
    });
}

// ================== Init ==================
document.addEventListener("DOMContentLoaded", function () {
    attachDdMmYyyyMask();
    const form = document.querySelector("form");
    if (form) convertDatesBeforeSubmit(form);
});
