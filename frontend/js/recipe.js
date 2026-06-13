function formatDate(iso) {
  if (!iso) return '';
  return new Date(iso).toLocaleDateString('en-GB', {
    year: 'numeric', month: 'long', day: 'numeric',
  });
}

async function loadRecipe() {
  const container = document.getElementById('content');
  const id = new URLSearchParams(window.location.search).get('id');

  if (!id) {
    container.innerHTML = `
      <a href="index.html" class="back-link">‹ All Recipes</a>
      <div class="error-msg">No recipe ID found in the URL.</div>`;
    return;
  }

  try {
    const r = await api.getRecipe(id);

    document.title = `${r.title} – HandedDown`;

    const ingredientItems = (r.ingredients || [])
      .map(i => `<li>${escapeHtml(i)}</li>`)
      .join('');

    const instructionItems = (r.instructions || [])
      .map(s => `<li>${escapeHtml(s)}</li>`)
      .join('');

    container.innerHTML = `
      <a href="index.html" class="back-link">‹ All Recipes</a>

      <h1 class="recipe-title">${escapeHtml(r.title)}</h1>
      <div class="recipe-badges">
        ${r.chapter ? `<span class="badge badge-chapter">${escapeHtml(r.chapter)}</span>` : ''}
        ${r.author  ? `<span class="badge badge-author">by ${escapeHtml(r.author)}</span>` : ''}
      </div>
      <div class="recipe-date">${formatDate(r.createdDate)}</div>

      ${ingredientItems ? `
        <div class="recipe-section">
          <h2>Ingredients</h2>
          <ul class="ingredients-list">${ingredientItems}</ul>
        </div>
      ` : ''}

      ${instructionItems ? `
        <div class="recipe-section">
          <h2>Instructions</h2>
          <ol class="instructions-list">${instructionItems}</ol>
        </div>
      ` : ''}

      <div class="recipe-actions">
        <button class="btn btn-danger" id="delete-btn">Delete Recipe</button>
      </div>
    `;

    document.getElementById('delete-btn').addEventListener('click', () => deleteRecipe(id));
  } catch (err) {
    container.innerHTML = `
      <a href="index.html" class="back-link">‹ All Recipes</a>
      <div class="error-msg">Could not load recipe: ${escapeHtml(err.message)}</div>
    `;
  }
}

async function deleteRecipe(id) {
  if (!confirm('Delete this recipe? This cannot be undone.')) return;

  const btn = document.getElementById('delete-btn');
  btn.disabled = true;
  btn.textContent = 'Deleting…';

  try {
    await api.deleteRecipe(id);
    window.location.href = 'index.html';
  } catch (err) {
    alert(`Delete failed: ${err.message}`);
    btn.disabled = false;
    btn.textContent = 'Delete Recipe';
  }
}

loadRecipe();
