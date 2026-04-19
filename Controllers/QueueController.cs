using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolQueueSystem.Data;
using SchoolQueueSystem.Models;

namespace SchoolQueueSystem.Controllers
{
    // This controller handles all queue-related API calls.
    // The frontend (HTML pages) will call these endpoints using JavaScript fetch().
    [ApiController]
    [Route("api/[controller]")] // Base URL: /api/queue
    public class QueueController : ControllerBase
    {
        private readonly AppDbContext _db;

        // Dependency Injection: ASP.NET automatically provides the database context
        public QueueController(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────
        // GET /api/queue
        // Returns ALL queue counters (for the student display board)
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var queues = await _db.QueueCounters.ToListAsync();
            return Ok(queues);
        }

        // ─────────────────────────────────────────────
        // GET /api/queue/5
        // Returns a SINGLE queue counter by its ID
        // ─────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var queue = await _db.QueueCounters.FindAsync(id);
            if (queue == null) return NotFound(new { message = "Queue not found." });
            return Ok(queue);
        }

        // ─────────────────────────────────────────────
        // POST /api/queue/5/next
        // Advances the queue: CurrentNumber = CurrentNumber + 1
        // Called by the admin when they finish serving the current student
        // ─────────────────────────────────────────────
        [HttpPost("{id}/next")]
        public async Task<IActionResult> CallNext(int id)
        {
            var queue = await _db.QueueCounters.FindAsync(id);
            if (queue == null) return NotFound(new { message = "Queue not found." });

            if (queue.CurrentNumber >= queue.NextNumber - 1)
                return BadRequest(new { message = "No more numbers in queue." });

            queue.CurrentNumber++;
            queue.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(queue);
        }

        // ─────────────────────────────────────────────
        // POST /api/queue/5/issue
        // Issues the next queue number to a student
        // Returns the number they should take
        // ─────────────────────────────────────────────
        [HttpPost("{id}/issue")]
        public async Task<IActionResult> IssueNumber(int id)
        {
            var queue = await _db.QueueCounters.FindAsync(id);
            if (queue == null) return NotFound(new { message = "Queue not found." });
            if (queue.Status == "Closed") return BadRequest(new { message = "This queue is currently closed." });

            int issuedNumber = queue.NextNumber;
            queue.NextNumber++;
            queue.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new { issuedNumber, message = $"Your number is {issuedNumber}. Please wait." });
        }

        // ─────────────────────────────────────────────
        // PUT /api/queue/5
        // Full update: admin can manually set CurrentNumber, NextNumber, Status, Name
        // ─────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QueueCounter updated)
        {
            var queue = await _db.QueueCounters.FindAsync(id);
            if (queue == null) return NotFound(new { message = "Queue not found." });

            queue.Name = updated.Name;
            queue.CurrentNumber = updated.CurrentNumber;
            queue.NextNumber = updated.NextNumber;
            queue.Status = updated.Status;
            queue.LastUpdated = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok(queue);
        }

        // ─────────────────────────────────────────────
        // POST /api/queue/5/reset
        // Resets the queue to 0 (start of day)
        // ─────────────────────────────────────────────
        [HttpPost("{id}/reset")]
        public async Task<IActionResult> Reset(int id)
        {
            var queue = await _db.QueueCounters.FindAsync(id);
            if (queue == null) return NotFound(new { message = "Queue not found." });

            queue.CurrentNumber = 0;
            queue.NextNumber = 1;
            queue.LastUpdated = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Queue '{queue.Name}' has been reset." });
        }

        // ─────────────────────────────────────────────
        // POST /api/queue
        // Create a NEW queue counter (admin feature)
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QueueCounter newQueue)
        {
            newQueue.CurrentNumber = 0;
            newQueue.NextNumber = 1;
            newQueue.LastUpdated = DateTime.Now;

            _db.QueueCounters.Add(newQueue);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newQueue.Id }, newQueue);
        }

        // ─────────────────────────────────────────────
        // DELETE /api/queue/5
        // Delete a queue counter
        // ─────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var queue = await _db.QueueCounters.FindAsync(id);
            if (queue == null) return NotFound(new { message = "Queue not found." });

            _db.QueueCounters.Remove(queue);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Queue deleted." });
        }
    }
}
