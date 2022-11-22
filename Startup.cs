using Backend.Data;
using Backend.Model;
using Backend.Services;
using JwtSharp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // configure Entity Framework with SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("DataSource=database.sqlite")
            );

            // add membership system for .NET
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // this is to create tokens
            var jwtIssuerOptions = new JwtIssuerOptions()
            {
                Audience = JwtConfiguration.Audience,
                Issuer = JwtConfiguration.Issuer,
                SecurityKey = JwtConfiguration.SigningKey,
                ExpireSeconds = JwtConfiguration.ExpireSeconds
            };
            var jwtIssuer = new JwtIssuer(jwtIssuerOptions);
            services.AddSingleton(jwtIssuer);

            // authentication configuration for .NET
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = JwtConfiguration.GetTokenValidationParameters();
                cfg.Events = new JwtBearerEvents
                {
                    // event for custom responses for not authenticated users
                    OnChallenge = async (context) =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.Headers.Append(
                            HeaderNames.WWWAuthenticate,
                            context.Options.Challenge);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Invalid token" }));
                    }
                };
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Sub] = ClaimTypes.Name;

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OnlyForAccountHolders", policy => policy.RequireClaim("role", "ACCOUNT_HOLDERS"));
                options.AddPolicy("OnlyForAuditors", policy => policy.RequireClaim("role", "AUDITORS"));
            });

            // add controllers and services to Dependency Injection Container
            services.AddControllers();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IAccountService, AccountService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ApplicationDbContext dbContext)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();

            SeedUsers(serviceProvider, dbContext).Wait();
        }

        private async Task SeedUsers(IServiceProvider serviceProvider, ApplicationDbContext dbContext)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var usersRole = new IdentityRole("ACCOUNT_HOLDERS");
            var auditorsRole = new IdentityRole("AUDITORS");
            await roleManager.CreateAsync(usersRole);
            await roleManager.CreateAsync(auditorsRole);

            var billyUser = new IdentityUser("Billy");
            await userManager.CreateAsync(billyUser, "test");
            await userManager.AddToRoleAsync(billyUser, "ACCOUNT_HOLDERS");

            var emilyUser = new IdentityUser("Emily");
            await userManager.CreateAsync(emilyUser, "pass");
            await userManager.AddToRoleAsync(emilyUser, "ACCOUNT_HOLDERS");

            var annaUser = new IdentityUser("Anna");
            await userManager.CreateAsync(annaUser, "12345");
            await userManager.AddToRoleAsync(annaUser, "ACCOUNT_HOLDERS");

            var theAuditor = new IdentityUser("John");
            await userManager.CreateAsync(theAuditor, "secret");
            await userManager.AddToRoleAsync(theAuditor, "AUDITORS");

            var billyUserAccount = new BankAccount
            {
                Id = Guid.Parse("BF861F2B-A238-4D37-8C4D-E634B47577F0"),
                UserId = billyUser.Id,
                UserName = "Billy",
                SSN = "123-45-6789",
                Balance = 5440.50M,
            };

            var emilyUserAccount = new BankAccount
            {
                Id = Guid.Parse("F63A109F-A7AF-44DC-8DCD-52FBA219C9D0"),
                UserId = emilyUser.Id,
                UserName = "Emily",
                SSN = "456-78-901",
                Balance = 15700.00M,
            };

            var annaUserAccount = new BankAccount
            {
                Id = Guid.Parse("92F70A00-FC13-4A57-863A-C30E3F397FA4"),
                UserId = annaUser.Id,
                UserName = "Anna",
                SSN = "368-56-975",
                Balance = 8700.00M,
            };

            dbContext.Accounts.Add(billyUserAccount);
            dbContext.Accounts.Add(emilyUserAccount);
            dbContext.Accounts.Add(annaUserAccount);
            await dbContext.SaveChangesAsync();
        }
    }
}