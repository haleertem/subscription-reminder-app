const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json", ...(options.headers || {}) },
    ...options,
  });

  if (response.status === 204) return null;

  const text = await response.text();
  let data = null;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (!response.ok) {
    const message =
      typeof data === "string"
        ? data
        : data?.title || data?.message || text || "İşlem başarısız.";
    throw new Error(message);
  }

  return data;
}

export const api = {
  getCustomers: () => request("/customers"),
  createCustomer: (payload) =>
    request("/customers", { method: "POST", body: JSON.stringify(payload) }),
  deleteCustomer: (id) => request(`/customers/${id}`, { method: "DELETE" }),
  //update customer
  updateCustomer: (id, payload) =>
    request(`/customers/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  getSubscriptions: (customerId) =>
    request(`/subscriptions${customerId ? `?customerId=${customerId}` : ""}`),
  createSubscription: (payload) =>
    request("/subscriptions", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updateSubscription: (id, payload) =>
    request(`/subscriptions/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),
  deleteSubscription: (id) =>
    request(`/subscriptions/${id}`, { method: "DELETE" }),
  queryDebt: (subscriptionId) =>
    request(`/subscriptions/${subscriptionId}/debt`),

  getPayments: (customerId) =>
    request(`/payments${customerId ? `?customerId=${customerId}` : ""}`),
  createPayment: (subscriptionId) =>
    request("/payments", {
      method: "POST",
      body: JSON.stringify({ subscriptionId }),
    }),

  checkReminders: (customerId) =>
    request(`/reminders/customers/${customerId}/check?days=5`),
  getSummary: (customerId) => request(`/customers/${customerId}/summary`),
};
