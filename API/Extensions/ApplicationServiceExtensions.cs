using Application.BusinessLogic;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using DeepServices;
using MediatR;
using Domain.Image;
using Application.DataIngestion;
using ShallowServices;

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

            services.AddMvc(options => {
                options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter());
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));

            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            // According to the documentation, this should work. But it doesn't, so the workaround follows.
            //services.AddPredictionEnginePool<ImageModelInput, ImageModelOutput>().FromFile(modelName: "ImagePredictionModel", filePath: "path/to/model.zip", watchForChanges: true);

            services.AddSingleton<IPredictionService<ImageModelInput, ImageModelOutput>>(serviceProvider => new PredictionService<ImageModelInput, ImageModelOutput>("InitialModels/FlowersModelCopy.zip", 224, 224));

            // TODO: There should be a better way of doing this. Currently, this follows a quick and dirty solution found at
            // https://stackoverflow.com/questions/73760859/mediatr-generic-handlers
            services.AddTransient<IRequestHandler<IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command, Result<ImageModelOutput>>, IngestFileFromForm<ImageModelInput, ImageModelOutput>.Handler>();

            string path = Directory.GetCurrentDirectory();
            Console.WriteLine("The current directory is {0}", path);

            return services;
        }
    }
}
