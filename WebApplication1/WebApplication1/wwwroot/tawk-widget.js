(async function () {
    const configResponse = await fetch('/api/chatbot/config');
    const config = await configResponse.json();

    if (!config.isConfigured || !config.embedUrl) {
        console.warn('Tawk is not configured. Add Tawk:PropertyId and Tawk:WidgetId.');
        return;
    }

    window.Tawk_API = window.Tawk_API || {};
    window.Tawk_LoadStart = new Date();

    window.Tawk_API.onLoad = async function () {
        const token = localStorage.getItem('accessToken');
        if (!token || typeof window.Tawk_API.setAttributes !== 'function') {
            return;
        }

        try {
            const contextResponse = await fetch('/api/chatbot/context', {
                headers: { Authorization: `Bearer ${token}` }
            });

            if (!contextResponse.ok) {
                return;
            }

            const context = await contextResponse.json();
            const cartTotal = context.cartItems.reduce((total, item) => total + item.price * item.quantity, 0);
            const latestOrder = context.recentOrders[0];
            const latestRoomDesign = context.roomDesigns[0];

            window.Tawk_API.visitor = {
                name: context.user.fullName,
                email: context.user.email
            };

            window.Tawk_API.setAttributes({
                userId: String(context.user.id),
                role: context.user.role,
                cartItemsCount: String(context.cartItems.length),
                cartTotal: cartTotal.toFixed(2),
                cartProducts: context.cartItems.map(item => `${item.productName} x${item.quantity}`).join(', ').slice(0, 255),
                latestOrderId: latestOrder ? String(latestOrder.orderId) : '',
                latestOrderStatus: latestOrder ? latestOrder.status : '',
                latestRoomDesignId: latestRoomDesign ? String(latestRoomDesign.roomDesignId) : '',
                latestRoomType: latestRoomDesign ? latestRoomDesign.roomType : ''
            }, function (error) {
                if (error) {
                    console.warn('Failed to set Tawk attributes.', error);
                }
            });
        } catch (error) {
            console.warn('Failed to load chatbot context.', error);
        }
    };

    const script = document.createElement('script');
    script.async = true;
    script.src = config.embedUrl;
    script.charset = 'UTF-8';
    script.setAttribute('crossorigin', '*');
    document.head.appendChild(script);
})();
