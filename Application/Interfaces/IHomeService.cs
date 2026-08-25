using Application.Common.Result;
using Application.Dtos.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IHomeService
    {
        Task<Result<HomeDataDto>> GetHomeDashboardData();
    }
}
