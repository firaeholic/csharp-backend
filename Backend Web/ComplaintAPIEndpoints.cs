using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_Web
{
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly IMongoCollection<Complaint> _complaints;

        public ComplaintsController(IMongoCollection<Complaint> complaints)
        {
            _complaints = complaints;
        }

        [HttpGet("/")]
        public string Get()
        {
            return "Yoo";
        }

        [HttpGet("/complaints")]
        public async Task<IActionResult> GetComplaints()
        {
            var filter = Builders<Complaint>.Filter.Empty;
            var complaints = await _complaints.Find(filter).ToListAsync();
            return Ok(complaints);
        }

        [HttpGet("/complaints/{complaintId}")]
        public async Task<IActionResult> GetComplaintById(string complaintId)
        {
            var filter = Builders<Complaint>.Filter.Eq(c => c.Id, ObjectId.Parse(complaintId));
            var complaint = await _complaints.Find(filter).FirstOrDefaultAsync();
            if (complaint == null)
            {
                return NotFound();
            }
            return Ok(complaint);
        }

        [HttpPost("/complaints")]
        public async Task<IActionResult> CreateComplaint([FromBody] Complaint complaint)
        {
            if (string.IsNullOrWhiteSpace(complaint?.ComplaintText))
            {
                return BadRequest();
            }
            complaint.Id = ObjectId.GenerateNewId();

            complaint.V = 0;

            await _complaints.InsertOneAsync(complaint);
            return Ok(complaint);
        }

        [HttpDelete("/complaints/{complaintId}")]
        public async Task<IActionResult> DeleteComplaintById(string complaintId)
        {
            var filter = Builders<Complaint>.Filter.Eq(c => c.Id, ObjectId.Parse(complaintId));
            var result = await _complaints.DeleteOneAsync(filter);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
