﻿using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LiveBolt.Data;
using LiveBolt.Models;
using Microsoft.AspNetCore.HttpOverrides;
using MQTTnet.Core.ManagedClient;
using System;
using MQTTnet.Core.Client;
using MQTTnet;
using MQTTnet.Core;
using System.Text;

namespace LiveBolt
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public async void ConfigureServices(IServiceCollection services)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=livebolt.db"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=livebolt.db")); // TODO: Move this to a SQL Server instance for production
            }

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddScoped<IRepository, Repository>();

            services.AddMvc();

            services.AddAutoMapper(typeof(Startup));

            // Setup MQTT subscriptions
            var mqttOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("LiveboltServer")
                    .WithTcpServer("localhost", 1883)
                    .WithCredentials("livebolt", "livebolt")
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();

            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("dlm/register").Build());
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("dlm/status").Build());

            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("idm/register").Build());
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("idm/status").Build());

            mqttClient.ApplicationMessageReceived += (s, e) =>
            {
                Console.WriteLine("### Message Received ###");
                Console.WriteLine($"Topic: {e.ApplicationMessage.Topic}");
                Console.WriteLine($"Payload: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine();
            };

            await mqttClient.StartAsync(mqttOptions);

            /*services.AddIdentityServer()
                .AddDeveloperSigningCredential() // TODO: This should check if in production and utilize cert from machine certificate store
                .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
                .AddInMemoryClients(IdentityServerConfig.GetClients())
                .AddAspNetIdentity<ApplicationUser>();*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                await dbContext.Database.EnsureCreatedAsync();
            }
            else
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                await dbContext.Database.EnsureCreatedAsync(); // TODO: Once in production remove this
            }

            app.UseStaticFiles();

            app.UseAuthentication();
            /*app.UseIdentityServer();*/

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
