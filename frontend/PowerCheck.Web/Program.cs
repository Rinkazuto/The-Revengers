using PowerCheck.Web.Components;
using MudBlazor.Services;
using PowerCheck.Web.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Fake or real API, chosen by "UseMockApi" in appsettings.json
if (builder.Configuration.GetValue<bool>("UseMockApi"))
{
    builder.Services.AddSingleton<IPowerCheckApi, MockPowerCheckApi>();
}
else
{
    builder.Services.AddHttpClient<IPowerCheckApi, PowerCheckApi>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
        client.Timeout = TimeSpan.FromSeconds(15);
    });
}

// The last kWh and tariff the user calculated, shared by the Bill and Solar screens (one per phone)
builder.Services.AddScoped<CalculationState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
