using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MatBoxApi.Controllers;
using MatBoxApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MatBoxApi
{
    public class StartupOwin
    {
        public StartupOwin(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<AppDbContext>(options => 
                options.UseNpgsql(Configuration.GetConnectionString("MaterialsDatabase")));
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(pipeline =>
            {
                pipeline(next => OwinHello);
            });
        }

        private static Task OwinHello(IDictionary<string, object> environment)
        {
            var responseText = "Hello World via OWIN";
            var responseBytes = Encoding.UTF8.GetBytes(responseText);

            // OWIN Environment Keys: https://owin.org/spec/spec/owin-1.0.0.html
            var responseStream = (Stream)environment["owin.ResponseBody"];
            var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            responseHeaders["Content-Length"] = new string[] { responseBytes.Length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders["Content-Type"] = new string[] { "text/plain" };

            return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}