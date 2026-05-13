import React, { useEffect, useMemo, useState } from "react";
import { createRoot } from "react-dom/client";
import { api } from "./api/client";
import "./styles.css";

const subscriptionTypes = [
  { value: 1, label: "Elektrik" },
  { value: 2, label: "Su" },
  { value: 3, label: "İnternet" },
  { value: 4, label: "GSM" },
  { value: 5, label: "Doğalgaz" },
  { value: 99, label: "Diğer" },
];

const statuses = [
  { value: 1, label: "Aktif" },
  { value: 2, label: "Pasif" },
];

function typeLabel(value) {
  return (
    subscriptionTypes.find((x) => x.value === Number(value))?.label || value
  );
}

function statusLabel(value) {
  return Number(value) === 1 ? "Aktif" : "Pasif";
}

function paymentStatusLabel(value) {
  return Number(value) === 1 ? "Başarılı" : "Başarısız";
}

function App() {
  const [customers, setCustomers] = useState([]);
  const [selectedCustomerId, setSelectedCustomerId] = useState("");
  const [subscriptions, setSubscriptions] = useState([]);
  const [payments, setPayments] = useState([]);
  const [summary, setSummary] = useState(null);
  const [debtResult, setDebtResult] = useState(null);
  const [reminders, setReminders] = useState([]);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const selectedCustomer = useMemo(
    () => customers.find((c) => c.id === Number(selectedCustomerId)),
    [customers, selectedCustomerId],
  );

  async function loadCustomers() {
    const data = await api.getCustomers();
    setCustomers(data);
    if (!selectedCustomerId && data.length > 0)
      setSelectedCustomerId(String(data[0].id));
  }

  async function loadCustomerData(customerId = selectedCustomerId) {
    if (!customerId) return;
    const [subs, pays, sum] = await Promise.all([
      api.getSubscriptions(customerId),
      api.getPayments(customerId),
      api.getSummary(customerId),
    ]);
    setSubscriptions(subs);
    setPayments(pays);
    setSummary(sum);
  }

  async function runAction(action, successText) {
    setLoading(true);
    setMessage("");
    try {
      const result = await action();
      setMessage(successText);
      return result;
    } catch (error) {
      setMessage(error.message || "Bir hata oluştu.");
      return null;
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    runAction(loadCustomers, "Müşteri listesi yüklendi.");
  }, []);

  useEffect(() => {
    if (selectedCustomerId) {
      setDebtResult(null);
      setReminders([]);
      runAction(
        () => loadCustomerData(selectedCustomerId),
        "Müşteri verileri güncellendi.",
      );
    }
  }, [selectedCustomerId]);

  async function handleCustomerCreate(event) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const created = await runAction(
      () =>
        api.createCustomer({
          fullName: form.get("fullName"),
          email: form.get("email"),
          phoneNumber: form.get("phoneNumber"),
        }),
      "Müşteri oluşturuldu.",
    );
    if (created) {
      event.currentTarget.reset();
      await loadCustomers();
      setSelectedCustomerId(String(created.id));
    }
  }

  async function handleSubscriptionCreate(event) {
    event.preventDefault();
    if (!selectedCustomerId) return;
    const form = new FormData(event.currentTarget);
    const created = await runAction(
      () =>
        api.createSubscription({
          customerId: Number(selectedCustomerId),
          type: Number(form.get("type")),
          providerName: form.get("providerName"),
          subscriptionNumber: form.get("subscriptionNumber"),
          status: Number(form.get("status")),
        }),
      "Abonelik eklendi.",
    );
    if (created) {
      event.currentTarget.reset();
      await loadCustomerData();
    }
  }

  async function toggleSubscription(subscription) {
    await runAction(
      () =>
        api.updateSubscription(subscription.id, {
          type: subscription.type,
          providerName: subscription.providerName,
          subscriptionNumber: subscription.subscriptionNumber,
          status: subscription.status === 1 ? 2 : 1,
        }),
      "Abonelik durumu güncellendi.",
    );
    await loadCustomerData();
  }

  async function deleteSubscription(id) {
    await runAction(() => api.deleteSubscription(id), "Abonelik silindi.");
    await loadCustomerData();
  }

  async function queryDebt(subscriptionId) {
    const debt = await runAction(
      () => api.queryDebt(subscriptionId),
      "Borç sorgulandı.",
    );
    if (debt) setDebtResult(debt);
  }

  async function pay(subscriptionId) {
    const payment = await runAction(
      () => api.createPayment(subscriptionId),
      "Ödeme işlemi kaydedildi.",
    );
    if (payment) await loadCustomerData();
  }

  async function checkReminders() {
    if (!selectedCustomerId) return;
    const result = await runAction(
      () => api.checkReminders(selectedCustomerId),
      "Hatırlatma kontrolü tamamlandı.",
    );
    if (result) setReminders(result);
  }

  return (
    <main className="page">
      <header className="hero">
        <div>
          <p className="eyebrow">Bankacılık Case Study</p>
          <h1>Abonelik & Otomatik Ödeme Hatırlatma</h1>
          <p>
            Müşteri, abonelik, borç sorgulama, ödeme ve hatırlatma süreçlerini
            tek ekranda yönetin.
          </p>
        </div>
        <div className="statusBox">
          {loading ? "İşlem yapılıyor..." : message || "Hazır"}
        </div>
      </header>

      <section className="grid two">
        <div className="card">
          <h2>Müşteri Yönetimi</h2>
          <form onSubmit={handleCustomerCreate} className="form">
            <input name="fullName" placeholder="Ad Soyad" required />
            <input name="email" type="email" placeholder="E-posta" required />
            <input name="phoneNumber" placeholder="Telefon" />
            <button type="submit">Müşteri Oluştur</button>
          </form>
          <label className="fieldLabel">Aktif müşteri</label>
          <select
            value={selectedCustomerId}
            onChange={(e) => setSelectedCustomerId(e.target.value)}
          >
            {customers.map((customer) => (
              <option key={customer.id} value={customer.id}>
                {customer.fullName} - {customer.email}
              </option>
            ))}
          </select>
        </div>

        <div className="card summary">
          <h2>Özet</h2>
          {summary ? (
            <div className="metrics">
              <div>
                <strong>{summary.activeSubscriptionCount}</strong>
                <span>Aktif abonelik</span>
              </div>
              <div>
                <strong>{summary.unpaidCurrentMonthSubscriptionCount}</strong>
                <span>Bu ay ödenmemiş</span>
              </div>
              <div>
                <strong>
                  {summary.successfulPaymentTotal.toLocaleString("tr-TR")} TL
                </strong>
                <span>Başarılı toplam ödeme</span>
              </div>
            </div>
          ) : (
            <p>Özet bilgisi yok.</p>
          )}
          <button onClick={checkReminders} disabled={!selectedCustomerId}>
            Hatırlatma Kontrol Et
          </button>
        </div>
      </section>

      <section className="grid two">
        <div className="card">
          <h2>Abonelik Ekle</h2>
          <form onSubmit={handleSubscriptionCreate} className="form">
            <select name="type" defaultValue="1">
              {subscriptionTypes.map((t) => (
                <option key={t.value} value={t.value}>
                  {t.label}
                </option>
              ))}
            </select>
            <input
              name="providerName"
              placeholder="Hizmet sağlayıcı"
              required
            />
            <input
              name="subscriptionNumber"
              placeholder="Abonelik / müşteri no"
              required
            />
            <select name="status" defaultValue="1">
              {statuses.map((s) => (
                <option key={s.value} value={s.value}>
                  {s.label}
                </option>
              ))}
            </select>
            <button type="submit" disabled={!selectedCustomerId}>
              Abonelik Ekle
            </button>
          </form>
        </div>

        <div className="card">
          <h2>Borç Sorgulama Sonucu</h2>
          {debtResult ? (
            <div className="debt">
              <strong>{debtResult.amount.toLocaleString("tr-TR")} TL</strong>
              <p>Dönem: {debtResult.period}</p>
              <p>
                Son ödeme:{" "}
                {new Date(debtResult.dueDate).toLocaleDateString("tr-TR")}
              </p>
              <p>
                Ödeme durumu:{" "}
                {debtResult.isPaidForPeriod ? "Bu dönem ödenmiş" : "Ödenmemiş"}
              </p>
            </div>
          ) : (
            <p className="muted">Bir abonelik için “Borç Sorgula” seçin.</p>
          )}
        </div>
      </section>

      <section className="card">
        <h2>Abonelikler</h2>
        <div className="tableWrap">
          <table>
            <thead>
              <tr>
                <th>Tür</th>
                <th>Sağlayıcı</th>
                <th>No</th>
                <th>Durum</th>
                <th>İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {subscriptions.map((subscription) => (
                <tr key={subscription.id}>
                  <td>{typeLabel(subscription.type)}</td>
                  <td>{subscription.providerName}</td>
                  <td>{subscription.subscriptionNumber}</td>
                  <td>
                    <span
                      className={
                        subscription.status === 1
                          ? "pill active"
                          : "pill passive"
                      }
                    >
                      {statusLabel(subscription.status)}
                    </span>
                  </td>
                  <td className="actions">
                    <button onClick={() => queryDebt(subscription.id)}>
                      Borç Sorgula
                    </button>
                    <button onClick={() => pay(subscription.id)}>Öde</button>
                    <button onClick={() => toggleSubscription(subscription)}>
                      Aktif/Pasif
                    </button>
                    <button
                      className="danger"
                      onClick={() => deleteSubscription(subscription.id)}
                    >
                      Sil
                    </button>
                  </td>
                </tr>
              ))}
              {subscriptions.length === 0 && (
                <tr>
                  <td colSpan="5">Abonelik bulunamadı.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section className="grid two">
        <div className="card">
          <h2>Ödeme Geçmişi</h2>
          <div className="list">
            {payments.map((payment) => (
              <div className="listItem" key={payment.id}>
                <strong>
                  {payment.providerName} - {payment.period}
                </strong>
                <span>
                  {payment.amount.toLocaleString("tr-TR")} TL /{" "}
                  {paymentStatusLabel(payment.status)}
                </span>
                {payment.failureReason && (
                  <small>{payment.failureReason}</small>
                )}
              </div>
            ))}
            {payments.length === 0 && <p className="muted">Ödeme kaydı yok.</p>}
          </div>
        </div>

        <div className="card">
          <h2>Hatırlatma Sonuçları</h2>
          <div className="list">
            {reminders.map((reminder) => (
              <div className="listItem" key={reminder.subscriptionId}>
                <strong>
                  {reminder.providerName} - {reminder.period}
                </strong>
                <span>{reminder.message}</span>
                <small>
                  {reminder.amount.toLocaleString("tr-TR")} TL /{" "}
                  {new Date(reminder.dueDate).toLocaleDateString("tr-TR")}
                </small>
              </div>
            ))}
            {reminders.length === 0 && (
              <p className="muted">Henüz hatırlatma kontrolü yapılmadı.</p>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}

createRoot(document.getElementById("root")).render(<App />);
