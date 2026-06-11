(async function () {
    const response = await fetch('/api/chatbot/config');
    const config = await response.json();

    if (!config.baseUrl || !config.websiteToken) {
        console.warn('Chatwoot is not configured. Add Chatwoot:BaseUrl and Chatwoot:WebsiteToken.');
        return;
    }

    window.chatwootSettings = { hideMessageBubble: false, position: 'right', locale: 'en' };

    const script = document.createElement('script');
    script.src = `${config.baseUrl}/packs/js/sdk.js`;
    script.defer = true;
    script.async = true;
    script.onload = function () {
        window.chatwootSDK.run({ websiteToken: config.websiteToken, baseUrl: config.baseUrl });
    };
    document.head.appendChild(script);
})();
