import React from 'react';

export default function ReviewReportTable({ reports, loading, error, onRefresh }) {
  if (loading) {
    return (
      <div className="card text-center py-8">
        <div className="spinner"></div>
        <p className="text-muted mt-4">Loading customer review report from SQL Server...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="card alert-error">
        <div className="alert-title">Failed to load review report</div>
        <p>{error}</p>
        <button onClick={onRefresh} className="btn btn-secondary mt-3">
          Try Again
        </button>
      </div>
    );
  }

  if (!reports || reports.length === 0) {
    return (
      <div className="card text-center py-8">
        <h3>No Products Found</h3>
        <p className="text-muted">The product catalog is currently empty in the database.</p>
        <button onClick={onRefresh} className="btn btn-primary mt-3">
          Refresh Data
        </button>
      </div>
    );
  }

  // Calculate totals
  const totalReviewsCount = reports.reduce((acc, cur) => acc + (cur.totalReviews || 0), 0);
  const totalPositive = reports.reduce((acc, cur) => acc + (cur.positive || 0), 0);
  const totalNegative = reports.reduce((acc, cur) => acc + (cur.negative || 0), 0);
  const totalNeutral = reports.reduce((acc, cur) => acc + (cur.neutral || 0), 0);

  return (
    <div className="card">
      <div className="card-header flex justify-between items-center">
        <div>
          <h2 className="card-title">Product Review Analysis Report</h2>
          <p className="card-subtitle">
            Endpoint: <code className="endpoint-tag">GET /api/reports/reviews</code>
          </p>
        </div>
        <button onClick={onRefresh} className="btn btn-secondary" title="Refresh report from API">
          ↻ Refresh Report
        </button>
      </div>

      {/* Summary KPI Badges */}
      <div className="stats-grid mb-4">
        <div className="stat-card">
          <div className="stat-label">Total Products</div>
          <div className="stat-value">{reports.length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Total Reviews</div>
          <div className="stat-value">{totalReviewsCount}</div>
        </div>
        <div className="stat-card text-success">
          <div className="stat-label">Positive Reviews</div>
          <div className="stat-value">{totalPositive}</div>
        </div>
        <div className="stat-card text-danger">
          <div className="stat-label">Negative Reviews</div>
          <div className="stat-value">{totalNegative}</div>
        </div>
        <div className="stat-card text-muted">
          <div className="stat-label">Neutral Reviews</div>
          <div className="stat-value">{totalNeutral}</div>
        </div>
      </div>

      {/* Required Table as per Examination Specifications */}
      <div className="table-responsive">
        <table className="report-table">
          <thead>
            <tr>
              <th style={{ width: '60px' }}>No</th>
              <th>Product name</th>
              <th className="text-center">Total reviews</th>
              <th className="text-center">Positive</th>
              <th className="text-center">Negative</th>
              <th className="text-center">Neutral</th>
              <th className="text-center">Average rating</th>
            </tr>
          </thead>
          <tbody>
            {reports.map((row) => (
              <tr key={row.no || row.productName}>
                <td className="font-semibold text-center">{row.no}</td>
                <td>
                  <span className="product-name">{row.productName}</span>
                </td>
                <td className="text-center font-semibold">{row.totalReviews}</td>
                <td className="text-center">
                  <span className={`badge ${row.positive > 0 ? 'badge-positive' : 'badge-neutral'}`}>
                    {row.positive}
                  </span>
                </td>
                <td className="text-center">
                  <span className={`badge ${row.negative > 0 ? 'badge-negative' : 'badge-neutral'}`}>
                    {row.negative}
                  </span>
                </td>
                <td className="text-center">
                  <span className="badge badge-neutral">{row.neutral}</span>
                </td>
                <td className="text-center">
                  <div className="rating-pill">
                    <span className="rating-star">★</span>
                    <span className="font-semibold">
                      {typeof row.averageRating === 'number' ? row.averageRating.toFixed(1) : '0.0'}
                    </span>
                    <span className="text-muted text-xs">/ 5.0</span>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
