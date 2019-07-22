using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationSample
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
            // Configurate Database Context for dependency injection.
            services.AddDbContext<Persistence.UserContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // Add SignalR Services.
            services.AddSignalR();

            // Configure our Identity Services.
            this.ConfigureIdentity(services);

            // Configure MVC for .Net Core 3
            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Configure Authz
            this.ConfigureAuthentication(services);
        }

        /// <summary>
        /// Configures Identity Services and Contexts
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<Persistence.UserContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });
        }

        public void ConfigureAuthentication(IServiceCollection services)
        {
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Get signing key.
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtAppSettingOptions["SecretKey"]));

            // Add and Configure Authentication Scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/chathub")))
                        {
                            Console.WriteLine("Added access token to context.");
                            Console.WriteLine(accessToken);
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("SigningKey Id {0}", _signingKey.KeyId);
                        Console.WriteLine("SigningKey {0}", Encoding.ASCII.GetString(_signingKey.Key));
                        Console.WriteLine("Audience {0}", context.Options.Audience);
                        Console.WriteLine("Issuer {0}", context.Options.ClaimsIssuer);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("SigningKey Id {0}", _signingKey.KeyId);
                        Console.WriteLine("SigningKey {0}", Encoding.ASCII.GetString(_signingKey.Key));
                        Console.WriteLine("Expected Audience {0}", jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)]);
                        Console.WriteLine("Audience {0}", context.Options.Audience);
                        Console.WriteLine("Expected Issuer {0}", jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)]);
                        Console.WriteLine("Issuer {0}", context.Options.ClaimsIssuer);
                        return Task.CompletedTask;
                    }
                };
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],
                    ValidateIssuer = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions["SecretKey"])),
                    ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],
                    ValidateAudience = false
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Enable Authentication First
            app.UseAuthentication();
            // Enable Authorization Second
            app.UseAuthorization();
            // Set CORS Policy, allowing all for development only.
            app.UseCors(options =>
            {
                List<string> origins = new List<string>();
                origins.Add("http://localhost:4200");
                options.WithOrigins(origins.ToArray());
                options.AllowAnyMethod();
                options.AllowAnyHeader();
                options.AllowCredentials();
            });
            // Force WebSockets Only because its faster
            app.UseWebSockets();
            // Enable SignalR Routing
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<Hubs.ChatHub>("/chatHub");
            });
            // Force SSL
            app.UseHttpsRedirection();
            
            app.UseMvc();
        }
    }
}
