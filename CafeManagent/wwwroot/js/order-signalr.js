function initOrderSignalR(role) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/orderHub")
        .build();

    connection.on("OrderCreated", (order) => {
        if (role === "waiter" || role === "bartender") {
            addOrderToSection("waiting-orders", order);
        }
    });

    connection.on("OrderCanceled", (orderId) => {
        document.getElementById(`order-${orderId}`)?.remove();
    });

    connection.on("OrderPreparing", (orderId) => {
        moveOrder(orderId, "preparing-orders");
    });

    connection.on("OrderReady", (order) => {
        moveOrder(order.OrderId, "ready-orders");
    });

    connection.on("OrderConfirmed", (order) => {
        document.getElementById(`order-${order.OrderId}`)?.remove();
        if (role === "bartender") addOrderToSection("completed-orders", order);
    });

    connection.start()
        .then(() => console.log(role + " connected to OrderHub"))
        .catch(err => console.error(err));
}

// Helper
function addOrderToSection(sectionId, order) {
    const container = document.getElementById(sectionId);
    if (!container) return;
    const html = `
    <div class="card mb-2 p-3 shadow-sm" id="order-${order.OrderId}">
        <div class="d-flex justify-content-between align-items-center">
            <div>
                <h6>Order #${order.OrderId} - <b>${order.CustomerName}</b></h6>
                <div><b>Trạng thái:</b> ${order.StatusText}</div>
                <div class="text-muted small">${order.OrderTime}</div>
            </div>
        </div>
    </div>`;
    container.insertAdjacentHTML("afterbegin", html);
}

function moveOrder(orderId, targetSectionId) {
    const el = document.getElementById(`order-${orderId}`);
    const target = document.getElementById(targetSectionId);
    if (el && target) {
        el.remove();
        target.insertAdjacentElement("afterbegin", el);
    }
}
