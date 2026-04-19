# 📋 School Queue Tracking System
## How to Run — Step-by-Step Guide (VS Code)

---

## 🗂️ Project Structure

```
SchoolQueueSystem/
├── Controllers/
│   └── QueueController.cs      ← All the API endpoints (the "brain")
├── Data/
│   └── AppDbContext.cs         ← Database setup and seed data
├── Models/
│   └── QueueCounter.cs         ← The data model (what a queue looks like)
├── wwwroot/
│   ├── admin/
│   │   └── index.html          ← Admin Panel (for staff)
│   ├── student/
│   │   └── index.html          ← Student Display Board
│   ├── css/
│   │   ├── admin.css
│   │   └── student.css
│   └── js/
│       ├── admin.js
│       └── student.js
├── Program.cs                  ← App startup & configuration
├── appsettings.json
└── SchoolQueueSystem.csproj    ← Project file (lists dependencies)
```

---

## ✅ Step 1 — Install Prerequisites

You need these installed on your computer:

### 1A. .NET 8 SDK
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- After installing, open a terminal and verify:
  ```
  dotnet --version
  ```
  You should see something like `8.0.xxx`

### 1B. VS Code Extensions (install from the Extensions tab in VS Code)
- **C# Dev Kit** (by Microsoft) — provides IntelliSense and run support
- **SQLite Viewer** (optional, but nice) — lets you see the database file

---

## ✅ Step 2 — Open the Project in VS Code

1. Open VS Code
2. Go to **File → Open Folder**
3. Select the `SchoolQueueSystem` folder
4. VS Code will detect it as a .NET project

---

## ✅ Step 3 — Restore Packages (Download Dependencies)

Open the **Terminal** in VS Code (`Ctrl + ~`) and run:

```bash
dotnet restore
```

This downloads the NuGet packages listed in the `.csproj` file:
- `Microsoft.EntityFrameworkCore` — ORM (lets C# talk to the database)
- `Microsoft.EntityFrameworkCore.Sqlite` — SQLite database driver

---

## ✅ Step 4 — Run the Application

In the terminal, run:

```bash
dotnet run
```

You should see this output:
```
=================================================
  School Queue System is running!
  Student Display : http://localhost:5000/student/index.html
  Admin Panel     : http://localhost:5000/admin/index.html
  API Base URL    : http://localhost:5000/api/queue
=================================================
```

> 💡 The first time you run it, the SQLite database file `queue.db` is
> automatically created in the project folder with 4 default counters.

---

## ✅ Step 5 — Open the Pages

Open your browser and go to:

| Page | URL |
|------|-----|
| 🖥️ Student Display Board | http://localhost:5000/student/index.html |
| 🔑 Admin Login / Register | http://localhost:5000/admin/login.html |
| ⚙️ Admin Panel (requires login) | http://localhost:5000/admin/index.html |
| ⚙️ Admin Panel (requires login) | http://localhost:5000/admin/index.html |
| 🔌 Raw API (JSON) | http://localhost:5000/api/queue |

---

## 🌐 Sharing on a Local Network (School Wi-Fi)

To let other devices on the same Wi-Fi see the display board:

1. Find your computer's local IP:
   - Windows: run `ipconfig` in Command Prompt → look for `IPv4 Address`
   - e.g., `192.168.1.5`

2. Change the last line of `Program.cs` from:
   ```csharp
   app.Run("http://localhost:5000");
   ```
   To:
   ```csharp
   app.Run("http://0.0.0.0:5000");
   ```

3. Restart the server (`dotnet run`)

4. Other devices can now access it at:
   ```
   http://192.168.1.5:5000/student/index.html
   ```

---

## 🔌 API Reference (for developers)

The backend exposes a REST API at `/api/queue`:

| Method | URL | What it does |
|--------|-----|--------------|
| GET | `/api/queue` | Get all queues |
| GET | `/api/queue/{id}` | Get one queue by ID |
| POST | `/api/queue` | Create a new queue |
| PUT | `/api/queue/{id}` | Update a queue (name, numbers, status) |
| DELETE | `/api/queue/{id}` | Delete a queue |
| POST | `/api/queue/{id}/next` | Advance to the next number (call next student) |
| POST | `/api/queue/{id}/issue` | Issue a number to a student |
| POST | `/api/queue/{id}/reset` | Reset the queue to zero |

### Example: Manually call the API
Open your browser or use a tool like Postman:
```
GET http://localhost:5000/api/queue
```
Returns:
```json
[
  {
    "id": 1,
    "name": "Tuition Payment",
    "currentNumber": 3,
    "nextNumber": 8,
    "status": "Open",
    "lastUpdated": "2025-01-01T10:30:00"
  },
  ...
]
```

---

## 🗄️ Database

- Uses **SQLite** — a single file called `queue.db` in the project root
- No separate database server needed!
- The database is created automatically when you first `dotnet run`
- To view the raw data, install the **SQLite Viewer** extension in VS Code
  then open `queue.db`

---

## 🛠️ Common Issues

| Problem | Fix |
|---------|-----|
| `dotnet: command not found` | Install .NET 8 SDK from microsoft.com/dotnet |
| Port 5000 already in use | Change the port in `Program.cs` to e.g. `5001` |
| Page shows "Cannot connect to server" | Make sure `dotnet run` is running in the terminal |
| Database error on startup | Delete `queue.db` and restart — it will be recreated |

---

## 💡 How to Add More Queues

**Via Admin Panel (easiest):**
1. Go to http://localhost:5000/admin/index.html
2. Click "➕ Add Queue Counter" in the sidebar
3. Fill in the name and click Create

**Via Code (permanent default):**
Open `Data/AppDbContext.cs` and add another entry inside `OnModelCreating()`:
```csharp
new QueueCounter { Id = 5, Name = "Library Clearance", CurrentNumber = 0, NextNumber = 1, Status = "Open", LastUpdated = DateTime.Now }
```
Then delete `queue.db` and restart.

---

Made with ❤️ for school queue management
