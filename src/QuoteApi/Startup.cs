using Microsoft.EntityFrameworkCore;
using QuoteApi.Data;

namespace QuoteApi
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

            /* string connectionString = Configuration.GetConnectionString("QuoteDb");
                 services.AddDbContext<QuoteContext>(options => options.UseSqlite(connectionString));
            */
            services.AddDbContext<QuoteContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));


            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder => builder.AllowAnyOrigin());
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            using (var serviceScope =
            app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())

            {
                var context = serviceScope.ServiceProvider.GetRequiredService<QuoteContext>();
                context.Database.Migrate();
            }
            app.UseCors("AllowOrigin");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}