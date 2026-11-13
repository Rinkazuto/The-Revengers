var builder = WebApplication.CreateBuilder(args); // start a new API

// Describe our endpoints (for the Swagger page)
builder.Services.AddOpenApi();

// Lets our endpoints call outside websites/APIs later
builder.Services.AddHttpClient();

// CORS: allow the frontend to call this API from the browser
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Swagger page at /swagger
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PowerCheck API"));

app.UseCors();

// Quick check that the API is alive
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run(); 