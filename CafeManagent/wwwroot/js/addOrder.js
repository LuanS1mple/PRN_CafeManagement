// File: ~/js/addOrder.js
$(document).ready(function () {
    let cart = [];
    const VAT_PERCENT = 0.05;
    let currentDraftOrderId = 0;

    // ... (Giữ nguyên: loadDraftFromInitialData, formatCurrency) ...
    function loadDraftFromInitialData() {
        if (initialDraftData && initialDraftData.Items) {
            console.log("Đang tải dữ liệu bản nháp từ Session...");
            const draft = initialDraftData;
            currentDraftOrderId = draft.OrderId || 0;
            cart = draft.Items.map(item => ({
                ProductId: item.ProductId,
                ProductName: item.ProductName,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice
            }));

            $('#customerPhone').val(draft.CustomerPhone || '');
            $('#discountPercent').val(draft.DiscountPercent || 0);
            $('#orderNote').val(draft.Note || '');

            $('#submitDraft').text('Cập Nhật Bản Nháp').removeClass('btn-info').addClass('btn-primary');

            console.log(`Đã tải bản nháp Order #${currentDraftOrderId} thành công.`);
        }
        updateCartUI();
    }

    function formatCurrency(amount) {
        return (amount || 0).toLocaleString('vi-VN', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }) + ' VNĐ';
    }


    function updateTotals() {
        // 1. Logic Thoát Sớm (Cart Rỗng)
        if (cart.length === 0 && currentDraftOrderId === 0) {
            $('#subtotal').text(formatCurrency(0));
            $('#discountAmount').text(formatCurrency(0));
            $('#vatAmount').text(formatCurrency(0));
            $('#grandTotal').text(formatCurrency(0));
            $('#emptyCartMessage').show();
            $('#customerInfo').html('Không nhập SĐT/SĐT không hợp lệ.');
            $('#newCustomerNameContainer').empty().hide();
            return;
        }

        const discountPercent = parseFloat($('#discountPercent').val()) || 0;
        let phone = $('#customerPhone').val();
        const note = $('#orderNote').val();

        // --- XÁC THỰC PHONE TRÊN FRONTEND (MỚI) ---
        // Loại bỏ các ký tự không phải số và giới hạn độ dài
        phone = phone.replace(/[^\d]/g, '');
        $('#customerPhone').val(phone); // Cập nhật lại giá trị input

        // Chỉ coi là hợp lệ khi >= 10 số (hoặc nếu rỗng, ta vẫn gọi API để tính toán tổng tiền vãng lai)
        if (phone.length > 0 && phone.length < 10) {
            $('#customerPhone').addClass('is-invalid');
            $('#customerInfo').html('<span class="text-danger">SĐT phải có ít nhất 10 chữ số.</span>');
            // Cập nhật giá trị phone để gửi lên API là rỗng (coi như khách vãng lai)
            phone = '';
        } else {
            $('#customerPhone').removeClass('is-invalid');
        }
        // --- KẾT THÚC XÁC THỰC PHONE ---

        const newCustomerNameValue = $('#newCustomerName').val() || '';

        const draftDto = {
            Items: cart,
            CustomerPhone: phone, // Gửi SĐT đã được làm sạch và xác thực
            DiscountPercent: discountPercent,
            Note: note,
            OrderId: currentDraftOrderId,
            NewCustomerName: newCustomerNameValue
        };

        // 2. Gọi API tính toán
        $.ajax({
            url: '/Order/CalculateDraftApi',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(draftDto),
            success: function (response) {
                if (response.success) {
                    const data = response.data;

                    // Cập nhật các trường tổng tiền (Không gây mất focus)
                    $('#subtotal').text(formatCurrency(data.subtotal));
                    $('#discountAmount').text(formatCurrency(data.discountAmount));
                    $('#vatAmount').text(formatCurrency(data.vatAmount));
                    $('#grandTotal').text(formatCurrency(data.grandTotal));
                    $('#customerInfo').html(data.customerStatus);

                    // --- XỬ LÝ RENDER/TẠO Ô NHẬP TÊN (CHỈ KHI CẦN THIẾT) ---
                    const newCustomerContainer = $('#newCustomerNameContainer');

                    // Trường hợp 1: SĐT mới (cần nhập tên) VÀ container CHƯA có input
                    if (data.customerFoundStatus === 2 && newCustomerContainer.is(':empty')) {
                        const html = `
                        <div class="alert alert-info py-2 mt-2" role="alert">
                            <i class="fas fa-exclamation-circle me-2"></i> Đây là **SĐT mới**. Vui lòng nhập **Tên khách hàng** để lưu thông tin và tích lũy ${data.pointsEarned} điểm.
                        </div>
                        <div class="form-group mb-2">
                            <label for="newCustomerName" class="form-label small fw-bold">Tên Khách Hàng Mới:</label>
                            <input type="text" class="form-control" id="newCustomerName" placeholder="Ví dụ: Nguyễn Văn A" value="${newCustomerNameValue}">
                            <div class="invalid-feedback">Vui lòng nhập tên khách hàng.</div>
                        </div>
                        `;
                        newCustomerContainer.html(html).show();

                        // QUAN TRỌNG: Gắn lại sự kiện updateTotals cho ô input Tên Khách Hàng Mới
                        $('#newCustomerName').off('input').on('input', function () {
                            // Khi nhập tên mới, ta không cần gọi API, chỉ cần gọi updateTotals để cập nhật draftDto.NewCustomerName
                            // Lưu ý: Trong logic này, ta đã gửi newCustomerNameValue lên API, nên ta không cần gọi updateTotals ở đây.
                        });
                    }
                    // Trường hợp 2: Đang có input nhưng SĐT bị xóa/thay đổi
                    else if (data.customerFoundStatus !== 2) {
                        newCustomerContainer.empty().hide();
                    }

                    // Trường hợp 3: SĐT mới nhưng input đã tồn tại, chỉ cần update value
                    else if (data.customerFoundStatus === 2 && !newCustomerContainer.is(':empty')) {
                        $('#newCustomerName').val(newCustomerNameValue); // Đảm bảo giữ giá trị đã nhập
                    }

                } else {
                    console.error("Lỗi tính toán API:", response.message);
                }
            },
            error: function (xhr) {
                console.error('Lỗi server khi tính toán giá.');
            }
        });
    }

    // ... (Giữ nguyên: updateCartUI, btn-add-to-cart, item-quantity, btn-remove-item) ...
    function updateCartUI() {
        $('#cartItems').empty();
        if (cart.length === 0) {
            $('#cartItems').append('<div class="text-center text-muted mt-3" id="emptyCartMessage">Giỏ hàng trống.</div>');
            updateTotals();
            return;
        }

        $('#emptyCartMessage').hide();

        cart.forEach(item => {
            const itemTotal = item.Quantity * item.UnitPrice;
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

        updateTotals();
    }


    // --- 2. XỬ LÝ SỰ KIỆN CART & TÍNH TOÁN (GIỮ NGUYÊN) ---
    $(document).on('click', '.btn-add-to-cart', function () {
        const $item = $(this).closest('.product-item');
        const productId = parseInt($item.data('id'));
        const productName = $item.data('product-name');
        const unitPrice = parseFloat($item.data('price'));

        const existingItem = cart.find(item => item.ProductId === productId);

        if (existingItem) {
            existingItem.Quantity++;
        } else {
            cart.push({ ProductId: productId, ProductName: productName, Quantity: 1, UnitPrice: unitPrice });
        }
        updateCartUI();
    });

    $(document).on('change', '.item-quantity', function () {
        const productId = parseInt($(this).data('product-id'));
        const newQuantity = parseInt($(this).val());
        if (newQuantity <= 0 || isNaN(newQuantity)) { $(this).val(1); return; }
        const item = cart.find(item => item.ProductId === productId);
        if (item) { item.Quantity = newQuantity; }
        updateCartUI();
    });

    $(document).on('click', '.btn-remove-item', function () {
        const productId = parseInt($(this).data('product-id'));
        cart = cart.filter(item => item.ProductId !== productId);
        updateCartUI();
    });

    // D. Thay đổi Discount/Phone (Sử dụng 'input' để kiểm tra liên tục)
    $('#discountPercent, #customerPhone').on('input', function () {
        // Validation cho Discount (Chỉ cho phép số từ 0-100)
        if ($(this).attr('id') === 'discountPercent') {
            let val = parseFloat($(this).val());
            if (isNaN(val) || val < 0) val = 0;
            if (val > 100) val = 100;
            $(this).val(val);
        }

        // Hạn chế nhập ký tự không phải số trong ô phone
        if ($(this).attr('id') === 'customerPhone') {
            let phoneVal = $(this).val().replace(/[^\d]/g, '');
            $(this).val(phoneVal);
        }

        updateTotals();
    });

    // E. Tìm kiếm
    $('#productSearch').on('keyup', function () {
        const query = $(this).val().toLowerCase();
        $('.product-item').each(function () {
            const name = $(this).data('name').toLowerCase(); // Chuyển name sang chữ thường để tìm kiếm không phân biệt hoa thường
            if (name.includes(query)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });


    // --- 3. GỌI API TẠO/CẬP NHẬT BẢN NHÁP (SUBMIT DRAFT) ---

    $('#submitDraft').click(function () {
        if (cart.length === 0) {
            alert('Vui lòng thêm sản phẩm vào giỏ hàng.');
            return;
        }

        let customerPhone = $('#customerPhone').val().replace(/[^\d]/g, ''); // LÀM SẠCH LẠI LẦN CUỐI
        let newCustomerName = '';
        let isNewCustomer = $('#newCustomerNameContainer').is(':visible');

        // Kiểm tra SĐT hợp lệ (10 số) nếu có nhập
        if (customerPhone.length > 0 && customerPhone.length < 10) {
            $('#customerPhone').addClass('is-invalid');
            alert('SĐT phải có ít nhất 10 chữ số.');
            return;
        } else {
            $('#customerPhone').removeClass('is-invalid');
        }

        // Kiểm tra tên khách hàng mới nếu cần
        if (isNewCustomer) {
            newCustomerName = $('#newCustomerName').val().trim();

            if (!newCustomerName) {
                $('#newCustomerName').addClass('is-invalid');
                alert('Vui lòng nhập tên khách hàng mới để tích lũy điểm.');
                return;
            } else {
                $('#newCustomerName').removeClass('is-invalid');
            }
        }

        const draftData = {
            Items: cart,
            CustomerPhone: customerPhone, // Gửi SĐT đã làm sạch
            DiscountPercent: parseFloat($('#discountPercent').val()) || 0,
            Note: $('#orderNote').val(),
            OrderId: currentDraftOrderId,
            NewCustomerName: newCustomerName
        };

        $.ajax({
            url: '/Order/CreateDraft',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(draftData),
            beforeSend: function () {
                $('#submitDraft').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span> Đang xử lý...');
            },
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else {
                    alert('Lỗi tạo/cập nhật bản nháp: ' + response.message);
                    $('#submitDraft').prop('disabled', false).text(currentDraftOrderId ? 'Cập Nhật Bản Nháp' : 'Tạo Bản Nháp & Tiếp tục');
                }
            },
            error: function (xhr) {
                alert('Lỗi server khi tạo bản nháp. Vui lòng thử lại.');
                $('#submitDraft').prop('disabled', false).text(currentDraftOrderId ? 'Cập Nhật Bản Nháp' : 'Tạo Bản Nháp & Tiếp tục');
            }
        });
    });

    loadDraftFromInitialData();
});

