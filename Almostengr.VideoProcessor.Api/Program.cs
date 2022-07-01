using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Database;
using Almostengr.VideoProcessor.Core.Repository;
using Almostengr.VideoProcessor.Core.Services.Data;
using Almostengr.VideoProcessor.Core.Services.FileSystem;
using Almostengr.VideoProcessor.Core.Services.MusicService;
using Almostengr.VideoProcessor.Core.Services.Subtitles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string environment = string.Empty;

#if RELEASE
    environment = AppEnvironment.Prod;
#else
    environment = AppEnvironment.Devl;
#endif

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile(
        (environment == AppEnvironment.Prod) ? AppConstants.AppSettingsProdFile : AppConstants.AppSettingsDevlFile, 
        false, 
        true)
    .Build();

AppSettings appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();

builder.Services.AddSingleton(appSettings);
        
builder.Services.AddDbContext<IVideoDbContext, VideoDbContext>();

builder.Services.AddTransient<IStatusRepository, StatusRepository>();

builder.Services.AddTransient<IFileSystemService, FileSystemService>();
builder.Services.AddTransient<IMusicService, MusicService>();
builder.Services.AddTransient<ISrtSubtitleService, SrtSubtitleService>();
builder.Services.AddTransient<IStatusService, StatusService>();

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

app.Run();
