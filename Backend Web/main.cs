using MongoDB.Driver;


namespace Backend_Web
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
}
