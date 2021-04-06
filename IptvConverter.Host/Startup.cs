using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IptvConverter.Host.Swagger;
using IptvConverter.Host.Middleware;
using IptvConverter.Business;

namespace IptvConverter.Host
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
            services.AddControllers().AddNewtonsoftJson();

            services.AddService();

            services.AddSwagger();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            }

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api/swagger";
                c.SwaggerEndpoint("v1/swagger.json", "Flaster API");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path.Value.StartsWith("/api"))
                {
                    throw new EntryPointNotFoundException();
                }

                await next();
            });

            if (env.IsProduction())
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp/build";
                });
            }
        }
    }

}
