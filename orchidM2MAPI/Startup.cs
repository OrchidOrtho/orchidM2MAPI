using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Internal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.WebApi;
using orchidM2MAPI.DataProviders;
using Polly;
using Polly.Registry;
using Refit;
using static orchidM2MAPI.Features;

namespace orchidM2MAPI
{

    public class Startup
    {

        private IPolicyRegistry<string> policyRegistry;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                if (appAssembly != null)
                {
                    builder.AddUserSecrets(appAssembly, optional: true);
                }
            }

            Configuration = builder.Build();

            if (env.IsProduction())
            {
                builder.AddAzureKeyVault(Configuration["KeyVaultName"]);
                Configuration = builder.Build();
            }
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ConfigureAuth(services);

            services.AddMvc()
                .AddXmlSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(provider => Configuration);

            ConfigureDataProviders(services);
            ConfigurePolicies(services);
            ConfigureFeatures(services);
            ConfigureHealth(services);
            ConfigureOpenApi(services);
            ConfigureApiOptions(services);
            ConfigureVersioning(services);
            ConfigureApplicationInsights(services);
            ConfigureHSTS(services);

        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });
        }

        private void ConfigureDataProviders(IServiceCollection services)
        {
            services.AddTransient<IJobDataProvider, JobDataProvider>();
            services.AddTransient<IPurchaseOrderDataProvider, PurchaseOrderDataProvider>();
            services.AddTransient<IPartDataProvider, PartDataProvider>();
            services.AddTransient<IShippingInfoDataProvider, ShippingInfoDataProvider>();
            services.AddTransient<IShippingLotInfoDataProvider, ShippingLotInfoDataProvider>();
            services.AddTransient<IReceivingDataProvider, ReceivingDataProvider>();
        }

        private void ConfigureHSTS(IServiceCollection services)
        {
            services.AddHsts(
                options =>
                {
                    options.MaxAge = TimeSpan.FromDays(100);
                    options.IncludeSubDomains = true;
                    options.Preload = true;
                });
        }

        private void ConfigureFeatures(IServiceCollection services)
        {
            var provider = new AppSettingsProvider { Configuration = Configuration };
            services.AddSingleton(new AdvancedHealthFeature { ToggleValueProvider = provider });
        }

        private void ConfigureApplicationInsights(IServiceCollection services)
        {
            IHostingEnvironment env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();
            services.AddApplicationInsightsTelemetry(options => {
                options.DeveloperMode = env.IsDevelopment();
                options.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
            });
        }

        private void ConfigureVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                // Includes headers "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;
            });

        }

        private void ConfigurePolicies(IServiceCollection services)
        {
            policyRegistry = services.AddPolicyRegistry();
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));
            policyRegistry.Add("timeout", timeoutPolicy);
        }

        private void ConfigureApiOptions(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://asp.net/core",
                        Detail = "Please refer to the errors property for additional details."
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });
        }

        private void ConfigureOpenApi(IServiceCollection services)
        {
            services.AddSwaggerDocument();
        }

        private void ConfigureHealth(IServiceCollection services)
        {
            //services.AddHealthChecks(checks =>
            //{
            //    // Use feature toggle to add this functionality
            //    var feature = services.BuildServiceProvider().GetRequiredService<AdvancedHealthFeature>();
            //    if (feature.FeatureEnabled)
            //    {
            //        checks.AddHealthCheckGroup(
            //            "memory",
            //            group => group
            //                .AddPrivateMemorySizeCheck(200000000) // Maximum private memory
            //                .AddVirtualMemorySizeCheck(3000000000000)
            //                .AddWorkingSetCheck(200000000),
            //            CheckStatus.Unhealthy
            //        );
            //    }
            //});
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Information);
            loggerFactory.AddEventSourceLogger(); // ETW on Windows, dev/null on other platforms
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Do not expose Swagger interface in production
                //app.UseSwaggerUi3(typeof(Startup).GetTypeInfo().Assembly, settings =>
                //{
                //    settings.DocumentPath = "/swagger/v2/swagger.json";
                //    settings.EnableTryItOut = true;
                //    settings.DocExpansion = "list";
                //    settings.PostProcess = document =>
                //    {
                //        document.BasePath = "/";
                //    };
                //    settings.GeneratorSettings.Description = "Building Web APIs Workshop Demo Web API";
                //    settings.GeneratorSettings.Title = "Genealogy API";
                //    settings.GeneratorSettings.Version = "2.0";
                //    settings.GeneratorSettings.OperationProcessors.Add(
                //        new ApiVersionProcessor() { IncludedVersions = new[] { "2.0" } }
                //    );
                //});


                app.UseSwaggerUi3(typeof(Startup).GetTypeInfo().Assembly, settings =>
                {
                    settings.DocumentPath = "/swagger/v1/swagger.json";
                    settings.EnableTryItOut = true;
                    settings.DocExpansion = "list";
                    settings.PostProcess = document =>
                    {
                        document.BasePath = "/";
                    };
                    settings.GeneratorSettings.Description = "Orchid ERP API";
                    settings.GeneratorSettings.Title = "Orchid ERP API";
                    settings.GeneratorSettings.Version = "1.0";
                    settings.GeneratorSettings.OperationProcessors.Add(
                        new ApiVersionProcessor() { IncludedVersions = new[] { "1.0" } }
                    );
                });
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
