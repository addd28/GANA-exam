import React, { useState, useEffect } from 'react';
import { apiService } from '../services/api';
import ReviewReportTable from '../components/ReviewReportTable';
import ProductDescriptionGenerator from '../components/ProductDescriptionGenerator';
import ReviewSubmission from '../components/ReviewSubmission';
import ShoppingAssistant from '../components/ShoppingAssistant';

export default function DashboardPage() {
  const [activeTab, setActiveTab] = useState('report');
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchReports = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiService.getReviewReport();
      setReports(data);
    } catch (err) {
      setError(err.message || 'Failed to fetch review report.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, []);

  return (
    <div className="dashboard-container">
      {/* Header */}
      <header className="dashboard-header">
        <div className="header-brand">
          <span className="brand-logo" aria-label="SmartShop AI Logo">
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2L14.4 8.6L21 11L14.4 13.4L12 20L9.6 13.4L3 11L9.6 8.6L12 2Z" fill="url(#brand-ai-grad)" stroke="#2563eb" strokeWidth="1.2" strokeLinejoin="round"/>
              <path d="M19 15L19.8 17.2L22 18L19.8 18.8L19 21L18.2 18.8L16 18L18.2 17.2L19 15Z" fill="#7c3aed"/>
              <defs>
                <linearGradient id="brand-ai-grad" x1="3" y1="2" x2="21" y2="20" gradientUnits="userSpaceOnUse">
                  <stop stopColor="#2563eb"/>
                  <stop offset="1" stopColor="#9333ea"/>
                </linearGradient>
              </defs>
            </svg>
          </span>
          <div>
            <h1 className="brand-title">SmartShop AI Dashboard</h1>
            <p className="brand-subtitle">Generative AI for .NET Developers with Google AI – Examination Set01</p>
          </div>
        </div>

        <div className="header-meta">
          <span className="tech-badge">ASP.NET Core (.NET 10)</span>
          <span className="tech-badge">SQL Server (SMARTSHOP)</span>
          <span className="tech-badge">Gemini 2.5 Flash</span>
        </div>
      </header>

      {/* Navigation Tabs */}
      <nav className="tab-nav">
        <button
          className={`tab-btn ${activeTab === 'report' ? 'active' : ''}`}
          onClick={() => setActiveTab('report')}
        >
          📊 Review Report (Q4 & Q5)
        </button>
        <button
          className={`tab-btn ${activeTab === 'description' ? 'active' : ''}`}
          onClick={() => setActiveTab('description')}
        >
          ✍️ Product Description (Q1)
        </button>
        <button
          className={`tab-btn ${activeTab === 'review' ? 'active' : ''}`}
          onClick={() => setActiveTab('review')}
        >
          📝 Review Analysis (Q2)
        </button>
        <button
          className={`tab-btn ${activeTab === 'assistant' ? 'active' : ''}`}
          onClick={() => setActiveTab('assistant')}
        >
          🤖 Shopping Assistant (Q3)
        </button>
      </nav>

      {/* Main Tab Content */}
      <main className="tab-content">
        {activeTab === 'report' && (
          <ReviewReportTable
            reports={reports}
            loading={loading}
            error={error}
            onRefresh={fetchReports}
          />
        )}

        {activeTab === 'description' && (
          <ProductDescriptionGenerator />
        )}

        {activeTab === 'review' && (
          <ReviewSubmission onReviewSubmitted={fetchReports} />
        )}

        {activeTab === 'assistant' && (
          <ShoppingAssistant />
        )}
      </main>

      {/* Footer */}
      <footer className="dashboard-footer">
        <div>SmartShop AI Examination Set01 • Safe Implementation Mode</div>
      </footer>
    </div>
  );
}
