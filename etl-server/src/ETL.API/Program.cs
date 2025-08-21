using System.IdentityModel.Tokens.Jwt;
using ETL.API.Infrastructure;
using ETL.API.Infrastructure.Authentication;
// using ETL.API.Middleware;
using ETL.Application;
using ETL.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SystemAdminOnly", policy =>
        policy.RequireRole("system_admin"));

    options.AddPolicy("DataAdminOnly", policy =>
        policy.RequireRole("data_admin"));

    options.AddPolicy("AnalystOnly", policy =>
        policy.RequireRole("analyst"));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("system_admin"));
    
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddKeycloakStatelessCookieAuth(builder.Configuration); // clean extension


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddHttpClient(string.Empty, client => { /* can configure client defaults here */ })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        // ⚠️ WARNING: DANGEROUS - FOR DEVELOPMENT ONLY
        // This handler bypasses SSL certificate validation.
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionHandler();

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseKeycloakClaims(); // custom middleware
app.UseAuthorization();



app.MapControllers();

app.Run();
