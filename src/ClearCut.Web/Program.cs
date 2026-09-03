using ClearCut.Web.Components;
using ClearCut.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddScoped<AgentClient>();
builder.Services.AddScoped<ReviewSessionStore>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

// Strict fail-closed production fixture lockout: Fail at startup if UseFixtures is true outside Development
var useFixtures = app.Configuration.GetValue<bool>("UseFixtures", false);
if (useFixtures && !app.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "CRITICAL SECURITY VIOLATION: Fixture mode is enabled in a non-Development environment. " +
        "The application must fail closed."
    );
}

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
