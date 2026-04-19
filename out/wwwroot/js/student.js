// ════════════════════════════════════════════════════════════
//  student.js — Logic for the Student Queue Display Board
//
//  What this file does:
//  1. Fetches all queue data from the API every 5 seconds
//  2. Renders queue cards on the board
//  3. Renders "Get a Number" buttons
//  4. Shows a ticket modal when a student requests a number
// ════════════════════════════════════════════════════════════

const API_BASE = '/api/queue'; // Base URL for all API calls

// ── Clock ────────────────────────────────────────────────────
function updateClock() {
  const now = new Date();
  document.getElementById('current-time').textContent =
    now.toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  document.getElementById('current-date').textContent =
    now.toLocaleDateString('en-PH', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
}
setInterval(updateClock, 1000);
updateClock();

// ── Fetch & Render Queues ────────────────────────────────────
async function loadQueues() {
  try {
    const response = await fetch(API_BASE);
    if (!response.ok) throw new Error('API error');
    const queues = await response.json();
    renderBoard(queues);
    renderIssueButtons(queues);
  } catch (err) {
    document.getElementById('queue-grid').innerHTML =
      '<div class="loading-msg">⚠️ Could not connect to server. Is it running?</div>';
  }
}

// ── Render the main queue display cards ──────────────────────
function renderBoard(queues) {
  const grid = document.getElementById('queue-grid');
  if (queues.length === 0) {
    grid.innerHTML = '<div class="loading-msg">No queues configured yet.</div>';
    return;
  }

  grid.innerHTML = queues.map(q => {
    const isClosed = q.status === 'Closed';
    const waiting = Math.max(0, q.nextNumber - q.currentNumber - 1);
    return `
      <div class="queue-card ${isClosed ? 'closed' : ''}">
        <span class="card-status-badge ${isClosed ? 'badge-closed' : 'badge-open'}">
          ${isClosed ? '🔴 Closed' : '🟢 Open'}
        </span>
        <div class="card-counter-name">${escapeHtml(q.name)}</div>
        <div class="card-now-serving-label">Now Serving</div>
        <div class="card-current-number">${q.currentNumber === 0 ? '—' : q.currentNumber}</div>
        <hr class="card-divider"/>
        <div class="card-meta">
          <span>⏳ Waiting: <strong>${waiting}</strong></span>
          <span>🎟️ Last: <strong>${q.nextNumber - 1 || '—'}</strong></span>
        </div>
      </div>
    `;
  }).join('');
}

// ── Render "Get a Number" buttons ────────────────────────────
function renderIssueButtons(queues) {
  const grid = document.getElementById('issue-grid');
  grid.innerHTML = queues.map(q => {
    const isClosed = q.status === 'Closed';
    return `
      <button
        class="issue-btn ${isClosed ? 'closed-btn' : ''}"
        onclick="${isClosed ? '' : `issueNumber(${q.id}, '${escapeHtml(q.name)}')`}"
        ${isClosed ? 'disabled title="This counter is closed"' : ''}
      >
        <span>🏷️</span>
        ${escapeHtml(q.name)}
      </button>
    `;
  }).join('');
}

// ── Issue a queue number to a student ────────────────────────
async function issueNumber(queueId, queueName) {
  try {
    const response = await fetch(`${API_BASE}/${queueId}/issue`, { method: 'POST' });
    const data = await response.json();

    if (!response.ok) {
      alert('⚠️ ' + (data.message || 'Could not issue a number.'));
      return;
    }

    // Show the ticket modal
    document.getElementById('modal-counter-name').textContent = queueName;
    document.getElementById('modal-number').textContent = data.issuedNumber;
    document.getElementById('modal-overlay').classList.add('active');

    // Refresh board right away so the count updates
    loadQueues();
  } catch {
    alert('⚠️ Server connection error. Please try again.');
  }
}

// ── Modal close ───────────────────────────────────────────────
function closeModal() {
  document.getElementById('modal-overlay').classList.remove('active');
}

// Close modal if user clicks the background
document.getElementById('modal-overlay').addEventListener('click', function(e) {
  if (e.target === this) closeModal();
});

// ── Utility: prevent XSS by escaping HTML characters ─────────
function escapeHtml(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

// ── Auto-refresh every 5 seconds ─────────────────────────────
loadQueues(); // Load immediately on page open
setInterval(loadQueues, 5000); // Then refresh every 5s
