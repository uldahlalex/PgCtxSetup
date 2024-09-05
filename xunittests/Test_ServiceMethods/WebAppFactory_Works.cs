
using dataaccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PgCtx;
using Xunit.Abstractions;


namespace UnitTests;

    public class WebAppFactoryWorks :  IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly PgCtxSetup<HospitalContext> _pgCtxSetup;
        private readonly HttpClient _client;
        
        public WebAppFactoryWorks(
            ITestOutputHelper outputHelper,
            WebApplicationFactory<Startup> factory)

        {
            _client = factory.CreateClient();
            _pgCtxSetup = new PgCtxSetup<HospitalContext>();
 
        }
        
        
            [Fact]
            public async Task CanGetHelloWorldWhenUsingHttpClient()
            {
                var response = await _client.GetAsync("/"); 
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.Equal("Hello World!", responseString);
                Assert.Equal(200, (int)response.StatusCode);
            }

    }

    