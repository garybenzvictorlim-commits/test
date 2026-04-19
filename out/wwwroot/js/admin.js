// ════════════════════════════════════════════════════════════
//  admin.js — Logic for the Admin Panel
//
//  What this file does:
//  1. Loads and displays all queue counters in a table
//  2. Allows admin to: Call Next, Edit, Reset, Delete queues
//  3. Allows admin to add a new queue counter
// ════════════════════════════════════════════════════════════

const API_BASE = '/api/queue';

// ── Clock ────────────────────────────────────────────────────
function updateClock() {
  const now = new Date();
  const timeEl = document.getElementById('sidebar-time');
  const dateEl = document.getElementById('sidebar-date');
  if (timeEl) timeEl.textContent = now.toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit' });
  if (dateEl) dateEl.textContent = now.toLocaleDateString('en-PH', { weekday: 'short', month: 'short', day: 'numeric' });
}
setInterval(updateClock, 1000);
updateClock();

// ── Section Navigation ───────────────────────────────────────
function showSection(name) {
  document.querySelectorAll('section').forEach(s => s.classList.add('hidden'));
  document.querySelectorAll('.nav-item').forEach(a => a.classList.remove('active'));
  document.getElementById(`section-${name}`).classList.remove('hidden');
  document.getElementById('page-title').textContent =
    name === 'dashboard' ? 'Dashboard' : 'Add Queue Counter';
  event.currentTarget.classList.add('active');
}

// ── Load and render all queues ───────────────────────────────
async function loadQueues() {
  try {
    const res = await fetch(API_BASE);
    const queues = await res.json();
    renderSummary(queues);
    renderTable(queues);
  } catch {
    showToast('❌ Cannot reach server. Is it running?', 'error');
  }
}

// ── Summary Stats ────────────────────────────────────────────
function renderSummary(queues) {
  const totalWaiting = queues.reduce((acc, q) => acc + Math.max(0, q.nextNumber - q.currentNumber - 1), 0);
  document.getElementById('sum-total').textContent = queues.length;
  document.getElementById('sum-open').textContent  = queues.filter(q => q.status === 'Open').length;
  document.getElementById('sum-waiting').textContent = totalWaiting;
}

// ── Table Render ─────────────────────────────────────────────
function renderTable(queues) {
  const tbody = document.getElementById('queue-tbody');
  if (queues.length === 0) {
    tbody.innerHTML = '<tr><td colspan="7" class="loading-row">No queues yet. Add one from the sidebar.</td></tr>';
    return;
  }

  tbody.innerHTML = queues.map((q, i) => {
    const waiting = Math.max(0, q.nextNumber - q.currentNumber - 1);
    const isOpen  = q.status === 'Open';
    const noMore  = q.currentNumber >= q.nextNumber - 1;

    return `
      <tr>
        <td style="color:var(--muted)">${i + 1}</td>
        <td style="font-weight:600;color:var(--text-inv)">${escapeHtml(q.name)}</td>
        <td><span class="current-num">${q.currentNumber === 0 ? '—' : q.currentNumber}</span></td>
        <td>${q.nextNumber}</td>
        <td>${waiting > 0 ? `<strong style="color:var(--yellow)">${waiting}</strong>` : waiting}</td>
        <td>
          <span class="status-badge ${isOpen ? 'status-open' : 'status-closed'}">
            ${isOpen ? '🟢 Open' : '🔴 Closed'}
          </span>
        </td>
        <td>
          <div class="action-group">
            <button class="btn btn-next"   onclick="callNext(${q.id})"    ${noMore ? 'disabled title="Queue empty"' : ''}>▶ Next</button>
            <button class="btn btn-edit"   onclick="openEdit(${q.id}, '${escapeHtml(q.name)}', ${q.currentNumber}, ${q.nextNumber}, '${q.status}')">✏️ Edit</button>
            <button class="btn btn-reset"  onclick="resetQueue(${q.id}, '${escapeHtml(q.name)}')">↺ Reset</button>
            <button class="btn btn-delete" onclick="deleteQueue(${q.id}, '${escapeHtml(q.name)}')">🗑 Delete</button>
          </div>
        </td>
      </tr>
    `;
  }).join('');
}

// ── Call Next Number ─────────────────────────────────────────
async function callNext(id) {
  const res  = await fetch(`${API_BASE}/${id}/next`, { method: 'POST' });
  const data = await res.json();
  if (res.ok) {
    showToast(`✅ Now serving #${data.currentNumber} at "${data.name}"`, 'success');
    loadQueues();
  } else {
    showToast('⚠️ ' + (data.message || 'Error'), 'error');
  }
}

// ── Reset Queue ──────────────────────────────────────────────
async function resetQueue(id, name) {
  if (!confirm(`Reset "${name}"?\n\nThis will set current number to 0 and restart from 1.`)) return;
  const res  = await fetch(`${API_BASE}/${id}/reset`, { method: 'POST' });
  const data = await res.json();
  showToast(res.ok ? `✅ ${data.message}` : '❌ Error resetting.', res.ok ? 'success' : 'error');
  loadQueues();
}

// ── Delete Queue ─────────────────────────────────────────────
async function deleteQueue(id, name) {
  if (!confirm(`Delete "${name}" permanently? This cannot be undone.`)) return;
  const res = await fetch(`${API_BASE}/${id}`, { method: 'DELETE' });
  showToast(res.ok ? `🗑️ "${name}" deleted.` : '❌ Error deleting.', res.ok ? 'success' : 'error');
  loadQueues();
}

// ── Open Edit Modal ──────────────────────────────────────────
function openEdit(id, name, currentNum, nextNum, status) {
  document.getElementById('edit-id').value      = id;
  document.getElementById('edit-name').value    = name;
  document.getElementById('edit-current').value = currentNum;
  document.getElementById('edit-next').value    = nextNum;
  document.getElementById('edit-status').value  = status;
  document.getElementById('edit-modal').classList.add('active');
}

function closeEditModal() {
  document.getElementById('edit-modal').classList.remove('active');
}

// ── Save Edit ────────────────────────────────────────────────
async function saveEdit() {
  const id = document.getElementById('edit-id').value;
  const payload = {
    name:          document.getElementById('edit-name').value.trim(),
    currentNumber: parseInt(document.getElementById('edit-current').value),
    nextNumber:    parseInt(document.getElementById('edit-next').value),
    status:        document.getElementById('edit-status').value,
  };

  if (!payload.name) { showToast('⚠️ Name cannot be empty.', 'error'); return; }
  if (payload.nextNumber <= payload.currentNumber && payload.currentNumber > 0) {
    showToast('⚠️ Next Number must be greater than Current Number.', 'error'); return;
  }

  const res = await fetch(`${API_BASE}/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  if (res.ok) {
    showToast('✅ Queue updated successfully!', 'success');
    closeEditModal();
    loadQueues();
  } else {
    showToast('❌ Failed to update queue.', 'error');
  }
}

// ── Add New Queue ─────────────────────────────────────────────
async function addQueue() {
  const name   = document.getElementById('new-name').value.trim();
  const status = document.getElementById('new-status').value;

  if (!name) { showToast('⚠️ Please enter a counter name.', 'error'); return; }

  const res = await fetch(API_BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, status }),
  });

  if (res.ok) {
    showToast(`✅ "${name}" added!`, 'success');
    document.getElementById('new-name').value = '';
    showSection('dashboard');
    loadQueues();
  } else {
    showToast('❌ Failed to add queue.', 'error');
  }
}

// ── Toast Notification ───────────────────────────────────────
let toastTimer;
function showToast(msg, type = 'success') {
  const toast = document.getElementById('toast');
  toast.textContent = msg;
  toast.className = `toast show ${type}`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { toast.className = 'toast'; }, 3500);
}

// ── Utility: XSS prevention ──────────────────────────────────
function escapeHtml(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

// ── Init ─────────────────────────────────────────────────────

// On page load: verify session is valid before showing the page
async function checkSession() {
  try {
    const res  = await fetch('/api/auth/me');
    const data = await res.json();

    if (!res.ok) {
      // Not logged in — redirect to login page
      window.location.href = '/admin/login.html';
      return;
    }

    // Show the logged-in admin's name in the UI
    const fullName = data.fullName || data.username;
    const username = data.username;

    const topbarUser = document.getElementById('topbar-user');
    const sidebarFull = document.getElementById('sidebar-fullname');
    const sidebarUser = document.getElementById('sidebar-username');

    if (topbarUser)   topbarUser.textContent   = `👤 ${fullName}`;
    if (sidebarFull)  sidebarFull.textContent  = fullName;
    if (sidebarUser)  sidebarUser.textContent  = `@${username}`;

  } catch {
    // Server unreachable — still show the page but warn
    console.warn('Could not verify session.');
  }
}

// ── Logout ───────────────────────────────────────────────────
async function doLogout() {
  if (!confirm('Are you sure you want to log out?')) return;
  try {
    await fetch('/api/auth/logout', { method: 'POST' });
  } catch { /* ignore errors, redirect anyway */ }
  window.location.href = '/admin/login.html';
}

// Run session check first, then load queue data
checkSession();
loadQueues();
setInterval(loadQueues, 8000); // Auto-refresh admin table every 8s
