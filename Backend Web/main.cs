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
        private IMongoCollection<User>? _users;


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
            _users = mongoDatabase.GetCollection<User>("users");
            services.AddSingleton<IMongoCollection<Complaint>>(_complaints);
            services.AddSingleton<IMongoCollection<User>>(_users);

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
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public ObjectId? Id { get; set; }

        [BsonElement("fullname")]
        public string? FullName { get; set; }

        [BsonElement("username")]
        public string? UserName { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("password")]
        public string? Password { get; set; }

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

    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;

        public UsersController(IMongoCollection<User> users)
        {
            _users = users;
        }

        [HttpGet("/users")]
        public async Task<IActionResult> GetUsers()
        {
            var filter = Builders<User>.Filter.Empty;
            var users = await _users.Find(filter).ToListAsync();
            return Ok(users);
        }

        [HttpGet("/users/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var filter = Builders<User>.Filter.Eq(c => c.Id, ObjectId.Parse(userId));
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("/users")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user?.FullName) || string.IsNullOrWhiteSpace(user?.UserName) || string.IsNullOrWhiteSpace(user?.Email) || string.IsNullOrWhiteSpace(user?.Password))
            {
                return BadRequest();
            }
            user.Id = ObjectId.GenerateNewId();
            await _users.InsertOneAsync(user);
            return Ok(user);
        }

        [HttpDelete("/users/{userId}")]
        public async Task<IActionResult> DeleteUserById(string userId)
        {
            var filter = Builders<User>.Filter.Eq(c => c.Id, ObjectId.Parse(userId));
            var result = await _users.DeleteOneAsync(filter);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpGet("/users/{email}/{password}")]
        public async Task<IActionResult> CheckIfUserExists(string email, string password)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Email, email),
                Builders<User>.Filter.Eq(u => u.Password, password)
            );
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
