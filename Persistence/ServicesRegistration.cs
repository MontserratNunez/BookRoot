using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;
using Supabase;

namespace Persistence
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Supabase Config
            var url = config["SUPABASE_URL"];
            var key = config["SUPABASE_KEY"];

            var options = new SupabaseOptions
            {
                AutoRefreshToken = false,
                AutoConnectRealtime = false
            };


            services.AddSingleton<Supabase.Client>(provider =>
            {
                var client = new Supabase.Client(url, key, options);

                client.InitializeAsync().GetAwaiter().GetResult();

                return client;
            });

            #endregion

            #region Repositories IOC
            services.AddSingleton<ISupabaseClientProvider, SupabaseClientProvider>();
            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<IInteractionRepository, InteractionRepository>();
            services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IFollowRepository, FollowRepository>();
            services.AddTransient<IBookListRepository, BookListRepository>();
            services.AddTransient<IAchievementRepository, AchievementRepository>();
            #endregion
        }
    }
}
