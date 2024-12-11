using Azure;

namespace Backoffice.Services
{
    public class TestService
    {
        private readonly HttpClient _client;

        public TestService(HttpClient client)
        {
            _client = client;
        }

        public Task<string> GetTest()
        {
            try
            {
                return _client.GetStringAsync("/test");
            }
            catch (Exception e)
            {
                return Task.FromResult<string>($"{e.Message}");
            }
        }
    }
}
