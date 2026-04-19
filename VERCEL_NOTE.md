# ⚠️ Important Note About Vercel Deployment

Vercel is designed for **frontend-only** or **serverless** apps (Node.js, Python, etc.).

It does **NOT** natively support:
- ASP.NET / C# backend servers
- Persistent file-based databases like SQLite (`queue.db`)

## ✅ Recommended Free Hosting Alternatives for This Project

### Option A — Railway.app (EASIEST for .NET + SQLite)
Railway supports .NET apps and gives you a real server.
See: DEPLOY_GUIDE.md → Section 3A

### Option B — Render.com (Also great for .NET)
Similar to Railway, free tier available.
See: DEPLOY_GUIDE.md → Section 3B

### Option C — Run locally + share via ngrok (Quickest demo)
Run on your laptop and share a public URL in seconds.
See: DEPLOY_GUIDE.md → Section 3C

---

Vercel CAN host the student display board as a static site
if you move to a cloud database (e.g. PlanetScale, Supabase),
but that requires rewriting the backend to serverless functions.
That is a bigger project — start with Railway or Render instead.
