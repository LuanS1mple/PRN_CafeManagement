
(function () {
    // ===== Lấy anti-forgery token từ form #__af =====
    const tokenInput = document.querySelector('#__af input[name="__RequestVerificationToken"]');
    const antiForgeryToken = tokenInput ? tokenInput.value : null;

    // ===== Kết nối SignalR tới hub /hubs/staff =====
    if (!window.signalR) {
        console.error("signalR chưa được load. Nhớ include script SignalR trước file staff-status.js");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/staff")
        .withAutomaticReconnect()
        .build();

    // Khi server bắn sự kiện cập nhật status
    connection.on("ReceiveStatusUpdate", (payload) => {
        // payload = { staffId, status, name, badgeClass }
        const cell = document.querySelector(`td[data-staff-id='${payload.staffId}']`);
        if (!cell) return;

        const sel = cell.querySelector('.js-status-select');
        const badge = cell.querySelector('.js-status-badge');
        const dot = badge ? badge.querySelector('.dot') : null;

        // Cập nhật dropdown
        if (sel) {
            sel.value = String(payload.status);
        }

        // Cập nhật badge (màu + text)
        if (badge) {
            badge.classList.remove('badge-green', 'badge-red', 'badge-gray');
            if (payload.badgeClass) {
                badge.classList.add(payload.badgeClass);
            }

            // Giữ lại span.dot, chỉ chỉnh text phía sau
            const children = Array.from(badge.childNodes);
            const textNode = children.find(n => n.nodeType === Node.TEXT_NODE);
            if (textNode) {
                textNode.nodeValue = " " + payload.name;
            } else {
                badge.append(" " + payload.name);
            }
        }

        // Cập nhật màu chấm dot
        if (dot) {
            dot.classList.remove('dot-green', 'dot-red', 'dot-gray');
            if (payload.status === 1) {
                dot.classList.add('dot-green');
            } else if (payload.status === 3) {
                dot.classList.add('dot-red');
            } else {
                dot.classList.add('dot-gray');
            }
        }
    });

    async function startConnection() {
        try {
            await connection.start();
            console.log("SignalR connected to /hubs/staff");
        } catch (err) {
            console.error("SignalR connect error:", err);
            setTimeout(startConnection, 2000);
        }
    }

    startConnection();

    // ===== Gửi AJAX khi user đổi dropdown trạng thái =====
    document.addEventListener('change', async (e) => {
        const sel = e.target.closest('.js-status-select');
        if (!sel) return;

        const staffId = sel.getAttribute('data-id');
        const newVal = sel.value;

        if (!antiForgeryToken) {
            alert("Thiếu anti-forgery token, không thể gửi yêu cầu.");
            return;
        }

        sel.disabled = true;

        try {
            const res = await fetch('/staffs/status', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                },
                body: new URLSearchParams({
                    __RequestVerificationToken: antiForgeryToken,
                    id: staffId,
                    status: newVal
                })
            });

            if (!res.ok) throw new Error('HTTP ' + res.status);
            const json = await res.json();
            if (!json.ok) throw new Error(json.message || 'Cập nhật thất bại');

            // Không update UI ở đây, vì SignalR sẽ bắn ReceiveStatusUpdate cho tất cả tab.
        } catch (err) {
            console.error(err);
            alert(err.message || 'Có lỗi xảy ra trong quá trình cập nhật.');

            // revert lại select theo text badge hiện tại
            const cell = sel.closest('td');
            const badge = cell ? cell.querySelector('.js-status-badge') : null;
            const txt = (badge?.textContent || "").trim();

            if (txt === 'Đang làm việc') sel.value = '1';
            else if (txt === 'Nghỉ phép') sel.value = '2';
            else if (txt === 'Nghỉ việc') sel.value = '3';
        } finally {
            sel.disabled = false;
        }
    });
})();
