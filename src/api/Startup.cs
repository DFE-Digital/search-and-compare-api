using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.SearchAndCompare.Api
{
    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            this.logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.logger.LogInformation("Configuring the services");
            var connectionString = new EnvConfigConnectionStringBuilder().GetConnectionString(Configuration);
            this.logger.LogInformation("Connnecting to "+connectionString);

            services.AddEntityFrameworkNpgsql().AddDbContext<CourseDbContext>(options => options
                .UseNpgsql(connectionString));
                
            services.AddMvc().AddJsonOptions(
            options => {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            }
            );;
            services.AddScoped<ICourseDbContext>(provider => provider.GetService<CourseDbContext>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, CourseDbContext dbContext)
        {
            this.logger.LogInformation("Configuring the application");
            this.logger.LogInformation("Seeding the schema");

            app.SeedSchema(dbContext);

            if (env.IsDevelopment())
            {
                this.logger.LogInformation("We're in DEVELOPMENT mode");

                app.UseDeveloperExceptionPage();
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = context => {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                });
            }
            else
            {
                this.logger.LogInformation("We're in a production mode");

                app.UseExceptionHandler("/Home/Error");
                app.UseStaticFiles();
            }

            app.UseMvc(routes => {});
        }
    }
}
