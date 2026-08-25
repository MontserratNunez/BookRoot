using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Auth
{
    public class TokenResponseDto
    {
        public string CustomJwt { get; set; } = default!;
        public string SupabaseAccessToken { get; set; } = default!;
        public string SupabaseRefreshToken { get; set; } = default!;
    }
}
