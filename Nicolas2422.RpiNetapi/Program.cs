using Nicolas2422.RpiNetapi.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISystemInformation>(provider =>
{
#if DEBUG
    return new DebugSystemInformation();
#else
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return new DebugSystemInformation();
    }
    else
    {
        return new RaspbianSystemInformation();
    }
#endif
});

// Build app
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
