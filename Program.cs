using System.Net;
using Amazon.Runtime.SharedInterfaces;
using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;
using WebAdvert.Web.Settings;

namespace WebAdvert.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Configs
            builder.Services.Configure<AdvertApi>(
                builder.Configuration.GetSection(nameof(AdvertApi)));

            // Add services to the container.
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddTransient<IFileUploader, S3FileUploader>();
            builder.Services.AddCognitoIdentity(c =>
            {
                c.Password = new PasswordOptions()
                {
                    RequiredLength = 6,
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false,
                    RequiredUniqueChars = 0
                };
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = "/Accounts/Login";
                options.SlidingExpiration = true;
            });
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient<IAdvertApiClient, AdvertApiClient>()
                .AddPolicyHandler(GetBackOffRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

           
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(60))
               ;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetBackOffRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(5, retryAttempt =>
                {
                    Console.WriteLine();
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                });

        }
    }
}