using ClearCut.Web.Components;
using ClearCut.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddScoped<AgentClient>();
builder.Services.AddSingleton<IIdentityTokenProvider, GoogleIdentityTokenProvider>();
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

// Strict production validation
if (!app.Environment.IsDevelopment())
{
    var useIdToken = app.Configuration.GetValue<bool>("CLEARCUT_AGENT_USE_ID_TOKEN", false);
    var agentBaseUrl = app.Configuration["CLEARCUT_AGENT_BASE_URL"];

    if (useFixtures)
    {
        throw new InvalidOperationException("CRITICAL SECURITY VIOLATION: UseFixtures must be false in production.");
    }
    if (!useIdToken)
    {
        throw new InvalidOperationException("CRITICAL SECURITY VIOLATION: CLEARCUT_AGENT_USE_ID_TOKEN must be true in production.");
    }
    if (string.IsNullOrWhiteSpace(agentBaseUrl) ||
        !Uri.TryCreate(agentBaseUrl, UriKind.Absolute, out var uri) ||
        uri.Scheme != Uri.UriSchemeHttps ||
        uri.IsLoopback)
    {
        throw new InvalidOperationException("CRITICAL SECURITY VIOLATION: CLEARCUT_AGENT_BASE_URL must be an absolute HTTPS URL and not point to localhost in production.");
    }
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

app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/healthz/agent", async (AgentClient agentClient, CancellationToken cancellationToken) =>
{
    var healthy = await agentClient.IsHealthyAsync(cancellationToken);
    return healthy
        ? Results.Ok(new { status = "healthy" })
        : Results.Json(new { status = "unavailable" }, statusCode: 503);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
