using DvizhX.Application.Common.Interfaces.Authentication;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using DvizhX.Infrastructure.Authentication;
using DvizhX.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using DvizhX.Infrastructure.Services;

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

            services.AddScoped<INotificationService, FirebaseNotificationService>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBoardRepository, BoardRepository>();
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();

            // Можно добавить generic регистрацию, если нужно инжектить IRepository<SomeEntity>
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            return services;
        }
    }
}
