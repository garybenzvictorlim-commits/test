namespace SchoolQueueSystem.Models
{
    // This class represents one "service window" in the school
    // e.g. "Tuition Payment", "Registrar", "Scholarship Office"
    public class QueueCounter
    {
        public int Id { get; set; }                        // Auto-generated ID
        public string Name { get; set; } = string.Empty;  // e.g. "Tuition Payment"
        public int CurrentNumber { get; set; } = 0;        // The number currently being served
        public int NextNumber { get; set; } = 1;           // The next number to be given out
        public string Status { get; set; } = "Open";       // "Open" or "Closed"
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
