
using dataaccess;
using Microsoft.Extensions.DependencyInjection;

using PgCtxSetup;
using Xunit.Abstractions;


namespace UnitTests;

    public class GetDoctorById_ThrowsException_WhenNoDoctorExists_Test 
    {
        private readonly PgCtxSetup<HospitalContext> _pgCtxSetup;
        
        public GetDoctorById_ThrowsException_WhenNoDoctorExists_Test(
            ITestOutputHelper outputHelper)

        {
            _pgCtxSetup = new PgCtxSetup<HospitalContext>(configureServices: services =>
            {
                services.AddTransient<DoctorService>(serviceProvider =>
                {
                    var context = serviceProvider.GetRequiredService<HospitalContext>();
                    return new DoctorService(context);
                });
            });
   
        }

        [Fact]
        public void GetDoctorById_ThrowsException_WhenNoDoctorExists()
        {
            Assert.Throws<KeyNotFoundException>(() => _pgCtxSetup.ServiceProviderInstance
                .GetRequiredService<DoctorService>().GetDoctorById(-1));
        }
        
     

    }

    