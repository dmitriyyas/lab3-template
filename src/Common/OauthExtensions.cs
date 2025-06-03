using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Common;

public static class OauthExtensions
{
    public static IServiceCollection AddOauth(this IServiceCollection builder)
    {
        builder.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = "Issuer";
            options.Audience = "Audience";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "Issuer",
                ValidateAudience = true,
                ValidAudience = "Audience",
                ValidateLifetime = true,
                IssuerSigningKeyResolver = (token, securityToken, keyIdentifier, parameters) =>
                {
                    var client = new HttpClient();
                    var jwksUri = "http://services-auth-srv:8090/.well-known/jwks.json";
                    var jwks = client.GetStringAsync(jwksUri).Result;
                    var keys = JsonWebKeySet.Create(jwks);

                    return keys.GetSigningKeys();
                }
            };
            options.RequireHttpsMetadata = false;
        });

        return builder;
    }

    public static IServiceCollection AddSwaggerGenWithJwt(this IServiceCollection builder)
    {
        builder.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "My API",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                    },
                    new string[] { }
                }
            });
        });

        return builder;
    }
}
