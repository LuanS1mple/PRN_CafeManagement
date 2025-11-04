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

    const actionRadios = document.querySelectorAll("input[name='actionType']");
    const changeShiftSection = document.getElementById("changeShiftSection");
    const newShiftSelect = document.getElementById("newShiftSelect");


    scheduleSelect.addEventListener("change", () => {

        const val = scheduleSelect.value;
        if (!val) {
            scheduleInfo.classList.add("d-none");
            resetFields();
            return;
        }

        const id = val;

        fetch(`/WorkScheduleRequest/GetWorkSchedule?id=${id}`)
            .then(res => res.json())
            .then(data => {

                wsId.value = data.id ?? "";
                wsShiftName.value = data.shiftName ?? "";
                wsDate.value = data.date ?? "";
                wsDescription.value = data.description ?? "";

                wsStaff.value = data.staffName ?? "";
                wsManager.value = data.managerName ?? "";

                wsStart.value = data.startTime ?? "";
                wsEnd.value = data.endTime ?? "";

                scheduleInfo.classList.remove("d-none");

                changeShiftSection.classList.add("d-none");
                newShiftSelect.innerHTML = `<option value="">-- Chọn ca --</option>`;
                actionRadios.forEach(r => r.checked = false);
            });

    });

    actionRadios.forEach(radio => {
        radio.addEventListener("change", () => {
            if (radio.value === "change") {
                changeShiftSection.classList.remove("d-none");
                loadAlternativeShifts();
            } else {
                changeShiftSection.classList.add("d-none");
                newShiftSelect.innerHTML = `<option value="">-- Chọn ca --</option>`;
            }
        });
    });


    function loadAlternativeShifts() {
        const val = scheduleSelect.value;
        if (!val) return;

        const [staffId, workShiftId, date] = val.split("-");

        newShiftSelect.innerHTML = `<option value="">-- Chọn ca --</option>`;

        document.querySelectorAll("#scheduleSelect option").forEach(opt => {
            if (!opt.value) return;

            const parts = opt.value.split("-");
            const optShiftId = parts[1];
            const optDate = parts[2];

            if (optDate === date && optShiftId !== workShiftId) {
                newShiftSelect.innerHTML += `
                    <option value="${optShiftId}">
                        Ca ${optShiftId} - ${optDate}
                    </option>`;
            }
        });
    }

    function resetFields() {
        wsId.value = "";
        wsDate.value = "";
        wsStaff.value = "";
        wsStart.value = "";
        wsEnd.value = "";
        wsShiftName.value = "";
        wsManager.value = "";
        wsDescription.value = "";
    }

});
