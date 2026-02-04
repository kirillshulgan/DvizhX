using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Infrastructure.Authentication;
using DvizhX.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DvizhX.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Auth services
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();
            services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Можно добавить generic регистрацию, если нужно инжектить IRepository<SomeEntity>
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            return services;
        }
    }
}
