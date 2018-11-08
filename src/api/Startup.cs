using System.Net.Http;
using System.Reflection;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Middleware;
using GovUk.Education.SearchAndCompare.UI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration.Processors.Security;
using Serilog;

namespace GovUk.Education.SearchAndCompare.Api
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
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            var connectionString = new EnvConfigConnectionStringBuilder().GetConnectionString(Configuration);

            services.AddEntityFrameworkNpgsql().AddDbContext<CourseDbContext>(options => options
                .UseNpgsql(connectionString));

            services.AddMvc().AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
                }
            );
            services.AddScoped<ICourseDbContext>(provider => provider.GetService<CourseDbContext>());
            services.AddScoped(provider => new HttpClient());

            // No default auth method has been set here because each action must explicitly be decorated with
            // ApiTokenAuthAttribute.
            services.AddAuthentication()
                .AddBearerTokenApiKey(options =>
                {
                    options.ApiKey = Configuration["api:key"];
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, CourseDbContext dbContext)
        {
            //app.SeedSchema(dbContext);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = context =>
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStaticFiles();
                app.SetSecurityHeaders();
            }

            app.UseMvc(routes => { });

            // Enable the Swagger UI middleware and the Swagger generator
            app.UseSwaggerUi3(typeof(Startup).GetTypeInfo().Assembly, settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling =
                    PropertyNameHandling.CamelCase;

                settings.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Search API";
                    document.Info.Description = "An API for searching course data";
                };
                settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender(BearerTokenApiKeyDefaults.AuthenticationScheme, new SwaggerSecurityScheme
                {
                    Type = SwaggerSecuritySchemeType.ApiKey,
                    Description = "In order to interactive with the api please input `Bearer {code}`",
                    In = SwaggerSecurityApiKeyLocation.Header,
                    Name = "Authorization"
                }));

                settings.GeneratorSettings.OperationProcessors.Add(new OperationSecurityScopeProcessor(BearerTokenApiKeyDefaults.AuthenticationScheme));
            });

            // for reading ucas site we need 1252 available
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
    }
}
