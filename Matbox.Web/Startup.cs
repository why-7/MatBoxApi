using System.Net;
using System.Threading.Tasks;
using Matbox.DAL.Models;
using Matbox.Web.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Matbox.Web
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
            services.AddDbContext<MaterialsDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Database"),
                    x => x.MigrationsAssembly("Matbox.DAL")));
            
            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Database"),
                    x => x.MigrationsAssembly("Matbox.DAL")));
            
            services.AddSwaggerGen();
            
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<UsersDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = c =>
                {
                    var exception = c.Features.Get<IExceptionHandlerFeature>();
                    var statusCode = exception.Error.GetType().Name switch
                    {
                        "MaterialAlreadyInDbException" => HttpStatusCode.BadRequest,
                        "WrongCategoryException" => HttpStatusCode.BadRequest,
                        "WrongMaterialSizeException" => HttpStatusCode.BadRequest,
                        "WrongMaterialVersionException" => HttpStatusCode.BadRequest,
                        "MaterialNotInDbException" => HttpStatusCode.NotFound,
                        _ => HttpStatusCode.ServiceUnavailable
                    };
                    c.Response.StatusCode = (int) statusCode;
                    
                    return Task.CompletedTask;
                }
            });
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}