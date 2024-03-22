using API.Extensions;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
// TODO: Put this back to Development mode. For now I'm troubleshooting the CI/CD pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseSession();

app.UseHttpsRedirection();

app.UseCors("ReactCorsPolicy");

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex)
{
    //var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

// Obtain an instance of the logger
logger.LogInformation("Application is listening on: {0}", app.Environment.WebRootPath);

app.Run();
