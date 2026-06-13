function addListItem(listEl, placeholder) {
  const row = document.createElement('div');
  row.className = 'list-item-row';
  row.innerHTML = `
    <input type="text" placeholder="${escapeHtml(placeholder)}" />
    <button type="button" class="btn-remove" title="Remove">×</button>
  `;

  row.querySelector('.btn-remove').addEventListener('click', () => {
    // Always keep at least one row so the list doesn't collapse entirely.
    if (listEl.querySelectorAll('.list-item-row').length > 1) {
      row.remove();
    } else {
      row.querySelector('input').value = '';
    }
  });

  listEl.appendChild(row);
  row.querySelector('input').focus();
}

function getListValues(listEl) {
  return Array.from(listEl.querySelectorAll('input'))
    .map(input => input.value.trim())
    .filter(Boolean);
}

// ── Init ──
const ingredientsList   = document.getElementById('ingredients-list');
const instructionsList  = document.getElementById('instructions-list');

addListItem(ingredientsList,  'e.g. 2 cups flour');
addListItem(instructionsList, 'e.g. Preheat oven to 180 °C');

document.getElementById('add-ingredient').addEventListener('click', () => {
  addListItem(ingredientsList, 'e.g. 1 tsp vanilla extract');
});

document.getElementById('add-instruction').addEventListener('click', () => {
  addListItem(instructionsList, 'e.g. Mix until smooth');
});

// ── Submit ──
document.getElementById('recipe-form').addEventListener('submit', async (e) => {
  e.preventDefault();

  const errorEl  = document.getElementById('form-error');
  const submitBtn = document.getElementById('submit-btn');

  errorEl.hidden = true;

  const title        = document.getElementById('title').value.trim();
  const chapter      = document.getElementById('chapter').value.trim();
  const author       = document.getElementById('author').value.trim();
  const ingredients  = getListValues(ingredientsList);
  const instructions = getListValues(instructionsList);

  if (!title) {
    errorEl.textContent = 'A title is required.';
    errorEl.hidden = false;
    document.getElementById('title').focus();
    return;
  }

  submitBtn.disabled = true;
  submitBtn.textContent = 'Saving…';

  try {
    const created = await api.createRecipe({ title, chapter, author, ingredients, instructions });
    window.location.href = `recipe.html?id=${encodeURIComponent(created.id)}`;
  } catch (err) {
    errorEl.textContent = `Failed to save recipe: ${err.message}`;
    errorEl.hidden = false;
    submitBtn.disabled = false;
    submitBtn.textContent = 'Save Recipe';
  }
});
