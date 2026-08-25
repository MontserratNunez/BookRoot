document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('bookDetailsContainer');
    if (!container) return;

    const currentBookWorkKey = container.dataset.bookKey;
    const currentBookTitle = container.dataset.bookTitle;

    // Helper para escapar HTML en strings inyectados dinámicamente
    function escapeHtml(str) {
        return (str || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function getAntiforgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    // --- MANEJO DE MODALES Y ACCIONES ---
    function abrirModalCompletado() {
        document.getElementById('completadoModal').style.display = 'block';
        const inicialRating = parseInt(document.getElementById('modalRating').value) || 0;
        setRating(inicialRating, true);
    }

    function cerrarModalCompletado() {
        document.getElementById('completadoModal').style.display = 'none';
    }

    function setRating(ratingValue, initial) {
        const ratingInput = document.getElementById('modalRating');
        const currentRating = parseInt(ratingInput.value) || 0;

        if (!initial && ratingValue === currentRating) {
            ratingValue = 0;
        }

        ratingInput.value = ratingValue;

        const stars = document.querySelectorAll('#completadoModal .star');
        stars.forEach(star => {
            const starValue = parseInt(star.getAttribute('data-value'));
            star.style.color = starValue <= ratingValue ? 'var(--br-amber)' : 'var(--br-border)';
        });
    }

    function abrirModalListas() {
        const modal = document.getElementById('listasModal');
        modal.style.display = 'block';
        const listContainer = document.getElementById('listasContainer');
        listContainer.innerHTML = `<div class="text-center text-muted py-3"><div class="spinner-border spinner-border-sm text-amber me-2" role="status"></div>Cargando listas...</div>`;

        fetch(`/List/GetMyListsJson?bookWorkKey=${encodeURIComponent(currentBookWorkKey)}`)
            .then(res => res.json())
            .then(data => {
                if (!data || data.length === 0) {
                    listContainer.innerHTML = `<p class="text-center text-muted py-3 small fst-italic"><i class="bi bi-info-circle me-1"></i>No tienes listas creadas.</p>`;
                    return;
                }

                let html = '<div class="list-group list-group-flush">';
                data.forEach(l => {
                    const checkIcon = l.hasBook
                        ? `<i class="bi bi-check2-circle text-success fs-5"></i>`
                        : `<i class="bi bi-circle text-muted fs-5"></i>`;

                    const actionClass = l.hasBook ? 'js-ask-remove-list' : 'js-add-to-list';

                    html += `
                        <button type="button" 
                                class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 px-3 ${actionClass}" 
                                data-id="${l.id}" 
                                data-name="${escapeHtml(l.listName)}"
                                style="background:transparent; border-left: none; border-right: none; cursor:pointer;">
                            <span class="fw-medium" style="color: var(--br-ink);">${escapeHtml(l.listName)}</span>
                            ${checkIcon}
                        </button>`;
                });
                html += '</div>';
                listContainer.innerHTML = html;
            })
            .catch(err => {
                console.error(err);
                listContainer.innerHTML = `<p class="text-center text-danger py-3 small"><i class="bi bi-exclamation-circle me-1"></i>Error al cargar las listas.</p>`;
            });
    }

    function agregarALista(listId) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/List/AddBook';
        form.innerHTML = `
            <input type="hidden" name="__RequestVerificationToken" value="${getAntiforgeryToken()}" />
            <input type="hidden" name="listId" value="${listId}" />
            <input type="hidden" name="bookWorkKey" value="${escapeHtml(currentBookWorkKey)}" />
            <input type="hidden" name="returnBookKey" value="${escapeHtml(currentBookWorkKey)}" />
        `;
        document.body.appendChild(form);
        form.submit();
    }

    function pedirConfirmacionQuitarLista(listId, listName) {
        document.getElementById('quitarListaId').value = listId;
        document.getElementById('quitarDeListaMensaje').innerHTML = `¿Quitar este libro de la lista <strong>${escapeHtml(listName)}</strong>?`;
        document.getElementById('quitarDeListaForm').action = '/List/RemoveBook';
        document.getElementById('quitarDeListaModal').style.display = 'block';
    }

    function abrirModalFavoritos() {
        document.getElementById('favoritosModal').style.display = 'block';
        const slotsContainer = document.getElementById('favoritosSlots');
        slotsContainer.innerHTML = `<div class="text-center text-muted py-3 w-100"><div class="spinner-border spinner-border-sm text-amber me-2" role="status"></div>Cargando...</div>`;

        fetch(`/User/GetTopFourJson`)
            .then(res => res.json())
            .then(data => {
                while (data.length < 4) data.push(null);

                let html = '';
                data.forEach((book, i) => {
                    if (book) {
                        const coverHtml = book.coverUrl
                            ? `<img src="${book.coverUrl}" style="width:100%;height:100%;object-fit:cover;" />`
                            : `<div class="d-flex align-items-center justify-content-center h-100 text-muted"><i class="bi bi-journal-text fs-3 text-amber"></i></div>`;

                        html += `
                            <div style="flex:1; display:flex; flex-direction:column; align-items:center; gap:6px; cursor:pointer;"
                                 class="js-ask-replace" data-slot="${i}" data-title="${escapeHtml(book.title)}"
                                 title="Reemplazar ${escapeHtml(book.title)}">
                                <div style="width:70px;height:105px;border-radius:6px;overflow:hidden;border:2px solid var(--br-amber);">
                                    ${coverHtml}
                                </div>
                                <span style="font-size:0.7rem;font-weight:600;text-align:center;max-width:70px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:var(--br-ink);">${escapeHtml(book.title)}</span>
                                <span style="font-size:0.65rem;color:#9c2a2a;"><i class="bi bi-arrow-repeat"></i> Reemplazar</span>
                            </div>`;
                    } else {
                        html += `
                            <div style="flex:1; display:flex; flex-direction:column; align-items:center; gap:6px; cursor:pointer;"
                                 class="js-add-to-slot" data-slot="${i}" title="Agregar en posición ${i + 1}">
                                <div style="width:70px;height:105px;border-radius:6px;overflow:hidden;border:2px dashed var(--br-border);display:flex;flex-direction:column;align-items:center;justify-content:center;background:#fdf8f3;">
                                    <i class="bi bi-plus-lg fs-3" style="color:var(--br-amber);"></i>
                                </div>
                                <span style="font-size:0.7rem;font-weight:600;text-align:center;color:var(--br-amber);">Posición #${i + 1}</span>
                                <span style="font-size:0.65rem;color:var(--br-amber);">Agregar aquí</span>
                            </div>`;
                    }
                });
                slotsContainer.innerHTML = html;
            })
            .catch(err => {
                console.error(err);
                slotsContainer.innerHTML = `<p class="text-danger small"><i class="bi bi-exclamation-circle me-1"></i>Error al cargar favoritos.</p>`;
            });
    }

    function agregarEnSlot(slotIndex) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/User/AddToFavorites';
        form.innerHTML = `
            <input type="hidden" name="__RequestVerificationToken" value="${getAntiforgeryToken()}" />
            <input type="hidden" name="bookWorkKey" value="${escapeHtml(currentBookWorkKey)}" />
            <input type="hidden" name="slotIndex" value="${slotIndex}" />
            <input type="hidden" name="returnBookKey" value="${escapeHtml(currentBookWorkKey)}" />
        `;
        document.body.appendChild(form);
        form.submit();
    }

    function pedirConfirmacionReemplazar(slotIndex, slotTitle) {
        document.getElementById('reemplazarFavSlot').value = slotIndex;
        document.getElementById('reemplazarFavMensaje').innerHTML =
            `¿Reemplazar <strong>${escapeHtml(slotTitle)}</strong> con <strong>${escapeHtml(currentBookTitle)}</strong> en esta posición?`;
        document.getElementById('reemplazarFavModal').style.display = 'block';
    }

    // --- DELEGACIÓN DE EVENTOS GLOBAL (CSP COMPLIANT) ---
    document.addEventListener('click', (e) => {
        // Abrir Modales
        if (e.target.closest('.js-open-modal-completado')) abrirModalCompletado();
        if (e.target.closest('.js-open-modal-listas')) abrirModalListas();
        if (e.target.closest('.js-open-modal-favoritos')) abrirModalFavoritos();
        if (e.target.closest('.js-open-modal-quitar-favorito')) document.getElementById('quitarFavModal').style.display = 'block';

        // Cerrar Modales
        if (e.target.closest('.js-close-modal-completado')) cerrarModalCompletado();
        if (e.target.closest('.js-close-modal-listas')) document.getElementById('listasModal').style.display = 'none';
        if (e.target.closest('.js-close-modal-quitar-lista')) document.getElementById('quitarDeListaModal').style.display = 'none';
        if (e.target.closest('.js-close-modal-favoritos')) document.getElementById('favoritosModal').style.display = 'none';
        if (e.target.closest('.js-close-modal-reemplazar')) document.getElementById('reemplazarFavModal').style.display = 'none';
        if (e.target.closest('.js-close-modal-quitar-favorito')) document.getElementById('quitarFavModal').style.display = 'none';

        // Selección de Estrellas
        const star = e.target.closest('.js-star');
        if (star) {
            const val = parseInt(star.dataset.value);
            setRating(val, false);
        }

        // Acciones dinámicas de Listas
        const addListBtn = e.target.closest('.js-add-to-list');
        if (addListBtn) agregarALista(addListBtn.dataset.id);

        const removeListBtn = e.target.closest('.js-ask-remove-list');
        if (removeListBtn) pedirConfirmacionQuitarLista(removeListBtn.dataset.id, removeListBtn.dataset.name);

        // Acciones dinámicas de Favoritos
        const addSlotBtn = e.target.closest('.js-add-to-slot');
        if (addSlotBtn) agregarEnSlot(addSlotBtn.dataset.slot);

        const replaceSlotBtn = e.target.closest('.js-ask-replace');
        if (replaceSlotBtn) pedirConfirmacionReemplazar(replaceSlotBtn.dataset.slot, replaceSlotBtn.dataset.title);
    });

    // Cierre al hacer click en el backdrop oscuro
    ['listasModal', 'favoritosModal', 'quitarFavModal', 'quitarDeListaModal', 'reemplazarFavModal', 'completadoModal'].forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.addEventListener('click', function (e) {
                if (e.target === this) this.style.display = 'none';
            });
        }
    });
});