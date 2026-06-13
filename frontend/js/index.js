async function loadRecipes() {
  const container = document.getElementById('content');

  try {
    const recipes = await api.getAllRecipes();

    if (!recipes || recipes.length === 0) {
      container.innerHTML = `
        <div class="empty">
          <p>No recipes yet. Add the first one!</p>
          <a href="add.html" class="btn btn-primary">+ Add Recipe</a>
        </div>`;
      return;
    }

    // Group by chapter, preserving insertion order within each group.
    const chapters = {};
    for (const recipe of recipes) {
      const ch = recipe.chapter?.trim() || 'Uncategorised';
      if (!chapters[ch]) chapters[ch] = [];
      chapters[ch].push(recipe);
    }

    const html = Object.keys(chapters).sort().map(chapter => `
      <section class="chapter-section">
        <h2 class="chapter-title">${escapeHtml(chapter)}</h2>
        ${chapters[chapter].map(r => `
          <a href="recipe.html?id=${encodeURIComponent(r.id)}" class="recipe-card">
            <div>
              <div class="recipe-card-title">${escapeHtml(r.title)}</div>
              <div class="recipe-card-meta">by ${escapeHtml(r.author || 'Unknown')}</div>
            </div>
            <span class="recipe-card-arrow">›</span>
          </a>
        `).join('')}
      </section>
    `).join('');

    container.innerHTML = html;
  } catch (err) {
    container.innerHTML = `<div class="error-msg">Could not load recipes: ${escapeHtml(err.message)}</div>`;
  }
}

loadRecipes();
