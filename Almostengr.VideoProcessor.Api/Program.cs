using Almostengr.VideoProcessor.Core.Database;
using Almostengr.VideoProcessor.Core.Repository;
using Almostengr.VideoProcessor.Core.Services.Data;
using Almostengr.VideoProcessor.Core.Services.MusicService;
using Almostengr.VideoProcessor.Core.Services.Subtitles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<IVideoDbContext, VideoDbContext>();

builder.Services.AddTransient<IStatusRepository, StatusRepository>();

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
