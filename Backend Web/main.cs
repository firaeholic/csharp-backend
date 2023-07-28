using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace YourNamespace
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<Complaint>? _complaints;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "_myAllowSpecificOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.AddControllers();

            var dbClient = new MongoClient("mongodb://127.0.0.1:27017");
            var mongoDatabase = dbClient.GetDatabase("complaints");
            _complaints = mongoDatabase.GetCollection<Complaint>("complaints");
            services.AddSingleton<IMongoCollection<Complaint>>(_complaints);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("_myAllowSpecificOrigins");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class Complaint
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public ObjectId? Id { get; set; }


        [BsonElement("complaint")]
        public string? ComplaintText { get; set; }

        [BsonElement("__v")]
        public int? V { get; set; }
    }

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
