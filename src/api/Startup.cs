using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Middleware;
using GovUk.Education.SearchAndCompare.UI.Middleware;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<Startup>();
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            var connectionString = new EnvConfigConnectionStringBuilder().GetConnectionString(Configuration);

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<CourseDbContext>(
                    options =>
                    {
                        const int maxRetryCount = 3;
                        const int maxRetryDelaySeconds = 5;

                        var postgresErrorCodesToConsiderTransient = new List<string>(); // ref: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/blob/16c8d07368cb92e10010b646098b562ecd5815d6/src/EFCore.PG/NpgsqlRetryingExecutionStrategy.cs#L99

                        // Note that the retry will only retry for TimeoutExceptions and transient postgres exceptions. ref: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/blob/8e97e4195b197ae3d16763704352acfffa95c73f/src/EFCore.PG/Storage/Internal/NpgsqlTransientExceptionDetector.cs#L12
                        options.UseNpgsql(connectionString,
                            b => b.MigrationsAssembly((typeof(CourseDbContext).Assembly).ToString())
                                .EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelaySeconds), postgresErrorCodesToConsiderTransient));
                    });

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
            Migrate(dbContext);

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

        /// <summary>
        /// Migrate with inifinte retry.
        /// </summary>
        /// <param name="dbContext"></param>
        private void Migrate(CourseDbContext dbContext)
        {
            // If the migration fails and throws then the app ends up in a broken state so don't let that happen.
            // If the migrations failed and the exception was swallowed then the code could make assumptions that result in corrupt data so don't let execution continue till this has worked.
            int migrationAttempt = 1;
            while (true)
            {
                try
                {
                    _logger.LogInformation($"Applying EF migrations. Attempt {migrationAttempt} of ∞");
                    dbContext.Database.Migrate();
                    _logger.LogInformation($"Applying EF migrations succeeded. Attempt {migrationAttempt} of ∞");
                    break; // success!
                }
                catch (Exception ex)
                {
                    const int maxDelayMs = 60 * 1000;
                    int delayMs = 1000 * migrationAttempt;
                    if (delayMs > maxDelayMs)
                    {
                        delayMs = maxDelayMs;
                    }
                    // exception included in message string because app insights isn't showing the messages and kudo log stream only shows the message string.
                    _logger.LogError($"Failed to apply EF migrations. Attempt {migrationAttempt} of ∞. Waiting for {delayMs}ms before trying again.\n{ex}", ex);
                    new TelemetryClient().TrackException(ex);
                    Thread.Sleep(delayMs);
                    migrationAttempt++;
                }
            }
        }
    }
}
