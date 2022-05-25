using Almostengr.VideoProcessor.Api.Configuration;
using Almostengr.VideoProcessor.Api.Database;
using Almostengr.VideoProcessor.Api.Repository;
using Almostengr.VideoProcessor.Api.Services.Data;
using Almostengr.VideoProcessor.Api.Services.ExternalProcess;
using Almostengr.VideoProcessor.Api.Services.MusicService;
using Almostengr.VideoProcessor.Api.Services.Subtitles;
using Almostengr.VideoProcessor.Api.Services.TextFile;
using Almostengr.VideoProcessor.Api.Services.VideoRender;
using Almostengr.VideoProcessor.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Almostengr.VideoProcessor.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AppSettings appSettings = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Almostengr.VideoProcessor.Api", Version = "v1" });
            });

            // DATABASE //////////////////////////////////////////////////////////////////////////////////////
            
            services.AddDbContext<VideoDbContext>(options => options.UseInMemoryDatabase("VideoProcessor"));

            // REPOSITORY ////////////////////////////////////////////////////////////////////////////////////

            services.AddTransient<IStatusRepository, StatusRepository>();

            // SERVICES //////////////////////////////////////////////////////////////////////////////////////

            services.AddTransient<IDashCamVideoRenderService, DashCamVideoRenderService>();
            services.AddTransient<IExternalProcessService, ExternalProcessService>();
            services.AddTransient<IMusicService, MusicService>();
            services.AddTransient<IRhtServicesVideoRenderService, RhtServicesVideoRenderService>();
            services.AddTransient<ITextFileService, TextFileService>();
            services.AddTransient<IStatusService, StatusService>();
            services.AddTransient<ISrtSubtitleService, SrtSubtitleService>();

            // WORKERS ///////////////////////////////////////////////////////////////////////////////////////

            services.AddHostedService<DashCamVideoRenderWorker>();
            services.AddHostedService<RhtServicesVideoRenderWorker>();
            services.AddHostedService<RhtServicesSubtitleWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Almostengr.VideoProcessor.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
