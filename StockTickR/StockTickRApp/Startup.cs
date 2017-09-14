using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.StockTicker
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddScoped<StockTickerHub>();
            services.AddSingleton<StockTicker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseFileServer();

            app.UseSignalR(routes =>
            {
                routes.MapHub<StockTickerHub>("stocks");
            });
        }
    }
}