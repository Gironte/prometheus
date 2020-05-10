using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using App.Metrics;
using App.Metrics.Counter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Avia.Services;
using AviaTicketsFinder.Models;

namespace Avia
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMetrics(Program.Metrics);

            services.Configure<CookiePolicyOptions>(option =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                option.CheckConsentNeeded = context => true;
                option.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var options = Configuration.GetSection("Options").Get<Settings>();
            services.AddHttpClient<IHttpClientWrapper, HttpClientWrapper>(client =>
            {
                client.DefaultRequestHeaders.Add("X-Access-Token", options.Token);
            });

            services.Configure<Settings>(Configuration.GetSection("Options"));
            services.AddTransient<ITicketsService, AviaSalesService>();
            services.AddHostedService<Worker>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                AutoDiscoverRoutes(context);
                // Do logging or other work that doesn't write to the Response.
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void AutoDiscoverRoutes(HttpContext context)
        {
            if (context.Request.Path.Value == "/favicon.ico")
                return;

            List<string> keys = new List<string>();
            List<string> vals = new List<string>();

            var routeData = context.GetRouteData();
            if (routeData != null)
            {
                keys.AddRange(routeData.Values.Keys);
                vals.AddRange(routeData.Values.Values.Select(p => p.ToString()));
            }

            keys.Add("method"); vals.Add(context.Request.Method);
            keys.Add("response"); vals.Add(context.Response.StatusCode.ToString());
            keys.Add("url"); vals.Add(context.Request.Path.Value);

            Program.Metrics.Measure.Counter.Increment(new CounterOptions
            {
                Name = "api",
                //ResetOnReporting = true, // обнулять, если коллетор собрал данные
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags(keys.ToArray(), vals.ToArray())
            });
        }
    }
}
