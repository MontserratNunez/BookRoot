using Application.Interfaces;

namespace Persistence.Contexts
{
    public class SupabaseClientProvider : ISupabaseClientProvider
    {
        private readonly Supabase.Client _client;

        public SupabaseClientProvider(Supabase.Client client)
        {
            _client = client;
        }

        public Supabase.Client GetClient()
        {
            return _client;
        }
    }
}
