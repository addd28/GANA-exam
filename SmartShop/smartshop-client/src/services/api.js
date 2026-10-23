const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

async function request(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {})
  };

  const response = await fetch(url, { ...options, headers });

  if (!response.ok) {
    let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
    try {
      const data = await response.json();
      if (data && data.message) {
        errorMessage = data.message;
      } else if (data && data.errors) {
        errorMessage = Object.values(data.errors).flat().join(', ');
      }
    } catch {
      // Non-JSON response
    }
    const error = new Error(errorMessage);
    error.status = response.status;
    throw error;
  }

  return response.json();
}

export const apiService = {
  // Question 4: Review report
  getReviewReport: () => request('/api/reports/reviews'),

  // Products catalog
  getProducts: () => request('/api/products'),
  getProductById: (id) => request(`/api/products/${id}`),

  // Question 1: Generate description
  generateProductDescription: (id) =>
    request(`/api/products/${id}/generatedescription`, { method: 'POST' }),

  // Question 2: Submit & analyze review
  analyzeReview: (data) =>
    request('/api/reviews/analyze', {
      method: 'POST',
      body: JSON.stringify(data)
    }),

  // Question 3: Shopping Assistant chat
  chatWithAssistant: (sessionId, message) =>
    request('/api/assistant/chat', {
      method: 'POST',
      body: JSON.stringify({ sessionId, message })
    }),

  // Chat history
  getChatHistory: (sessionId) =>
    request(`/api/assistant/history/${encodeURIComponent(sessionId)}`),

  clearChatHistory: (sessionId) =>
    request(`/api/assistant/history/${encodeURIComponent(sessionId)}`, {
      method: 'DELETE'
    })
};
