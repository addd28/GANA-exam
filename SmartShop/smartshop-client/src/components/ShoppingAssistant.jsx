import React, { useState, useEffect, useRef } from 'react';
import { apiService } from '../services/api';

export default function ShoppingAssistant() {
  const [sessionId, setSessionId] = useState('session-001');
  const [messages, setMessages] = useState([]);
  const [inputMessage, setInputMessage] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetchingHistory, setFetchingHistory] = useState(false);
  const [error, setError] = useState(null);
  const chatBottomRef = useRef(null);

  const loadHistory = async (sid) => {
    if (!sid) return;
    setFetchingHistory(true);
    setError(null);
    try {
      const history = await apiService.getChatHistory(sid);
      setMessages(history.map(m => ({
        id: m.id,
        role: m.role,
        content: m.content,
        createdAt: m.createdAt
      })));
    } catch {
      // If error or empty, start fresh
      setMessages([]);
    } finally {
      setFetchingHistory(false);
    }
  };

  useEffect(() => {
    loadHistory(sessionId);
  }, [sessionId]);

  useEffect(() => {
    chatBottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, loading]);

  const handleSendMessage = async (textToSend) => {
    const text = (textToSend || inputMessage).trim();
    if (!text || loading) return;

    setInputMessage('');
    setError(null);

    // Optimistically show user message
    const tempUserMsg = {
      id: Date.now(),
      role: 'user',
      content: text,
      createdAt: new Date().toISOString()
    };
    setMessages(prev => [...prev, tempUserMsg]);
    setLoading(true);

    try {
      const response = await apiService.chatWithAssistant(sessionId, text);
      const assistantMsg = {
        id: Date.now() + 1,
        role: 'model',
        content: response.reply,
        createdAt: response.createdAt || new Date().toISOString()
      };
      setMessages(prev => [...prev, assistantMsg]);
    } catch (err) {
      setError(err.message || 'Error communicating with Gemini assistant');
      // Remove temporary message if sending failed
      setMessages(prev => prev.filter(m => m.id !== tempUserMsg.id));
    } finally {
      setLoading(false);
    }
  };

  const handleClearHistory = async () => {
    if (!sessionId) return;
    try {
      await apiService.clearChatHistory(sessionId);
      setMessages([]);
      setError(null);
    } catch (err) {
      setError(err.message || 'Failed to clear session history');
    }
  };

  const sampleQuestions = [
    'Which phone has the biggest battery?',
    'How much does it cost?',
    'What features does the MacBook Air M4 have?',
    'Can you tell me the weather in Tokyo?' // Unrelated test
  ];

  return (
    <div className="card chat-card">
      <div className="card-header flex justify-between items-center">
        <div>
          <h2 className="card-title">AI Shopping Assistant</h2>
          <p className="card-subtitle">
            Endpoint: <code className="endpoint-tag">POST /api/assistant/chat</code>
            <br />
            Context-grounded on SQL Server product catalog, persistent session history, strictly refuses unrelated topics.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <label className="text-xs text-muted" htmlFor="session-id-input">Session ID:</label>
          <input
            id="session-id-input"
            type="text"
            className="form-control text-xs"
            style={{ width: '130px', padding: '4px 8px' }}
            value={sessionId}
            onChange={(e) => setSessionId(e.target.value)}
          />
          <button
            onClick={handleClearHistory}
            className="btn btn-secondary text-xs"
            title="Clear chat history in database for this session"
          >
            Clear
          </button>
        </div>
      </div>

      {/* Suggested Quick Prompts */}
      <div className="suggested-prompts-bar">
        <span className="text-xs text-muted font-semibold">Suggested Questions:</span>
        <div className="prompts-list">
          {sampleQuestions.map((q, idx) => (
            <button
              key={idx}
              className="prompt-chip"
              onClick={() => handleSendMessage(q)}
              disabled={loading}
            >
              {q}
            </button>
          ))}
        </div>
      </div>

      {/* Chat Messages Box */}
      <div className="chat-window">
        {fetchingHistory ? (
          <div className="text-center py-8 text-muted">
            <div className="spinner mb-2"></div>
            Loading session conversation...
          </div>
        ) : messages.length === 0 ? (
          <div className="empty-chat-state text-center py-8">
            <div className="assistant-avatar-large">🤖</div>
            <h4 className="mt-2">SmartShop Assistant Ready</h4>
            <p className="text-muted text-sm max-w-md mx-auto">
              Ask about phones, laptops, and accessories in the SmartShop catalog.
              Try asking: <em>"Which phone has the biggest battery?"</em> followed by <em>"How much does it cost?"</em>.
            </p>
          </div>
        ) : (
          messages.map((msg, index) => {
            const isUser = msg.role === 'user';
            return (
              <div
                key={index}
                className={`chat-bubble-container ${isUser ? 'user-bubble-container' : 'assistant-bubble-container'}`}
              >
                <div className={`chat-bubble ${isUser ? 'user-bubble' : 'assistant-bubble'}`}>
                  <div className="chat-role-label">
                    {isUser ? '👤 You' : '🤖 SmartShop Assistant'}
                  </div>
                  <div className="chat-text">{msg.content}</div>
                </div>
              </div>
            );
          })
        )}

        {loading && (
          <div className="chat-bubble-container assistant-bubble-container">
            <div className="chat-bubble assistant-bubble typing-bubble">
              <span className="dot"></span>
              <span className="dot"></span>
              <span className="dot"></span>
            </div>
          </div>
        )}

        <div ref={chatBottomRef} />
      </div>

      {error && (
        <div className="alert alert-error my-2">
          <strong>Error:</strong> {error}
        </div>
      )}

      {/* Input Form */}
      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleSendMessage();
        }}
        className="chat-input-row"
      >
        <input
          type="text"
          className="form-control"
          placeholder="Ask about products, specs, pricing..."
          value={inputMessage}
          onChange={(e) => setInputMessage(e.target.value)}
          disabled={loading}
        />
        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || !inputMessage.trim()}
        >
          Send
        </button>
      </form>
    </div>
  );
}
