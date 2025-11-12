
$(document).ready(function () {
    let cart = []; 
    const VAT_PERCENT = 0.05;
    function formatCurrency(amount) {
        return (amount || 0).toLocaleString('vi-VN', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }) + ' VNĐ';
    }
    function updateTotals() {
        const subtotal = cart.reduce((sum, item) => sum + (item.Quantity * item.UnitPrice), 0);
        const discountPercent = parseFloat($('#discountPercent').val()) || 0;
        const phone = $('#customerPhone').val();
        const note = $('#orderNote').val();

        // 1. Chuẩn bị DTO để gửi đi (Chỉ cần gửi dữ liệu thô)
        const draftDto = {
            Items: cart,
            CustomerPhone: phone,
            DiscountPercent: discountPercent,
            Note: note
        };

        // 2. Gọi API để Controller thực hiện tính toán đầy đủ và tìm kiếm khách hàng
        $.ajax({
            url: '/Order/CalculateDraftApi', // Cần tạo API này trong Controller
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(draftDto),
            success: function (response) {
                if (response.success) {
                    const data = response.data;
                    $('#subtotal').text(formatCurrency(data.Subtotal));
                    $('#discountAmount').text(formatCurrency(data.DiscountAmount));
                    $('#vatAmount').text(formatCurrency(data.VATAmount));
                    $('#grandTotal').text(formatCurrency(data.GrandTotal));

                    // Cập nhật thông tin khách hàng
                    $('#customerInfo').html(data.CustomerStatus);
                } else {
                    console.error("Lỗi tính toán API:", response.message);
                }
            },
            error: function (xhr) {
                console.error('Lỗi server khi tính toán giá.');
            }
        });
    }

    // Hàm Render Cart
    function updateCartUI() {
        $('#cartItems').empty();
        if (cart.length === 0) {
            $('#cartItems').append('<div class="text-center text-muted mt-3" id="emptyCartMessage">Giỏ hàng trống.</div>');
            updateTotals();
            return;
        }

        cart.forEach(item => {
            const itemTotal = item.Quantity * item.UnitPrice;

            // ĐÃ SỬA: Cấu trúc item mới, căn chỉnh 3 cột (Tên, SL, Thành tiền)
            const cartItemHtml = `
                <div class="list-group-item d-flex justify-content-between align-items-center p-2 border-0 border-bottom" data-product-id="${item.ProductId}">
                    <span class="text-dark small fw-semibold" style="width: 40%;">${item.ProductName}</span>
                    
                    <div class="d-flex align-items-center justify-content-end" style="width: 60%;">
                        <input type="number" class="form-control form-control-sm mx-2 item-quantity text-center"
                               style="width: 50px;" 
                               min="1"
                               value="${item.Quantity}"
                               data-product-id="${item.ProductId}">
                               
                        <span class="text-dark fw-semibold me-3" style="font-size: 0.9em; width: 70px; text-align: right;">${formatCurrency(itemTotal)}</span>
                        
                        <button class="btn btn-sm btn-outline-danger btn-remove-item" data-product-id="${item.ProductId}">
                            <i class="fas fa-times" style="font-size: 0.7em;"></i>
                        </button>
                    </div>
                </div>
            `;
            $('#cartItems').append(cartItemHtml);
        });

        updateTotals(); // Gọi hàm tính toán và cập nhật giá
    }


    // --- 2. XỬ LÝ SỰ KIỆN CART & TÍNH TOÁN ---

    // A. Thêm sản phẩm vào Cart
    $(document).on('click', '.btn-add-to-cart', function () {
        const $item = $(this).closest('.product-item');
        const productId = parseInt($item.data('id'));
        const productName = $item.data('product-name');
        const unitPrice = parseFloat($item.data('price'));

        const existingItem = cart.find(item => item.ProductId === productId);

        if (existingItem) {
            existingItem.Quantity++;
        } else {
            cart.push({
                ProductId: productId,
                ProductName: productName,
                Quantity: 1,
                UnitPrice: unitPrice
            });
        }
        updateCartUI(); // Cập nhật UI và gọi updateTotals()
    });

    // B. Thay đổi số lượng (Event delegation)
    $(document).on('change', '.item-quantity', function () {
        const productId = parseInt($(this).data('product-id'));
        const newQuantity = parseInt($(this).val());

        if (newQuantity <= 0 || isNaN(newQuantity)) {
            $(this).val(1);
            return;
        }

        const item = cart.find(item => item.ProductId === productId);
        if (item) {
            item.Quantity = newQuantity;
        }
        updateCartUI();
    });

    // C. Xóa sản phẩm khỏi Cart
    $(document).on('click', '.btn-remove-item', function () {
        const productId = parseInt($(this).data('product-id'));
        cart = cart.filter(item => item.ProductId !== productId);
        updateCartUI();
    });

    // D. Thay đổi phần trăm giảm giá HOẶC SĐT khách hàng
    $('#discountPercent, #customerPhone').on('input change', function () {
        // Chỉ gọi updateTotals() để tính toán lại qua API
        updateTotals();

        // Nếu là SĐT, cần cập nhật UI sớm hơn (có thể tách CheckPhone riêng nếu cần)
        if ($(this).attr('id') === 'customerPhone') {
            // Tối ưu: updateTotals() đã tự động gọi API để kiểm tra SĐT và cập nhật customerInfo
        }
    });


    // E. Tìm kiếm Sản phẩm (Local search)
    $('#productSearch').on('keyup', function () {
        const query = $(this).val().toLowerCase();
        $('.product-item').each(function () {
            const name = $(this).data('name');
            if (name.includes(query)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });


    // --- 3. GỌI API TẠO BẢN NHÁP (SUBMIT DRAFT) ---

    $('#submitDraft').click(function () {
        if (cart.length === 0) {
            alert('Vui lòng thêm sản phẩm vào giỏ hàng.');
            return;
        }

        const draftData = {
            Items: cart,
            CustomerPhone: $('#customerPhone').val(),
            DiscountPercent: parseFloat($('#discountPercent').val()) || 0,
            Note: $('#orderNote').val()
        };

        // Gọi API CreateDraft để Controller lưu vào Session và chuyển trang
        $.ajax({
            url: '/Order/CreateDraft',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(draftData),
            beforeSend: function () {
                $('#submitDraft').prop('disabled', true).text('Đang xử lý...');
            },
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl; // Chuyển hướng tới DraftView
                } else {
                    alert('Lỗi tạo bản nháp: ' + response.message);
                    $('#submitDraft').prop('disabled', false).text('Tạo Bản Nháp & Tiếp tục');
                }
            },
            error: function (xhr) {
                alert('Lỗi server khi tạo bản nháp. Vui lòng thử lại.');
                $('#submitDraft').prop('disabled', false).text('Tạo Bản Nháp & Tiếp tục');
            }
        });
    });

    // Khởi tạo (Nếu muốn hỗ trợ chỉnh sửa Draft từ Session, logic load cart từ Session cần được thêm vào đây)
    updateCartUI();
});