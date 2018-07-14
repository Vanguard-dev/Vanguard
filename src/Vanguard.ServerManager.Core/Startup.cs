using System.IdentityModel.Tokens.Jwt;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using Vanguard.ServerManager.Core.Entities;
using Vanguard.ServerManager.Core.Services;

namespace Vanguard.ServerManager.Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<VanguardDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("RedisConnection");
            });

            services.AddIdentity<VanguardUser, VanguardRole>(options =>
                {
                    options.Password = new PasswordOptions
                    {
                        RequireUppercase = false,
                        RequireLowercase = false,
                        RequireDigit = false,
                        RequireNonAlphanumeric = false,
                        RequiredLength = 5,
                        RequiredUniqueChars = 2
                    };
                })
                .AddEntityFrameworkStores<VanguardDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<VanguardDbContext>();
                })
                .AddServer(options =>
                {
                    options.UseMvc();

                    options.EnableTokenEndpoint("/connect/token")
                        .EnableLogoutEndpoint("/connect/logout");

                    options.AllowPasswordFlow()
                        .AllowRefreshTokenFlow();

                    options.RegisterScopes(
                        OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles
                    );

                    options.AcceptAnonymousClients();

                    if (HostingEnvironment.IsDevelopment())
                    {
                        options.DisableHttpsRequirement();
                        options.AddEphemeralSigningKey();
                    }
                    else
                    {
                        // Register a new ephemeral key, that is discarded when the application
                        // shuts down. Tokens signed using this key are automatically invalidated.
                        // This method should only be used during development.

                        // On production, using a X.509 certificate stored in the machine store is recommended.
                        // You can generate a self-signed certificate using Pluralsight's self-cert utility:
                        // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip
                        //
                        // options.AddSigningCertificate("7D2A741FE34CC2C7369237A5F2078988E17A6A75");
                        //
                        // Alternatively, you can also store the certificate as an embedded .pfx resource
                        // directly in this assembly or in a file published alongside this project:
                        //
                        // options.AddSigningCertificate(
                        //     assembly: typeof(Startup).GetTypeInfo().Assembly,
                        //     resource: "AuthorizationServer.Certificate.pfx",
                        //     password: "OpenIddict");
                    }
                })
                .AddValidation();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = OpenIddictValidationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIddictValidationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = OpenIddictValidationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = OpenIddictValidationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIddictValidationDefaults.AuthenticationScheme;
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddTransient<ServerNodeService>();
            services.AddSingleton<ServerNodeStatusService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:5055");
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}