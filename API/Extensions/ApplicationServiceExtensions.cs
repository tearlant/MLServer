using Application.BusinessLogic;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using DeepServices;
using Application.MLOperations;
using MediatR;
using Domain.SentimentAnalysis;
using Domain.MNIST;
using Microsoft.ML;
using Domain.DeckCrack;
using Domain.Image;
using Application.DataIngestion;

namespace API.Extensions
{

    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    // React
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000");
                });
            });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));

            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            // According to the documentation, this should work. But it doesn't, so the workaround follows.
            //services.AddPredictionEnginePool<MNISTModelInput, MNISTModelOutput>().FromFile(modelName: "PretrainedMNISTModel", filePath: "InitialModels/MNIST.zip", watchForChanges: true);
            //services.AddPredictionEnginePool<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>().FromFile(modelName: "SentimentAnalysisModel", filePath: "InitialModels/SentimentModel.zip", watchForChanges: true);

            services.AddSingleton<IPredictionService<ImageModelInput, ImageModelOutput>>(serviceProvider => new PredictionService<ImageModelInput, ImageModelOutput>("InitialModels/FlowersModel.zip", 224, 224));

            //services.AddSingleton<IPredictionService<MNISTModelInput, MNISTModelOutput>>(serviceProvider => new PredictionService<MNISTModelInput, MNISTModelOutput>("InitialModels/DeckCrackModel.zip", 256, 256));
            //services.AddSingleton<IPredictionService<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>>(serviceProvider => new PredictionService<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>("InitialModels/SentimentModel.zip"));
            //services.AddSingleton<IPredictionService<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>>();

            // TODO: There should be a better way of doing this. Currently, this follows a quick and dirty solution found at
            // https://stackoverflow.com/questions/73760859/mediatr-generic-handlers
            //services.AddTransient<IRequestHandler<Predict<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>.Command, SentimentAnalysisModelOutput>, Predict<SentimentAnalysisModelInput, SentimentAnalysisModelOutput>.Handler>();
            //services.AddTransient<IRequestHandler<Predict<MNISTModelInput, MNISTModelOutput>.Command, MNISTModelOutput>, Predict<MNISTModelInput, MNISTModelOutput>.Handler>();
            //services.AddTransient<IRequestHandler<AddRawImage<MNISTModelInput>.Command, Result<MNISTModelInput>>, AddRawImage<MNISTModelInput>.Handler>();
            //services.AddTransient<IRequestHandler<Predict<DeckCrackModelInput, DeckCrackModelOutput>.Command, DeckCrackModelOutput>, Predict<DeckCrackModelInput, DeckCrackModelOutput>.Handler>();
            //services.AddTransient<IRequestHandler<AddRawImage<DeckCrackModelInput>.Command, Result<DeckCrackModelInput>>, AddRawImage<DeckCrackModelInput>.Handler>();
            
            services.AddTransient<IRequestHandler<Predict<ImageModelInput, ImageModelOutput>.Command, ImageModelOutput>, Predict<ImageModelInput, ImageModelOutput>.Handler>();
            services.AddTransient<IRequestHandler<IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command, Result<ImageModelOutput>>, IngestFileFromForm<ImageModelInput, ImageModelOutput>.Handler>();

            string path = Directory.GetCurrentDirectory();
            Console.WriteLine("The current directory is {0}", path);

            return services;
        }
    }
}
