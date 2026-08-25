using Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFollowService
    {
        Task<Result> ToggleFollow(string targetUsername);
    }
}
