document.addEventListener("DOMContentLoaded", () => {

    const scheduleSelect = document.getElementById("scheduleSelect");
    const scheduleInfo = document.getElementById("scheduleInfo");

    const wsId = document.getElementById("wsId");
    const wsDate = document.getElementById("wsDate");
    const wsStaff = document.getElementById("wsStaff");
    const wsStart = document.getElementById("wsStart");
    const wsEnd = document.getElementById("wsEnd");
    const wsShiftName = document.getElementById("wsShiftName");
    const wsManager = document.getElementById("wsManager");
    const wsDescription = document.getElementById("wsDescription");

    const changeRadio = document.getElementById("actionChange");
    const cancelRadio = document.getElementById("actionCancel");

    const newShiftSelect = document.getElementById("newShiftSelect");
    const form = document.getElementById("workshiftForm");

    // ✅ Mặc định disable vì chưa chọn loại yêu cầu
    newShiftSelect.disabled = true;

    // ✅ Khi chọn Xin đổi ca → enable
    changeRadio.addEventListener("change", () => {
        newShiftSelect.disabled = false;
    });

    // ✅ Khi chọn Xin hủy ca → disable
    cancelRadio.addEventListener("change", () => {
        newShiftSelect.disabled = true;
        newShiftSelect.value = ""; // reset dropdown về mặc định
    });

    // ✅ Load thông tin ca
    scheduleSelect.addEventListener("change", () => {
        const val = scheduleSelect.value;
        if (!val) {
            scheduleInfo.classList.add("d-none");
            resetFields();
            return;
        }

        fetch(`/WorkScheduleRequest/GetWorkSchedule?id=${val}`)
            .then(res => res.json())
            .then(data => {

                wsId.value = data.id ?? "";
                wsShiftName.value = data.shiftId ?? "";
                wsDate.value = data.date ?? "";
                wsDescription.value = data.description ?? "";
                wsStaff.value = data.staffName ?? "";
                wsManager.value = data.managerName ?? "";
                wsStart.value = data.startTime ?? "";
                wsEnd.value = data.endTime ?? "";
                document.getElementById("OldShiftId").value = data.shiftId;
                scheduleInfo.classList.remove("d-none");
            })
            .catch(err => console.error("GetWorkSchedule error:", err));
    });

    function resetFields() {
        wsId.value = "";
        wsDate.value = "";
        wsStaff.value = "";
        wsStart.value = "";
        wsEnd.value = "";
        wsShiftName.value = "";
        wsManager.value = "";
        wsDescription.value = "";
        document.getElementById("OldShiftId").value = "";
    }

});
