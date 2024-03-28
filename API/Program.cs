using API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.WebHost.ConfigureKestrel(options =>
{
    // Retrieve certificate paths from appsettings.json
    var config = builder.Configuration;
    var certPath = config["CertificatePaths:CertPath"];
    var keyPath = config["CertificatePaths:KeyPath"];

    var certPem = File.ReadAllText(certPath);
    var keyPem = File.ReadAllText(keyPath);

    // workaround: https://stackoverflow.com/questions/57739563/create-x509certificate2-from-crt-and-key-files-using-asp-net-core-3-0-built-in/75727256#75727256
    var CreateCertFromPem = (string certString, string keyString) =>
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return X509Certificate2.CreateFromPem(certString, keyString);

        //workaround for windows issue https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
        //using var cert = X509Certificate2.CreateFromPemFile(certString, keyString);
        using var cert = X509Certificate2.CreateFromPem(certString, keyString);
        return new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
    };

    // workaround: https://stackoverflow.com/questions/57739563/create-x509certificate2-from-crt-and-key-files-using-asp-net-core-3-0-built-in/75727256#75727256
    var CreateCertFromPemFile = (string certFile, string keyFile) =>
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return X509Certificate2.CreateFromPemFile(certFile, keyFile);

        //workaround for windows issue https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
        //using var cert = X509Certificate2.CreateFromPemFile(certString, keyString);
        using var cert = X509Certificate2.CreateFromPem(certFile, keyFile);
        return new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
    };

    var x509 = CreateCertFromPemFile(certPem, keyPem);

    //var x509 = X509Certificate2.CreateFromPem(certPath, keyPath);

    options.ListenAnyIP(8765, listenOptions =>
    {
        listenOptions.UseHttps(x509);
    });
});

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
