using AutoMapper;
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
using LiveBolt.Services;
using Microsoft.Extensions.Logging;

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
        public void ConfigureServices(IServiceCollection services)
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
            services.AddScoped<IMqttService, MqttService>();
            services.AddScoped<IAPNSService, APNSService>();
            services.AddScoped<IMLService, MLService>();

            services.AddMvc();

            services.AddAutoMapper(typeof(Startup));

            /*services.AddIdentityServer()
                .AddDeveloperSigningCredential() // TODO: This should check if in production and utilize cert from machine certificate store
                .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
                .AddInMemoryClients(IdentityServerConfig.GetClients())
                .AddAspNetIdentity<ApplicationUser>();*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger<Startup> logger)
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

            // Setup MQTT subscriptions
            var mqttOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("LiveboltServer")
                    .WithTcpServer("localhost")
                    .WithCredentials("livebolt", "livebolt")
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();

            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("dlm/register").Build());
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("dlm/status").Build());
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("dlm/removeConfirm").Build());

            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("idm/register").Build());
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("idm/status").Build());
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("idm/removeConfirm").Build());

            mqttClient.ApplicationMessageReceived += (s, e) =>
            {
                var mqttService = serviceProvider.GetService<IMqttService>();

                var topic = e.ApplicationMessage.Topic;
                var values = Encoding.UTF8.GetString(e.ApplicationMessage.Payload).Split(",");

                if (!Guid.TryParse(values[0], out var guid))
                {
                    Console.WriteLine($"Could not parse GUID: {values[0]}");
                    return;
                }

                if (topic == "dlm/register" && values.Length == 4)
                {
                    mqttService.RegisterDLM(guid, values[1], values[2], values[3]);
                }
                else if (topic == "dlm/status" && values.Length == 2)
                {
                    if (!bool.TryParse(values[1], out var isLocked))
                    {
                        Console.WriteLine($"Could not parse dlm/status boolean: {values[1]}");
                        return;
                    }

                    Console.WriteLine("Calling UpdateDLMStatus");
                    mqttService.UpdateDLMStatus(guid, isLocked);
                }
                else if (topic == "idm/register" && values.Length == 4)
                {
                    mqttService.RegisterIDM(guid, values[1], values[2], values[3]);
                }
                else if (topic == "idm/status" && values.Length == 2)
                {
                    if (!bool.TryParse(values[1], out var isLocked))
                    {
                        Console.WriteLine($"Could not parse idm/status boolean: {values[1]}");
                        return;
                    }

                    mqttService.UpdateIDMStatus(guid, isLocked);
                }
                else if (topic == "idm/removeConfirm")
                {
                    Console.WriteLine($"Received idm/removeConfirm - {e.ApplicationMessage.Payload}");
                    mqttService.RemoveIDM(guid, values[1]);
                }
                else if (topic == "dlm/removeConfirm")
                {
                    Console.WriteLine($"Received dlm/removeConfirm - {e.ApplicationMessage.Payload}");
                    mqttService.RemoveDLM(guid, values[1]);
                }
                else
                {
                    Console.WriteLine($"Received unknown topic ({topic})");
                }


            };

            await mqttClient.StartAsync(mqttOptions);
        }
    }
}
