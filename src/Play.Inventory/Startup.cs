using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Client;
using Play.Inventory.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory
{
    public class Startup
    {
        private const string AllowedOriginSetting = "AllowedOrigin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo();
            services.AddMongoRepository<InventoryItem>("inventoryItems");
            services.AddMongoRepository<CatalogItem>("catalogItems");
            services.AddMassTransitWithRabbitMq();
            
            // Move the Synchronous communication code for seperate method
            AddCatalogClient(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory", Version = "v1" });
            });
        } 

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory v1"));

                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedOriginSetting])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        private static void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random();

            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
            })
                // Configure a retry policy:
                // - Retry up to 5 times
                // - Use exponential backoff: 2^retryAttempt seconds between each retry
                // - Also retry on timeout exceptions (TimeoutRejectedException)
                // - Log each retry attempt with the delay duration
                .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                                                   + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                    onRetry: (outcome, timespan, retryAttempt) =>
                    {
                        // Build a service provider to resolve the logger service
                        var serviceProvider = services.BuildServiceProvider();
                        // Log the retry attempt and delay
                        serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                    }
                ))
                // Sets up a circuit breaker policy:
                // - Breaks the circuit after 3 consecutive failures
                // - Keeps it open for 20 seconds before attempting to reset
                .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                    3,
                    TimeSpan.FromSeconds(20),
                    // This action runs when the circuit is broken
                    onBreak: (outcome, timespan) =>
                    {
                        var serviceProvider = services.BuildServiceProvider();
                        serviceProvider.GetService<ILogger<CatalogClient>>()?
                            .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
                    },
                    // This action runs when the circuit is reset (closed)
                    onReset: () =>
                    {
                        var serviceProvider = services.BuildServiceProvider();
                        serviceProvider.GetService<ILogger<CatalogClient>>()?
                            .LogWarning($"Closing the circuit...");
                    }
                ))
                // Apply a timeout policy: if a request takes longer than 2 seconds, it will fail
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(2));
        }
    }
}
