using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Application.Interfaces;
using Application.Services;

namespace Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Services IOC
            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<ICurrentUserService, CurrentUserService>();
            services.AddTransient<IInteractionService, InteractionService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IFollowService, FollowService>();
            services.AddTransient<IJournalService, JournalService>();
            services.AddTransient<IHomeService, HomeService>();
            services.AddTransient<IListService, ListService>();
            services.AddTransient<IAchievementService, AchievementService>();
            services.AddTransient<IExportService, ExportService>();
            #endregion
        }
    }
}
