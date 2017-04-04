using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pawgram.Data;
using Pawgram.Models;
using Pawgram.Services;
using Microsoft.AspNetCore.Mvc;
using Discord.OAuth2;

namespace Pawgram
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                options.SslPort = 44342;
                options.Filters.Add(new RequireHttpsAttribute());
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            app.UseTwitterAuthentication(new TwitterOptions()
            {
                ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"],
                ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"]
            });
            app.UseGoogleAuthentication(new GoogleOptions()
            {
                ClientId = Configuration["Authentication:Google:ClientId"],
                ClientSecret = Configuration["Authentication:Google:ClientSecret"]
            });
            app.UseTwitchAuthentication(new AspNet.Security.OAuth.Twitch.TwitchAuthenticationOptions()
            {
                ClientId = Configuration["Authentication:Twitch:ClientId"],
                ClientSecret = Configuration["Authentication:Twitch:ClientSecret"]
            });
            /**
            OAuthOptions DiscordOAuthOptions = new OAuthOptions()
            {
                AuthenticationScheme = "Discord",
                DisplayName = "Discord",
                ClaimsIssuer = "Discord",

                CallbackPath = new Microsoft.AspNetCore.Http.PathString("/signin-discord"),

                AuthorizationEndpoint = "https://discordapp.com/api/oauth2/authorize",
                TokenEndpoint = "https://discordapp.com/api/oauth2/token",
                UserInformationEndpoint = "https://discordapp.com/api/oauth2/authorize/users/",

                ClientId = Configuration["Authentication:Discord:ClientId"],
                ClientSecret = Configuration["Authentication:Discord:ClientSecret"]
            };
            DiscordOAuthOptions.Scope.Add("identify");
            DiscordOAuthOptions.Scope.Add("guilds");
            DiscordOAuthOptions.Scope.Add("email");
            app.UseOAuthAuthentication(DiscordOAuthOptions);
            **/
            app.UseDiscordAuthentication(new DiscordOptions
            {
                AppId = Configuration["Authentication:Discord:ClientId"],
                AppSecret = Configuration["Authentication:Discord:ClientSecret"],
                Scope = { "identify", "guilds" }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
