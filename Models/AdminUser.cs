namespace SchoolQueueSystem.Models
{
    // Represents one admin account in the database.
    // Passwords are stored as a BCrypt hash — NEVER plain text.
    public class AdminUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;   // e.g. "admin1"

        // This stores the hashed password, not the real one.
        // Example: "$2a$11$..." — looks like garbage, that's the point!
        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;   // e.g. "Maria Santos"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
