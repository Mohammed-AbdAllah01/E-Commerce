using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Project.Extentions
{
    public static class CustomJWTAuthExtention
    {
        public static void AddCustomJWTAuth(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    
                    ValidateIssuer = false,
                    ValidIssuer = configuration["JWT:Issuer"], // Something like "myapp.com"
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),


                };



            });
        }

        public static void AddSwaggerGenJWTAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "My API",
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Name = "Aston",
                        Email = "mo.alabd11298@gmail.com",
                        Url = new Uri("https://www.example.com")//my link 
                    }
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter The JWT Key",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",

                });


                options.AddSecurityRequirement(new OpenApiSecurityRequirement(){
                    {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Type =ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Name ="Bearer",
                        In = ParameterLocation.Header
                    },
                    new List <string>()
                    } });



            });
        }
    }
}
