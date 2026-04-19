using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolQueueSystem.Data;
using SchoolQueueSystem.Models;

namespace SchoolQueueSystem.Controllers
{
    // ════════════════════════════════════════════════════════════════
    //  AuthController — handles login, register, logout, and session checks
    //
    //  HOW SESSIONS WORK HERE:
    //  When an admin logs in successfully, we store their username in
    //  the server-side session (like a temporary memory slot).
    //  Every protected admin action checks if that session slot is filled.
    //  When they log out, we clear it.
    // ════════════════════════════════════════════════════════════════

    [ApiController]
    [Route("api/[controller]")] // Base URL: /api/auth
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        // The key we use to store the logged-in admin's username in the session
        private const string SESSION_KEY = "AdminUsername";

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────
        // POST /api/auth/register
        // Body: { "username": "...", "password": "...", "fullName": "..." }
        // Creates a new admin account
        // ─────────────────────────────────────────────
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(req.Username) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.FullName))
            {
                return BadRequest(new { message = "All fields are required." });
            }

            if (req.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters." });

            // Check if username is already taken (usernames must be unique)
            bool taken = await _db.AdminUsers.AnyAsync(u => u.Username == req.Username.ToLower().Trim());
            if (taken)
                return Conflict(new { message = "That username is already taken. Please choose another." });

            // Hash the password using BCrypt before saving
            // BCrypt automatically adds a "salt" so even identical passwords
            // produce different hashes — very secure!
            string hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var newAdmin = new AdminUser
            {
                Username     = req.Username.ToLower().Trim(),
                PasswordHash = hash,
                FullName     = req.FullName.Trim(),
                CreatedAt    = DateTime.Now,
            };

            _db.AdminUsers.Add(newAdmin);
            await _db.SaveChangesAsync();

            // Automatically log them in after registering
            HttpContext.Session.SetString(SESSION_KEY, newAdmin.Username);

            return Ok(new { message = $"Account created! Welcome, {newAdmin.FullName}.", username = newAdmin.Username });
        }

        // ─────────────────────────────────────────────
        // POST /api/auth/login
        // Body: { "username": "...", "password": "..." }
        // Verifies credentials and starts a session
        // ─────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Username and password are required." });

            // Find admin by username (stored lowercase)
            var admin = await _db.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == req.Username.ToLower().Trim());

            // BCrypt.Verify checks the entered password against the stored hash
            // If no user found OR password doesn't match → same generic error
            // (We don't say which one failed — that would help attackers)
            if (admin == null || !BCrypt.Net.BCrypt.Verify(req.Password, admin.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password." });

            // Login successful — save username in session
            HttpContext.Session.SetString(SESSION_KEY, admin.Username);

            return Ok(new { message = $"Welcome back, {admin.FullName}!", username = admin.Username, fullName = admin.FullName });
        }

        // ─────────────────────────────────────────────
        // POST /api/auth/logout
        // Clears the session (logs the admin out)
        // ─────────────────────────────────────────────
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        // ─────────────────────────────────────────────
        // GET /api/auth/me
        // Returns the currently logged-in admin's info
        // Used by the frontend to check if session is active
        // ─────────────────────────────────────────────
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var username = HttpContext.Session.GetString(SESSION_KEY);
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Not logged in." });

            var admin = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username);
            if (admin == null)
            {
                HttpContext.Session.Clear();
                return Unauthorized(new { message = "Session invalid." });
            }

            return Ok(new { username = admin.Username, fullName = admin.FullName });
        }
    }

    // ─────────────────────────────────────────────
    // Request body models (what the frontend sends)
    // ─────────────────────────────────────────────

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
