document.addEventListener("DOMContentLoaded", async () => {
    const tg = window.Telegram?.WebApp;

    if (tg) {
        tg.ready();
        tg.expand();
        tg.setHeaderColor?.('secondary_bg_color');
    }

    const initData = tg?.initData || "";

    // 1. Отображаем имя пользователя
    const userNameElement = document.getElementById("user-name");
    if (tg?.initDataUnsafe?.user) {
        const user = tg.initDataUnsafe.user;
        userNameElement.textContent = `${user.first_name} ${user.last_name || ''}`.trim();
    } else {
        userNameElement.textContent = "Локальный режим (без Telegram)";
    }

    // Вспомогательный метод для выполнения авторизованных запросов
    async function apiFetch(url, options = {}) {
        options.headers = {
            ...options.headers,
            "Content-Type": "application/json",
            "X-Telegram-Init-Data": initData
        };
        return await fetch(url, options);
    }

    // 2. Функция загрузки списка платежей
    async function loadPayments() {
        const listContainer = document.getElementById("payments-list");
        try {
            const response = await apiFetch("/api/payments?page=1&pageSize=10");

            if (!response.ok) {
                listContainer.innerHTML = `<p class="empty-text">Ошибка загрузки истории (${response.status})</p>`;
                return;
            }

            const data = await response.json();
            const payments = data.items || data; // поддерживает и пагинированный ответ, и обычный массив

            if (!payments || payments.length === 0) {
                listContainer.innerHTML = `<p class="empty-text">Платежей пока нет</p>`;
                return;
            }

            listContainer.innerHTML = payments.map(p => `
                <div style="display:flex; justify-content:space-between; align-items:center; padding: 10px 0; border-bottom: 1px solid rgba(0,0,0,0.05);">
                    <div>
                        <div style="font-weight:600;">${p.amount} ${p.currency}</div>
                        <div style="font-size:0.8rem; color:var(--hint-color);">${p.description || 'Без описания'}</div>
                    </div>
                    <div style="font-size:0.85rem; padding: 4px 8px; border-radius: 6px; background: rgba(0,0,0,0.05);">
                        ${p.status}
                    </div>
                </div>
            `).join('');
        } catch (err) {
            console.error("Ошибка загрузки платежей:", err);
            listContainer.innerHTML = `<p class="empty-text">Ошибка сети при загрузке</p>`;
        }
    }

    // Первичная загрузка истории
    await loadPayments();

    // 3. Обработка отправки формы создания платежа
    const form = document.getElementById("payment-form");
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        tg?.HapticFeedback?.impactOccurred("medium");

        const amount = parseFloat(document.getElementById("amount").value);
        const currency = document.getElementById("currency").value;
        const description = document.getElementById("description").value;

        const payBtn = document.getElementById("pay-btn");
        payBtn.disabled = true;
        payBtn.textContent = "Обработка...";

        try {
            const response = await apiFetch("/api/payments", {
                method: "POST",
                body: JSON.stringify({
                    amount: amount,
                    currency: currency,
                    description: description,
                    idempotencyKey: crypto.randomUUID()
                })
            });

            if (response.ok) {
                tg?.HapticFeedback?.notificationOccurred("success");
                tg?.showAlert("Платёж успешно создан!", () => {
                    form.reset();
                    payBtn.disabled = false;
                    payBtn.textContent = "Оплатить";
                    loadPayments(); // Перерисовываем историю
                });
            } else {
                const errorData = await response.json().catch(() => null);
                const errorMsg = errorData?.detail || errorData?.title || `Ошибка ${response.status}`;

                tg?.HapticFeedback?.notificationOccurred("error");
                tg?.showAlert(`Не удалось создать платёж: ${errorMsg}`, () => {
                    payBtn.disabled = false;
                    payBtn.textContent = "Оплатить";
                });
            }
        } catch (err) {
            tg?.HapticFeedback?.notificationOccurred("error");
            tg?.showAlert("Сетевая ошибка при запросе к серверу.", () => {
                payBtn.disabled = false;
                payBtn.textContent = "Оплатить";
            });
        }
    });
});