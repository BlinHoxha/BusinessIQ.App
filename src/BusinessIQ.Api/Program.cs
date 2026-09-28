using BusinessIQ.Application.Abstractions;
using BusinessIQ.Api.Security;
using BusinessIQ.Infrastructure;
using BusinessIQ.Application;
using BusinessIQ.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new()
{
    Title = "BusinessIQ API",
    Version = "v1",
    Description = "AI Business Intelligence & Advisory Platform. Business management and evidence-backed document answers."
}));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RequestExceptionHandler>();
builder.Services.AddBusinessApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentOrganization, CurrentOrganization>();
builder.Services.AddBusinessInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddBusinessAi(builder.Configuration, builder.Environment);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.Audience = builder.Configuration["Authentication:Audience"];
    options.MapInboundClaims = false;
    options.TokenValidationParameters.RoleClaimType = "role";
});
if (!builder.Environment.IsDevelopment() &&
    (string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Authority"]) ||
     string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Audience"])))
    throw new InvalidOperationException("Authentication:Authority and Authentication:Audience are required outside Development.");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageBusinesses", policy => policy.RequireAssertion(context =>
        builder.Environment.IsDevelopment() ||
        (context.User.Identity?.IsAuthenticated == true && context.User.IsInRole("owner"))));
    if (!builder.Environment.IsDevelopment())
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();
app.Run();

public partial class Program;
