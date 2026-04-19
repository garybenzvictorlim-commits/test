# 🚀 Deployment Guide
## How to Upload to GitHub & Host Online

---

## PART 1 — Upload to GitHub

### Step 1A — Create a GitHub Account
If you don't have one yet:
1. Go to https://github.com
2. Click **Sign up** and follow the steps
3. Verify your email

---

### Step 1B — Install Git on Your Computer
Git is the tool that talks to GitHub.

**Windows:**
- Download from https://git-scm.com/download/win
- Install with default settings
- After installing, open VS Code → Terminal and type:
  ```
  git --version
  ```
  You should see something like `git version 2.x.x`

---

### Step 1C — Create a New Repository on GitHub
1. Go to https://github.com and log in
2. Click the **➕ New** button (top right) → **New repository**
3. Fill in:
   - **Repository name:** `school-queue-system`
   - **Description:** School queue tracker for tuition, registrar, etc.
   - **Visibility:** Public (or Private — your choice)
   - ❌ Do NOT check "Add a README" (we already have one)
4. Click **Create repository**
5. GitHub will show you a page with commands — keep it open!

---

### Step 1D — Push Your Code from VS Code

Open the Terminal in VS Code (`Ctrl + ~`) and run these commands
**one by one**, inside your `SchoolQueueSystem` folder:

```bash
# 1. Initialize Git in your project folder
git init

# 2. Stage all files for the first commit
git add .

# 3. Create your first commit (a "snapshot" of your code)
git commit -m "Initial commit: School Queue System with admin login"

# 4. Rename the default branch to 'main' (GitHub's standard)
git branch -M main

# 5. Connect your local folder to your GitHub repository
#    REPLACE the URL below with YOUR repository URL from GitHub!
git remote add origin https://github.com/YOUR_USERNAME/school-queue-system.git

# 6. Push (upload) your code to GitHub
git push -u origin main
```

> 💡 GitHub may ask you to log in the first time.
> Use your GitHub username and a **Personal Access Token** as the password.
> To create a token: GitHub → Settings → Developer Settings → Personal access tokens → Generate new token (classic) → check "repo" scope.

After this, refresh your GitHub page — you should see all your files! ✅

---

### Step 1E — Updating GitHub in the Future

Every time you make changes and want to save them to GitHub:

```bash
git add .
git commit -m "Describe what you changed here"
git push
```

That's it! Three commands every time.

---

## PART 2 — ⚠️ Why Vercel Doesn't Work for This Project

Vercel is built for **JavaScript/TypeScript** frontends and serverless functions.
It does **not** support:
- Running a persistent C# / ASP.NET server
- Keeping a SQLite `.db` file alive between requests

**Use one of the alternatives below instead.**

---

## PART 3 — Free Hosting Options for This Project

---

### Option A — Railway.app ⭐ (Recommended — Easiest)

Railway runs your full .NET app on a real server, for free.

#### Step 1: Sign up
- Go to https://railway.app
- Click **Login** → **Login with GitHub**
- Authorize Railway to access your GitHub

#### Step 2: Create a new project
1. Click **New Project**
2. Click **Deploy from GitHub repo**
3. Select your `school-queue-system` repository
4. Railway detects it's a .NET app automatically ✅

#### Step 3: Set the start command
Railway needs to know how to start your app.
1. In your project dashboard, click your service
2. Go to **Settings** → **Deploy** → **Start Command**
3. Enter:
   ```
   dotnet run --urls http://0.0.0.0:$PORT
   ```

#### Step 4: Update Program.cs for Railway
Railway provides the port through an environment variable.
Change the last line of `Program.cs` from:
```csharp
app.Run("http://localhost:5000");
```
To:
```csharp
// Reads the PORT variable Railway provides, falls back to 5000 locally
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
```

Then push the change to GitHub:
```bash
git add .
git commit -m "Fix port for Railway deployment"
git push
```

Railway auto-deploys whenever you push to GitHub. 🚀

#### Step 5: Get your public URL
- In Railway dashboard, go to your service → **Settings** → **Networking**
- Click **Generate Domain**
- You get a URL like: `https://school-queue-system-production.up.railway.app`

#### ⚠️ SQLite on Railway
Railway's file system resets on each deploy, so `queue.db` will be wiped.
For a permanent database, Railway also offers free **PostgreSQL** add-ons.
But for school demos, SQLite is fine — just remember data resets on redeploy.

---

### Option B — Render.com

Similar to Railway. Also free, also supports .NET.

1. Go to https://render.com → Sign up with GitHub
2. Click **New** → **Web Service**
3. Connect your GitHub repo
4. Set:
   - **Environment:** Docker (or .NET)
   - **Build Command:** `dotnet publish -c Release -o out`
   - **Start Command:** `dotnet out/SchoolQueueSystem.dll`
5. Render gives you a free `*.onrender.com` URL

Same port fix as Railway — use `Environment.GetEnvironmentVariable("PORT")`.

---

### Option C — ngrok (Quickest for demos, no deployment needed)

This lets your local computer be accessible from anywhere on the internet.
Perfect for demos or showing the teacher — no server setup needed!

#### Step 1: Install ngrok
- Go to https://ngrok.com → Sign up (free)
- Download ngrok for Windows
- Extract the `.exe` file anywhere

#### Step 2: Authenticate ngrok
Run this in Terminal (replace with your token from ngrok dashboard):
```bash
ngrok config add-authtoken YOUR_TOKEN_HERE
```

#### Step 3: Start your app
```bash
dotnet run
```

#### Step 4: In a SECOND terminal window, run ngrok
```bash
ngrok http 5000
```

You'll see output like:
```
Forwarding  https://abc123.ngrok.io -> http://localhost:5000
```

Share `https://abc123.ngrok.io` with anyone!
- Student board: `https://abc123.ngrok.io/student/index.html`
- Admin panel:   `https://abc123.ngrok.io/admin/login.html`

> ⚠️ The URL changes every time you restart ngrok (on free plan).
> The tunnel only works while your computer is on and ngrok is running.

---

## PART 4 — Quick Summary

| Method | Cost | Difficulty | SQLite | Permanent URL |
|--------|------|------------|--------|---------------|
| Railway | Free | ⭐ Easy | ⚠️ Resets on deploy | ✅ Yes |
| Render | Free | ⭐⭐ Medium | ⚠️ Resets on deploy | ✅ Yes |
| ngrok | Free | ⭐ Instant | ✅ Yes (local) | ❌ Changes each time |
| Vercel | Free | ❌ Incompatible | ❌ No | — |

**Recommendation for a school project:**
- For demos → use **ngrok** (5 minutes to set up)
- For permanent hosting → use **Railway** (free, ~15 min setup)

---

## PART 5 — Common GitHub Issues

| Problem | Fix |
|---------|-----|
| `git: command not found` | Install Git from git-scm.com |
| `remote origin already exists` | Run `git remote remove origin` then re-add |
| Asked for password on `git push` | Use a Personal Access Token, not your GitHub password |
| `queue.db` got committed | It's in `.gitignore` — run `git rm --cached queue.db` |
| Want to update code | `git add . && git commit -m "message" && git push` |
