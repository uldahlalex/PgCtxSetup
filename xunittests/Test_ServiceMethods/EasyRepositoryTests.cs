using System.Text.Json;
using dataaccess;
using DataAccess;
using dataaccess.Models;
using Microsoft.Extensions.DependencyInjection;
using PgCtx;
using UnitTests.Mocks;
using Xunit.Abstractions;

namespace UnitTests;

public class EasyRepositoryTests(ITestOutputHelper outputHelper)
{
    private readonly PgCtxSetup<HospitalContext> _setup = 
        new(configureServices: services => services.AddTransient<EasyRepository>());

    [Fact]
    public void GetAllDoctors_ReturnsAllDoctors()
    {
        var doctors = new List<Doctor>
        {
            Constants.GetDoctor(),
            Constants.GetDoctor()
        };
        _setup.DbContextInstance.Doctors.AddRange(doctors);
        _setup.DbContextInstance.SaveChanges();

        var result =_setup.ServiceProviderInstance.GetRequiredService<EasyRepository>().GetAllDoctors();

        Assert.Equivalent(doctors, result);
    }
}