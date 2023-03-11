using BasketAPI.Middleware;
using BasketAPI.Models;
using BasketAPI.Services;
using CodeChallengeApiClientNamespace;
using Nelibur.ObjectMapper;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromXml("nlog.config").GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    //TinyMapper bindings
    TinyMapper.Bind<BasketRequest, Basket>();
    TinyMapper.Bind<BasketLineRequest, BasketLine>();
    TinyMapper.Bind<BasketLine, OrderLine>();

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICodeChallengeAPIService, CodeChallengeAPIService>();
    builder.Services.AddSingleton<IBasketService, BasketService>();
    builder.Services.AddSingleton<IStorageService, MemoryCacheStorageService>();

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();
    //var codeChallengeAPIService = app.Services.GetService<ICodeChallengeAPIService>();
    //codeChallengeAPIService.GetAllProductsAsync();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ErrorHandlerMiddleware>();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}