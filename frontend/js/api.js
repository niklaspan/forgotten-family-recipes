// In Azure Static Web Apps the Functions are proxied under /api automatically.
// Locally the Functions host runs on a different port, so detect and switch.
const API_BASE = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
  ? 'http://localhost:7071/api'
  : '/api';

async function apiRequest(method, path, body = null) {
  const options = {
    method,
    headers: { 'Content-Type': 'application/json' },
  };
  if (body !== null) options.body = JSON.stringify(body);

  const res = await fetch(`${API_BASE}${path}`, options);

  if (res.status === 204) return null;

  if (!res.ok) {
    const text = await res.text().catch(() => '');
    throw new Error(text || `Request failed with status ${res.status}`);
  }

  return res.json();
}

const api = {
  getAllRecipes:        ()          => apiRequest('GET',    '/recipes'),
  getRecipe:           (id)         => apiRequest('GET',    `/recipes/${id}`),
  getRecipesByChapter: (chapter)    => apiRequest('GET',    `/recipes/chapter/${encodeURIComponent(chapter)}`),
  createRecipe:        (recipe)     => apiRequest('POST',   '/recipes', recipe),
  updateRecipe:        (id, recipe) => apiRequest('PUT',    `/recipes/${id}`, recipe),
  deleteRecipe:        (id)         => apiRequest('DELETE', `/recipes/${id}`),
};

function escapeHtml(str) {
  return String(str ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}
