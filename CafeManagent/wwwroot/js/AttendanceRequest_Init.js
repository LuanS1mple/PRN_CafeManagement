document.addEventListener("DOMContentLoaded", function () {
    const workDateInput = document.getElementById("workDate");
    const workshiftSelect = document.getElementById("workshift");
    const checkInInput = document.getElementById("currentCheckIn");
    const checkOutInput = document.getElementById("currentCheckOut");

    // Hàm gọi dữ liệu attendance từ backend
    async function fetchAttendance() {
        const workDate = workDateInput.value;
        const workshiftId = workshiftSelect.value;

        // Chỉ gọi API khi đã chọn đủ ngày và ca
        if (!workDate || !workshiftId) {
            checkInInput.value = "";
            checkOutInput.value = "";
            return;
        }

        try {
            // Gọi API backend (ví dụ endpoint trong Controller)
            const response = await fetch(`/AttendanceRequest/GetAttendance?workDate=${workDate}&workshiftId=${workshiftId}`);

            if (!response.ok) throw new Error("Lỗi khi gọi API");

            const data = await response.json();

            if (data && data.checkIn && data.checkOut) {
                // Có dữ liệu → điền vào form
                checkInInput.value = data.checkIn;
                checkOutInput.value = data.checkOut;
            } else {
                // Không có dữ liệu → xóa giá trị
                checkInInput.value = "";
                checkOutInput.value = "";
            }
        } catch (error) {
            console.error("Fetch error:", error);
            checkInInput.value = "";
            checkOutInput.value = "";
        }
    }

    // Gắn sự kiện onchange cho cả ngày và ca làm việc
    workDateInput.addEventListener("change", fetchAttendance);
    workshiftSelect.addEventListener("change", fetchAttendance);
});
