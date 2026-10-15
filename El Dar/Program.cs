using ElDarBLL.BusinessServices.Implemintation;
using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.Helpers;
using ElDarBLL.Helpers.EmailService;
using ElDarBLL.Mapping;
using ElDarDal.Context;
using ElDarDal.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Supabase;

namespace El_Dar
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var isElectron = args.Contains("--electron") || Environment.GetEnvironmentVariable("ELECTRON_RUN") == "true";
            var isProduction = builder.Environment.IsProduction();
            var isDemo = builder.Configuration.GetValue<bool>("Demo:Enabled");

            if (isElectron)
            {
                var appFolder = isDemo ? "El-Dar-Demo" : "El-Dar";
                var dbFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    appFolder, "data", "ElDar.db"
                );

                var dbDirectory = Path.GetDirectoryName(dbFilePath);
                if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                    Directory.CreateDirectory(dbDirectory);

                builder.Services.AddDbContext<ElDarDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={dbFilePath}");
                });
            }
            else if (isProduction && !isDemo)
            {
                var monsterConnectionString = builder.Configuration.GetConnectionString("MonsterConnection");
                builder.Services.AddDbContext<ElDarDbContext>(options =>
                {
                    options.UseSqlServer(monsterConnectionString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                    });
                });
            }
            else
            {
                builder.Services.AddDbContext<ElDarDbContext>(options =>
                {
                    options.UseSqlite("Data Source=ElDar.db");
                });
            }

            if (!isDemo)
            {
                builder.Services.AddHostedService<DatabaseSyncWorker>();
            }

            var supabaseUrl = builder.Configuration["Supabase:Url"];
            var supabaseKey = builder.Configuration["Supabase:AnonKey"];

            if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseKey))
            {
                if (!isDemo)
                    throw new InvalidOperationException("Supabase:Url and Supabase:AnonKey must be configured.");

                supabaseUrl = "https://demo.invalid";
                supabaseKey = "demo";
            }

            builder.Services.AddSingleton(provider =>
                new Supabase.Client(
                    supabaseUrl,
                    supabaseKey,
                    new SupabaseOptions { AutoConnectRealtime = !isDemo }
                ));

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IGuestService, GuestService>();
            builder.Services.AddScoped<IDataService, DataService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddAutoMapper(x => x.AddProfile(new MappingProfile()));

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "ElDar_Auth_Cookie";
                options.LoginPath = "/Login/Login";
                options.AccessDeniedPath = "/Login/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<ElDarDbContext>();
                    dbContext.Database.EnsureCreated();
                    await DataBaseSeed.SeedDataAsync(services);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Database Initialization Error]: {ex.Message}");
                }
            }

            app.UseDeveloperExceptionPage();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpMethodOverride();
            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Home}")
                .WithStaticAssets();

            app.Run();
        }
    }
}