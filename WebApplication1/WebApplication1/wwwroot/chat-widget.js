(function () {
    if (window.__drdChatWidgetLoaded) {
        return;
    }
    window.__drdChatWidgetLoaded = true;

    const STYLE_HREF = '/chat-widget.css';
    const CHAT_ICON_SVG = '<svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">' +
        '<path d="M4 4.5A2.5 2.5 0 0 1 6.5 2h11A2.5 2.5 0 0 1 20 4.5v9a2.5 2.5 0 0 1-2.5 2.5H10l-4.5 4v-4H6.5A2.5 2.5 0 0 1 4 13.5v-9Z" fill="currentColor"/>' +
        '</svg>';

    const state = {
        open: false,
        view: 'list',
        conversationId: null,
        conversations: [],
        messages: [],
        loading: false
    };

    let ui = null;

    function getToken() {
        return localStorage.getItem('accessToken');
    }

    function injectStyles() {
        if (document.querySelector('link[href="' + STYLE_HREF + '"]')) {
            return;
        }

        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = STYLE_HREF;
        document.head.appendChild(link);
    }

    function el(tag, options) {
        options = options || {};
        const node = document.createElement(tag);

        if (options.className) {
            node.className = options.className;
        }

        if (options.text !== undefined) {
            node.textContent = options.text;
        }

        if (options.html !== undefined) {
            node.innerHTML = options.html;
        }

        if (options.attrs) {
            Object.keys(options.attrs).forEach(function (key) {
                node.setAttribute(key, options.attrs[key]);
            });
        }

        return node;
    }

    function formatPrice(value) {
        const numeric = Number(value);
        return Number.isFinite(numeric) ? '$' + numeric.toFixed(2) : '';
    }

    async function apiRequest(path, options) {
        options = options || {};
        const token = getToken();
        const headers = Object.assign(
            { 'Content-Type': 'application/json' },
            token ? { Authorization: 'Bearer ' + token } : {},
            options.headers || {}
        );

        const response = await fetch(path, Object.assign({}, options, { headers: headers }));

        if (response.status === 204) {
            return null;
        }

        const data = await response.json().catch(function () { return null; });

        if (!response.ok) {
            throw data || { title: 'Request failed', status: response.status };
        }

        return data;
    }

    function buildWidget() {
        const root = el('div', { className: 'drd-chat-widget' });

        const button = el('button', { className: 'drd-chat-button', html: CHAT_ICON_SVG, attrs: { type: 'button', 'aria-label': 'Open chat' } });

        const panel = el('div', { className: 'drd-chat-panel' });

        const header = el('div', { className: 'drd-chat-header' });
        const backButton = el('button', { className: 'drd-chat-back', text: '←', attrs: { type: 'button', 'aria-label': 'Back to conversations' } });
        const title = el('span', { className: 'drd-chat-title', text: 'Dream Room Assistant' });
        const newButton = el('button', { className: 'drd-chat-new', text: '+', attrs: { type: 'button', 'aria-label': 'New conversation', title: 'New conversation' } });
        const closeButton = el('button', { className: 'drd-chat-close', text: '×', attrs: { type: 'button', 'aria-label': 'Close chat' } });
        backButton.hidden = true;
        header.append(backButton, title, newButton, closeButton);

        const body = el('div', { className: 'drd-chat-body' });

        const inputBar = el('div', { className: 'drd-chat-input-bar' });
        const textarea = el('textarea', { className: 'drd-chat-input', attrs: { rows: '1', placeholder: 'Ask about furniture, rooms, or budgets...' } });
        const sendButton = el('button', { className: 'drd-chat-send', text: 'Send', attrs: { type: 'button' } });
        inputBar.append(textarea, sendButton);
        inputBar.hidden = true;

        panel.append(header, body, inputBar);
        root.append(button, panel);
        document.body.appendChild(root);

        return {
            root: root,
            button: button,
            panel: panel,
            backButton: backButton,
            title: title,
            newButton: newButton,
            closeButton: closeButton,
            body: body,
            inputBar: inputBar,
            textarea: textarea,
            sendButton: sendButton
        };
    }

    function setOpen(open) {
        state.open = open;
        ui.panel.classList.toggle('drd-open', open);

        if (!open) {
            return;
        }

        if (!getToken()) {
            renderSignInPrompt();
            return;
        }

        if (state.view === 'chat') {
            renderMessages();
        } else {
            loadConversations();
        }
    }

    function renderSignInPrompt() {
        ui.backButton.hidden = true;
        ui.newButton.hidden = true;
        ui.inputBar.hidden = true;
        ui.title.textContent = 'Dream Room Assistant';
        ui.body.innerHTML = '';
        ui.body.appendChild(el('div', { className: 'drd-chat-signin', text: 'Please sign in to chat with our assistant.' }));
    }

    function renderLoading() {
        ui.body.innerHTML = '';
        ui.body.appendChild(el('div', { className: 'drd-conv-empty', text: 'Loading...' }));
    }

    function renderError(message) {
        ui.body.innerHTML = '';
        ui.body.appendChild(el('div', { className: 'drd-conv-empty', text: message }));
    }

    async function loadConversations() {
        state.view = 'list';
        ui.backButton.hidden = true;
        ui.newButton.hidden = false;
        ui.inputBar.hidden = true;
        ui.title.textContent = 'Dream Room Assistant';
        renderLoading();

        try {
            state.conversations = await apiRequest('/api/chat/conversations');
            renderConversationList();
        } catch (error) {
            renderError('Could not load your conversations.');
        }
    }

    function renderConversationList() {
        ui.body.innerHTML = '';

        if (!state.conversations || state.conversations.length === 0) {
            ui.body.appendChild(el('div', {
                className: 'drd-conv-empty',
                text: 'No conversations yet. Start a new chat to ask about furniture, rooms, or budgets.'
            }));
            return;
        }

        state.conversations.forEach(function (conversation) {
            const item = el('div', { className: 'drd-conv-item' });
            item.appendChild(el('div', { className: 'drd-conv-title', text: conversation.title || 'New conversation' }));

            if (conversation.lastMessagePreview) {
                item.appendChild(el('div', { className: 'drd-conv-preview', text: conversation.lastMessagePreview }));
            }

            item.addEventListener('click', function () { openConversation(conversation.id); });
            ui.body.appendChild(item);
        });
    }

    function startNewConversation() {
        state.view = 'chat';
        state.conversationId = null;
        state.messages = [];
        ui.title.textContent = 'New conversation';
        ui.backButton.hidden = false;
        ui.newButton.hidden = false;
        ui.inputBar.hidden = false;
        renderMessages();
        ui.textarea.focus();
    }

    async function openConversation(id) {
        state.view = 'chat';
        state.conversationId = id;
        ui.backButton.hidden = false;
        ui.newButton.hidden = false;
        ui.inputBar.hidden = false;
        renderLoading();

        try {
            const conversation = await apiRequest('/api/chat/conversations/' + id);
            state.messages = conversation.messages || [];
            ui.title.textContent = conversation.title || 'Conversation';
            renderMessages();
        } catch (error) {
            renderError('Could not load this conversation.');
        }
    }

    function renderMessages() {
        ui.body.innerHTML = '';

        if (state.messages.length === 0) {
            ui.body.appendChild(el('div', {
                className: 'drd-conv-empty',
                text: 'Ask me about furniture, room ideas, or products within your budget.'
            }));
            return;
        }

        state.messages.forEach(renderMessage);
        scrollToBottom();
    }

    function renderMessage(message) {
        const bubble = el('div', {
            className: 'drd-msg ' + (message.role === 'user' ? 'drd-msg-user' : 'drd-msg-assistant'),
            text: message.content
        });
        ui.body.appendChild(bubble);

        if (message.recommendedProducts && message.recommendedProducts.length > 0) {
            const list = el('div', { className: 'drd-products' });
            message.recommendedProducts.forEach(function (product) {
                list.appendChild(renderProductCard(product));
            });
            ui.body.appendChild(list);
        }
    }

    function renderProductCard(product) {
        const card = el('div', { className: 'drd-product-card' });
        const img = el('img', { attrs: { alt: product.name || '', src: product.imageUrl || '' } });
        const info = el('div', { className: 'drd-product-info' });
        info.appendChild(el('div', { className: 'drd-product-name', text: product.name || '' }));
        info.appendChild(el('div', { className: 'drd-product-meta', text: product.category || '' }));
        info.appendChild(el('div', { className: 'drd-product-price', text: formatPrice(product.price) }));
        card.append(img, info);
        return card;
    }

    function scrollToBottom() {
        ui.body.scrollTop = ui.body.scrollHeight;
    }

    function showTypingIndicator() {
        const typing = el('div', { className: 'drd-typing', attrs: { id: 'drd-typing-indicator' } });
        typing.appendChild(el('span'));
        typing.appendChild(el('span'));
        typing.appendChild(el('span'));
        ui.body.appendChild(typing);
        scrollToBottom();
    }

    function hideTypingIndicator() {
        const typing = document.getElementById('drd-typing-indicator');
        if (typing) {
            typing.remove();
        }
    }

    async function sendMessage() {
        const text = ui.textarea.value.trim();
        if (!text || state.loading) {
            return;
        }

        if (!getToken()) {
            renderSignInPrompt();
            return;
        }

        ui.textarea.value = '';
        ui.textarea.style.height = 'auto';

        state.messages.push({ role: 'user', content: text, recommendedProducts: [], createdAt: new Date().toISOString() });
        renderMessages();
        showTypingIndicator();

        state.loading = true;
        ui.sendButton.disabled = true;

        try {
            const response = await apiRequest('/api/chat/message', {
                method: 'POST',
                body: JSON.stringify({ conversationId: state.conversationId, message: text })
            });

            state.conversationId = response.conversationId;
            hideTypingIndicator();

            state.messages.push({
                role: 'assistant',
                content: response.message,
                recommendedProducts: response.recommendedProducts || [],
                createdAt: response.createdAt
            });
            renderMessages();
        } catch (error) {
            hideTypingIndicator();
            state.messages.push({
                role: 'assistant',
                content: (error && (error.title || error.message)) || 'Sorry, something went wrong. Please try again.',
                recommendedProducts: [],
                createdAt: new Date().toISOString()
            });
            renderMessages();
        } finally {
            state.loading = false;
            ui.sendButton.disabled = false;
        }
    }

    function init() {
        injectStyles();
        ui = buildWidget();

        ui.button.addEventListener('click', function () { setOpen(!state.open); });
        ui.closeButton.addEventListener('click', function () { setOpen(false); });
        ui.backButton.addEventListener('click', loadConversations);
        ui.newButton.addEventListener('click', startNewConversation);
        ui.sendButton.addEventListener('click', sendMessage);

        ui.textarea.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' && !event.shiftKey) {
                event.preventDefault();
                sendMessage();
            }
        });

        ui.textarea.addEventListener('input', function () {
            ui.textarea.style.height = 'auto';
            ui.textarea.style.height = Math.min(ui.textarea.scrollHeight, 90) + 'px';
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
