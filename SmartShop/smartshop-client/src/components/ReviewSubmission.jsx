import React, { useState, useEffect } from 'react';
import { apiService } from '../services/api';

export default function ReviewSubmission({ onReviewSubmitted }) {
  const [products, setProducts] = useState([]);
  const [formData, setFormData] = useState({
    productId: 1,
    customerName: 'John',
    content: 'The battery is great but the phone gets hot when gaming.',
    rating: 4
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [analysisResult, setAnalysisResult] = useState(null);

  useEffect(() => {
    apiService.getProducts()
      .then(data => {
        setProducts(data);
        if (data.length > 0 && !formData.productId) {
          setFormData(prev => ({ ...prev, productId: data[0].productId }));
        }
      })
      .catch(() => {});
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'productId' || name === 'rating' ? Number(value) : value
    }));
  };

  const handlePreFillSample = () => {
    setFormData({
      productId: products[0]?.productId || 1,
      customerName: 'John',
      content: 'The battery is great but the phone gets hot when gaming.',
      rating: 4
    });
    setAnalysisResult(null);
    setError(null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.customerName.trim() || !formData.content.trim()) {
      setError('Please provide customer name and review content.');
      return;
    }

    setSubmitting(true);
    setError(null);
    setAnalysisResult(null);

    try {
      const response = await apiService.analyzeReview(formData);
      setAnalysisResult(response);

      // Trigger callback to refresh report table
      if (onReviewSubmitted) {
        onReviewSubmitted();
      }
    } catch (err) {
      setError(err.message || 'Failed to submit and analyze review.');
    } finally {
      setSubmitting(false);
    }
  };

  const getSentimentBadgeClass = (sentiment) => {
    switch ((sentiment || '').toLowerCase()) {
      case 'positive': return 'badge-positive';
      case 'negative': return 'badge-negative';
      default: return 'badge-neutral';
    }
  };

  return (
    <div className="card">
      <div className="card-header flex justify-between items-center">
        <div>
          <h2 className="card-title">Submit & Analyze Customer Review</h2>
          <p className="card-subtitle">
            Endpoint: <code className="endpoint-tag">POST /api/reviews/analyze</code>
            <br />
            Gemini parses structured JSON schema (sentiment, confidence score 0.0 - 1.0, summary).
          </p>
        </div>
        <button
          type="button"
          onClick={handlePreFillSample}
          className="btn btn-secondary text-xs"
        >
          Load Exam Sample
        </button>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid-2 gap-4">
          <div className="form-group">
            <label className="form-label" htmlFor="productId">Product:</label>
            <select
              id="productId"
              name="productId"
              className="form-control"
              value={formData.productId}
              onChange={handleChange}
            >
              {products.map(p => (
                <option key={p.productId} value={p.productId}>
                  #{p.productId} - {p.productName}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="customerName">Customer Name:</label>
            <input
              id="customerName"
              type="text"
              name="customerName"
              className="form-control"
              value={formData.customerName}
              onChange={handleChange}
              placeholder="e.g. John"
              required
            />
          </div>
        </div>

        <div className="form-group mt-3">
          <label className="form-label" htmlFor="rating">Rating (1 to 5 Stars):</label>
          <div className="rating-selector">
            {[1, 2, 3, 4, 5].map(star => (
              <label key={star} className="rating-option">
                <input
                  type="radio"
                  name="rating"
                  value={star}
                  checked={formData.rating === star}
                  onChange={handleChange}
                />
                <span className={`star-label ${formData.rating >= star ? 'active' : ''}`}>
                  ★ {star}
                </span>
              </label>
            ))}
          </div>
        </div>

        <div className="form-group mt-3">
          <label className="form-label" htmlFor="content">Review Content:</label>
          <textarea
            id="content"
            name="content"
            className="form-control"
            rows="3"
            value={formData.content}
            onChange={handleChange}
            placeholder="Write customer review..."
            required
          ></textarea>
        </div>

        {error && (
          <div className="alert alert-error mt-3">
            <strong>Error:</strong> {error}
          </div>
        )}

        <div className="mt-4">
          <button
            type="submit"
            disabled={submitting}
            className="btn btn-primary"
          >
            {submitting ? (
              <>
                <span className="btn-spinner"></span> Analyzing with Gemini JSON Mode...
              </>
            ) : (
              <span className="flex items-center gap-2">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 2L14.4 8.6L21 11L14.4 13.4L12 20L9.6 13.4L3 11L9.6 8.6L12 2Z"/>
                  <path d="M19 15L19.8 17.2L22 18L19.8 18.8L19 21L18.2 18.8L16 18L18.2 17.2L19 15Z"/>
                </svg>
                Submit & Analyze Review
              </span>
            )}
          </button>
        </div>
      </form>

      {/* Analysis Result Display */}
      {analysisResult && (
        <div className="analysis-result-box mt-4">
          <h3 className="result-title">Review Saved & Structured AI Analysis:</h3>

          <div className="grid-3 gap-3 mt-3">
            <div className="result-item">
              <span className="result-label">Sentiment:</span>
              <span className={`badge ${getSentimentBadgeClass(analysisResult.sentiment)} mt-1`}>
                {analysisResult.sentiment}
              </span>
            </div>

            <div className="result-item">
              <span className="result-label">Confidence Score:</span>
              <div className="score-container mt-1">
                <div className="score-value font-bold">{analysisResult.score?.toFixed(2)}</div>
                <div className="score-bar-bg">
                  <div
                    className="score-bar-fill"
                    style={{ width: `${Math.round((analysisResult.score || 0) * 100)}%` }}
                  ></div>
                </div>
              </div>
            </div>

            <div className="result-item">
              <span className="result-label">Review ID (Saved to DB):</span>
              <div className="font-semibold text-lg">#{analysisResult.reviewId}</div>
            </div>
          </div>

          <div className="mt-3">
            <span className="result-label">AI Summary:</span>
            <blockquote className="summary-quote mt-1">
              "{analysisResult.summary}"
            </blockquote>
          </div>
        </div>
      )}
    </div>
  );
}
