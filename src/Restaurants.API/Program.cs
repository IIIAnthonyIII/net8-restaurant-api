using Restaurants.API.Extensions;
using Restaurants.API.Middlewares;
using Restaurants.Application.Extensions;
using Restaurants.Domain.Entities;
using Restaurants.Infrastucture.Extensions;
using Restaurants.Infrastucture.Seeders;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Azure;

// Se aplica try catch para guardar en los logs algún error como cadena de conexión, etc
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.AddPresentation();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    //Los Seeders van después de que se construya la app
    var scope = app.Services.CreateScope();
    var seeders = scope.ServiceProvider.GetRequiredService<IRestaurantSeeder>();
    await seeders.Seed();

    // Configure the HTTP request pipeline.
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<RequestTimeLoggingMiddleware>();
    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    app.MapGroup("api/identity")
       .WithTags("Identity")   //Unir otra clase a las api de swagger
       .MapIdentityApi<User>();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
} catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
} finally
{
    Log.CloseAndFlush();
}


// Para referenciar a APITest
public partial class Program { }