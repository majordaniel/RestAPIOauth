using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RestAPIwithAuth0.Business.Helpers.Abstracts;
using RestAPIwithAuth0.Business.Implementations;
using RestAPIwithAuth0.Business.Interfaces;
using RestAPIwithAuth0.Data.Configs;

namespace RestAPIwithAuth0.API
{

    public class Startup
    {
        //private string ContentRootPath;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                   builder
                   .WithOrigins(Configuration["FrontendOrigin"])
                   .WithHeaders("Content-Type", "Authorization")
                   .AllowAnyMethod()
                   .AllowCredentials()
                );

                options.AddPolicy("UnsafeCORSAllow", builder =>
                   builder
                   .WithHeaders("Content-Type", "Authorization")
                   .AllowAnyMethod()
                   .AllowAnyOrigin()
                );
            });


            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                   options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(
                   Configuration["ConnectionStrings:DefaultConnection"]
               )
            );


            services.AddAppServices(Configuration);

            services.AddAutoMapper(typeof(AutomapperProfile));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTAPI", Version = "v1" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("your-auth-url", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                { "readAccess", "Access read operations" },
                                { "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });

                c.OperationFilter<OAuth2OperationFilter>();

                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestAPIService");
                c.OAuthClientId("cleint-id");
                c.OAuthClientSecret("client-secret");
                c.OAuthRealm("client-realm");
                c.OAuthAppName("OAuth-app");


                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
