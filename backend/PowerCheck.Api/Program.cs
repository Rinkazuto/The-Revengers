using Microsoft.EntityFrameworkCore;
using PowerCheck.Api.Data;

var builder = WebApplication.CreateBuilder(args); // start a new API

// Describe our endpoints (for the Swagger page)
builder.Services.AddOpenApi();

// Lets our endpoints call outside websites/APIs later
builder.Services.AddHttpClient();

// CORS: allow the frontend to call this API from the browser
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Database: one SQLite file (app.db), set in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Create or update the tables, then load the real CEB, PVGIS and solar data from data/seed
// (only when the tables are empty, so restarting never duplicates rows)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Seeder.Run(db, Path.Combine(app.Environment.ContentRootPath, "..", "..", "data", "seed"));
}

// Swagger page at /swagger
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PowerCheck API"));

app.UseCors();

// Quick check that the API is alive
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapBillEndpoints();
app.MapSolarEndpoints();
app.MapReadingEndpoints();
app.MapCycloneEndpoints();
app.Run(); 