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
          <span className="brand-logo">⚡</span>
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
