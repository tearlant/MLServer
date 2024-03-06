using Application.BusinessLogic;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using MediatR;
using Domain.Image;
using Application.DataIngestion;
using ShallowServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DeepServices;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // Register IHttpContextAccessor
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen();

            // For prototyping, this just uses a SQLite database
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            // This is no longer used after CSV inputs were descoped, but it is still useful while debugging
            services.AddMvc(options => {
                options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter());
            });

            var acceptableOrigins = new List<string> {
                "http://localhost:3000",
                "https://localhost:3000",
                "https://localhost:443",
                "http://localhost:8080",
                "https://localhost:8080",
                "http://localhost",
                "https://localhost"
            };

            services.AddCors(opt =>
            {
                opt.AddPolicy("ReactCorsPolicy", builder => {
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(origin => acceptableOrigins.Contains(origin))
                        .AllowCredentials(); // Allow credentials if needed
                });
            });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure the cookie is sent over HTTPS
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            services.AddSingleton<IPredictionService<ImageModelInput, ImageModelOutput>, PredictionService<ImageModelInput, ImageModelOutput>>();

            // TODO: There should be a better way of doing this. Currently, this follows a quick and dirty solution found at
            // https://stackoverflow.com/questions/73760859/mediatr-generic-handlers
            services.AddTransient<IRequestHandler<IngestFileFromForm<ImageModelInput, ImageModelOutput>.Command, Result<ImageModelOutput>>, IngestFileFromForm<ImageModelInput, ImageModelOutput>.Handler>();

            return services;
        }
    }
}
