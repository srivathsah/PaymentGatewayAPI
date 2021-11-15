using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System.Reflection;

namespace WebServerHostingUtils
{
    public class WebServerStartup : HostingUtils.StartupBase
    {
        public override IHostBuilder BeforeHostBuild(IHostBuilder hostBuilder)
        {
            hostBuilder = base.BeforeHostBuild(hostBuilder);
            hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.Configure(app => ConfigureApp(app));
                webHostBuilder = BeforeWebHostBuild(webHostBuilder);
            });
            return hostBuilder;
        }

        public virtual IWebHostBuilder BeforeWebHostBuild(IWebHostBuilder webHostBuilder) => webHostBuilder;

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            var mvcBuilder = services.AddControllers();
            BeforeMvcBuild(mvcBuilder);
            mvcBuilder = mvcBuilder.AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });

            AddAuthentication(services);
            AddAuthorization(services);
            services.AddSwaggerDocument();
            services.AddCors(options =>
            {
                options.AddPolicy("api", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
        }

        public virtual void BeforeMvcBuild(IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddApplicationPart(Assembly.GetEntryAssembly());
        }

        public virtual void ConfigureApp(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("api");

            app.UseHttpsRedirection();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                AddEndPoints(endpoints);
            });
        }

        public virtual void AddAuthentication(IServiceCollection services) { }
        public virtual void AddAuthorization(IServiceCollection services) { }
        public virtual void AddEndPoints(IEndpointRouteBuilder endpointRouteBuilder) { }
    }
}
