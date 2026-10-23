import React, { useState, useEffect } from 'react';
import { apiService } from '../services/api';

export default function ProductDescriptionGenerator() {
  const [products, setProducts] = useState([]);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [loading, setLoading] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  const fetchProducts = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiService.getProducts();
      setProducts(data);
      if (data.length > 0 && !selectedProductId) {
        setSelectedProductId(data[0].productId);
        setSelectedProduct(data[0]);
      } else if (selectedProductId) {
        const found = data.find(p => p.productId === Number(selectedProductId));
        if (found) setSelectedProduct(found);
      }
    } catch (err) {
      setError(err.message || 'Failed to load products');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleSelectChange = (e) => {
    const id = Number(e.target.value);
    setSelectedProductId(id);
    const found = products.find(p => p.productId === id);
    setSelectedProduct(found || null);
    setSuccessMsg(null);
    setError(null);
  };

  const handleGenerate = async () => {
    if (!selectedProductId) return;
    setGenerating(true);
    setError(null);
    setSuccessMsg(null);
    try {
      const result = await apiService.generateProductDescription(selectedProductId);
      setSuccessMsg(`Description successfully generated and saved to SQL Server!`);
      // Update local product description
      setSelectedProduct(prev => ({ ...prev, description: result.description }));
      setProducts(prev => prev.map(p => p.productId === Number(selectedProductId) ? { ...p, description: result.description } : p));
    } catch (err) {
      setError(err.message || 'Error generating description with Gemini');
    } finally {
      setGenerating(false);
    }
  };

  const getWordCount = (text) => {
    if (!text) return 0;
    return text.trim().split(/\s+/).filter(Boolean).length;
  };

  return (
    <div className="card">
      <div className="card-header">
        <h2 className="card-title">Product Description Generator</h2>
        <p className="card-subtitle">
          Endpoint: <code className="endpoint-tag">POST /api/products/&#123;id&#125;/generatedescription</code>
          <br />
          Uses Gemini AI as a professional copywriter (max 80 words, strict fact preservation).
        </p>
      </div>

      {loading ? (
        <div className="text-center py-4">
          <div className="spinner"></div>
          <p className="text-muted mt-2">Loading products...</p>
        </div>
      ) : (
        <div>
          <div className="form-group mb-4">
            <label className="form-label" htmlFor="product-select">Select Product from SQL Server Catalog:</label>
            <select
              id="product-select"
              className="form-control"
              value={selectedProductId}
              onChange={handleSelectChange}
            >
              {products.map(p => (
                <option key={p.productId} value={p.productId}>
                  #{p.productId} - {p.productName} ({p.category}) - {Number(p.price).toLocaleString()} VND
                </option>
              ))}
            </select>
          </div>

          {selectedProduct && (
            <div className="product-details-box mb-4">
              <div className="grid-2">
                <div>
                  <span className="text-muted text-sm">Category:</span>
                  <div className="font-semibold">{selectedProduct.category}</div>
                </div>
                <div>
                  <span className="text-muted text-sm">Price:</span>
                  <div className="font-semibold">{Number(selectedProduct.price).toLocaleString()} VND</div>
                </div>
              </div>
              <div className="mt-3">
                <span className="text-muted text-sm">Features (Input Context):</span>
                <div className="features-tag-list mt-1">
                  {selectedProduct.features?.split(',').map((f, i) => (
                    <span key={i} className="feature-tag">{f.trim()}</span>
                  ))}
                </div>
              </div>

              <div className="mt-4">
                <div className="flex justify-between items-center mb-1">
                  <span className="text-muted text-sm font-semibold">Current Saved Description:</span>
                  {selectedProduct.description && (
                    <span className="text-xs text-muted">
                      Word count: <strong>{getWordCount(selectedProduct.description)} / 80</strong>
                    </span>
                  )}
                </div>
                <div className="description-display">
                  {selectedProduct.description ? (
                    <p className="text-content">{selectedProduct.description}</p>
                  ) : (
                    <p className="text-muted italic">No description generated yet. Click the button below to generate one with Gemini.</p>
                  )}
                </div>
              </div>
            </div>
          )}

          {error && (
            <div className="alert alert-error mb-4">
              <strong>Error:</strong> {error}
            </div>
          )}

          {successMsg && (
            <div className="alert alert-success mb-4">
              <strong>Success:</strong> {successMsg}
            </div>
          )}

          <button
            onClick={handleGenerate}
            disabled={generating || !selectedProductId}
            className="btn btn-primary"
          >
            {generating ? (
              <>
                <span className="btn-spinner"></span> Generating with Gemini...
              </>
            ) : (
              '✨ Generate Description with Gemini'
            )}
          </button>
        </div>
      )}
    </div>
  );
}
