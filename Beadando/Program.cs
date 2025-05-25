using Beadando.Models;
using Beadando.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Beadando.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Apply migrations
        context.Database.Migrate();
        
        // Seed data
        await QuestionSeed.SeedQuestionsAsync(context);
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database setup completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while setting up the database.");
        throw; // Rethrow to fail startup if database setup fails
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
app.UseAuthorization();

app.MapControllers();


app.Urls.Clear();
app.Urls.Add("http://localhost:5165");

app.Run();