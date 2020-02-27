using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dotnet_cloud_run_hello_world
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            WebEnvironment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            // Populate Cloud Run environment information: service, revision and Cloud Project
            //

            // Cloud Run Service
            string service;
            try
            {
                service = Environment.GetEnvironmentVariable("K_SERVICE");
            }
            catch (ArgumentNullException)
            {
                service = "???";
            }

            // Cloud Run Revision
            string revision;
            try
            {
                revision = Environment.GetEnvironmentVariable("K_REVISION");
            }
            catch (ArgumentNullException)
            {
                revision = "???";
            }
            
            // Cloud Run project
            string project = string.Empty;
            bool projectFound = false;
            if (WebEnvironment.IsDevelopment())
            {
                try
                {
                    project = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
                    projectFound = true;
                }
                catch (ArgumentNullException)
                {
                    project = "???";
                }
            }
            else 
            {
                if (string.IsNullOrEmpty(project) || service != "???")
                {
                    var http = new HttpClient();
                    http.DefaultRequestHeaders.Add("Metadata-Flavor", new[]{ "Google"});

                    try
                    {
                        project = http.GetStringAsync("http://metadata.google.internal/computeMetadata/v1/project/project-id").Result;
                        projectFound = true;
                    }
                    catch (Exception)
                    {
                        project = "???";
                    }
                }
            }

            var envInfo = new EnvironmentInfo(service, project, revision, projectFound);
            services.AddSingleton<IEnvironmentInfo>(envInfo);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
