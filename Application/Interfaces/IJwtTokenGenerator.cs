using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string Generate(string userId, string email, string username);
    }
}
