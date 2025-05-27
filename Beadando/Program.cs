using Beadando.Models;
using Beadando.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Beadando.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "SessionKey";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // For localhost
    options.LoginPath = "/api/Session/login";
    options.LogoutPath = "/api/Session/logout";
    
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            var sessionService = context.HttpContext.RequestServices.GetRequiredService<SessionService>();
            try
            {
                await sessionService.ValidateSessionAsync(context.HttpContext.Request, context.HttpContext.Response);
            }
            catch (UnauthorizedAccessException)
            {
                context.RejectPrincipal();
            }
        },
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true);
    });
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var seeder = services.GetRequiredService<DataSeeder>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Delete and recreate database
        logger.LogInformation("Deleting existing database...");
        await context.Database.EnsureDeletedAsync();
        
        // Ensure database exists
        logger.LogInformation("Creating new database...");
        await context.Database.EnsureCreatedAsync();
        
        // Seed data from JSON files
        logger.LogInformation("Starting data seeding...");
        await seeder.SeedDataAsync();
        
        logger.LogInformation("Database setup and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while setting up the database.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = "swagger"; // <--- FONTOS!
    });
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
 

app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Urls.Clear();
app.Urls.Add("http://localhost:5165");

app.Run();