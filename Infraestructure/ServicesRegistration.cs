using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure
{
    public static class ServicesRegistration
    {
        public static void AddInfraestructureLayerIoc(this IServiceCollection services)
        {
            #region Services IOC
            services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
            #endregion
        }
    }
}
