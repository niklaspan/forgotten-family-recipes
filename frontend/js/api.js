// SWA CLI (port 4280) proxies /api to the local Functions host automatically, so use a
// relative path there — same as production. Direct access to the Functions host (port 7071)
// needs the full URL because no proxy is involved.
const API_BASE = window.location.port === '7071'
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
