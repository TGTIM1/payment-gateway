document.addEventListener("DOMContentLoaded", async () => {
    const tg = window.Telegram?.WebApp;

    if (tg) {
        tg.ready();
        tg.expand();
        tg.setHeaderColor?.('secondary_bg_color');
    }

    const initData = tg?.initData || "";

    const userNameElement = document.getElementById("user-name");
    if (tg?.initDataUnsafe?.user) {
        const user = tg.initDataUnsafe.user;
        userNameElement.textContent = `${user.first_name} ${user.last_name || ''}`.trim();
    } else {
        userNameElement.textContent = "Локальный режим (без Telegram)";
    }

    async function apiFetch(url, options = {}) {
        options.headers = {
            ...options.headers,
            "Content-Type": "application/json",
            "X-Telegram-Init-Data": initData
        };
        return await fetch(url, options);
    }

    let pollingTimer = null;

    async function loadPayments() {
        const listContainer = document.getElementById("payments-list");
        try {
            const userId = tg?.initDataUnsafe?.user?.id || 100;

            const response = await apiFetch(`/api/payments?telegramUserId=${userId}&page=1&pageSize=10`);

            if (!response.ok) {
                listContainer.innerHTML = `<p class="empty-text">Ошибка загрузки истории (${response.status})</p>`;
                return;
            }

            const data = await response.json();
            const payments = data.items || data;

            if (!payments || payments.length === 0) {
                listContainer.innerHTML = `<p class="empty-text">Платежей пока нет</p>`;
                return;
            }

            listContainer.innerHTML = payments.map(p => {
                let statusColor = 'var(--hint-color, #888)';
                let statusBg = 'rgba(0, 0, 0, 0.05)';
                let borderColor = 'rgba(0, 0, 0, 0.05)';

                const statusStr = String(p.status).toLowerCase();

                if (statusStr === 'completed' || statusStr === 'succeeded' || statusStr === '2') {
                    statusColor = '#2e7d32';
                    statusBg = '#e8f5e9';
                    borderColor = '#4caf50';
                } else if (statusStr === 'pending' || statusStr === 'processing' || statusStr === '0' || statusStr === '1') {
                    statusColor = '#ed6c02';
                    statusBg = '#fff3e0';
                    borderColor = '#ff9800';
                } else if (statusStr === 'failed' || statusStr === 'cancelled' || statusStr === '3') {
                    statusColor = '#d32f2f';
                    statusBg = '#ffebee';
                    borderColor = '#f44336';
                }

                return `
                    <div style="display:flex; justify-content:space-between; align-items:center; padding: 10px 12px; margin-bottom: 8px; border: 1.5px solid ${borderColor}; border-radius: 8px; transition: all 0.3s ease;">
                        <div>
                            <div style="font-weight:600;">${p.amount} ${p.currency}</div>
                            <div style="font-size:0.8rem; color:var(--hint-color);">${p.description || 'Без описания'}</div>
                        </div>
                        <div style="font-size:0.85rem; font-weight:600; padding: 4px 10px; border-radius: 6px; color: ${statusColor}; background: ${statusBg};">
                            ${p.status}
                        </div>
                    </div>
                `;
            }).join('');

            const hasPending = payments.some(p => {
                const statusStr = String(p.status).toLowerCase();
                return statusStr === 'pending' || statusStr === 'processing' || statusStr === '0' || statusStr === '1';
            });

            if (hasPending && !pollingTimer) {
                pollingTimer = setInterval(loadPayments, 3000);
            }
            else if (!hasPending && pollingTimer) {
                clearInterval(pollingTimer);
                pollingTimer = null;
            }

        } catch (err) {
            console.error("Ошибка загрузки платежей:", err);
            listContainer.innerHTML = `<p class="empty-text">Ошибка сети при загрузке</p>`;
        }
    }

    await loadPayments();

    const form = document.getElementById("payment-form");
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        tg?.HapticFeedback?.impactOccurred("medium");

        const amount = parseFloat(document.getElementById("amount").value);
        const currency = document.getElementById("currency").value;
        const description = document.getElementById("description").value;
        const userId = tg?.initDataUnsafe?.user?.id || 100;

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
                    idempotencyKey: crypto.randomUUID(),
                    telegramUserId: userId
                })
            });

            if (response.ok) {
                tg?.HapticFeedback?.notificationOccurred("success");
                tg?.showAlert("Платёж успешно создан!", () => {
                    form.reset();
                    payBtn.disabled = false;
                    payBtn.textContent = "Оплатить";
                    loadPayments();
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