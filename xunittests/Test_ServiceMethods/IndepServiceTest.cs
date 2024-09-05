using dataaccess.Production_SerivceMethods;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class IndepServiceTest
{
    
    private IServiceProvider _serviceProvider;
    public IndepServiceTest()
    {
     
        var services = new ServiceCollection();
        services.AddTransient<IndepService>();
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Fact]
    public void MyTest()
    {
 var str =       _serviceProvider.GetRequiredService<IndepService>().GetDoctor();
 
 Assert.Equal("Doctor", str);
 
    }
}