using API.Extensions;
using DeepServices;
using Domain.SentimentAnalysis;
using Domain.MNIST;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Persistence;
using Domain.DeckCrack;
using Domain.Image;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

var predictionService = services.GetRequiredService<IPredictionService<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>>();
//var predictionService = services.GetRequiredService<IPredictionService<DeckCrackModelInput, DeckCrackModelOutput>>();
var predictionService2 = services.GetRequiredService<IPredictionService<ImageModelInput, ImageModelOutput>>();
//var predictionService3 = services.GetRequiredService<IPredictionService<MNISTModelInput, MNISTModelOutput>>();

//var predictionHandler =
//    (PredictionEnginePool<SentimentAnalysisModelInput, SentimentAnalysisModelOutput> predictionEnginePool, SentimentAnalysisModelInput input) =>
//    {
//        return predictionEnginePool.Predict(modelName: "SentimentAnalysisModel", input);
//    };

//app.MapPost("/predict", predictionHandler);

app.Run();
