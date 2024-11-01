### News:

#### PgCtxSetup now also works for context classes extending IdentityDbContext<T>!

Example usage with or without service instantiation in service collection:

```csharp
public class GetAllDoctorsTest
{
    private readonly PgCtxSetup<HospitalContext> _setup = new();
    
    [Fact]
    public void GetAllDoctors_ReturnsAllDoctors()
    {
        var doctors = new List<Doctor>
        {
            Constants.GetDoctor(), //This is a method that returns a doctor object
            Constants.GetDoctor() //This is a method that returns a doctor object
        };
        _setup.DbContextInstance.Doctors.AddRange(doctors);
        _setup.DbContextInstance.SaveChanges();

        var result = new HospitalRepository(_setup.DbContextInstance).GetAllDoctors();

        Assert.Equivalent(doctors, result);
    }
}


public class GetAllDoctorsTestWithServiceCollection
{
    private readonly PgCtxSetup<HospitalContext> _setup = new(configureServices: 
        services =>
        {
            services.AddTransient<HospitalRepository>();
        });
    [Fact]
    public void GetAllDoctors_ReturnsAllDoctors()
    {
        var doctors = new List<Doctor>
        {
            Constants.GetDoctor(), //This is a method that returns a doctor object
            Constants.GetDoctor() //This is a method that returns a doctor object
        };
        _setup.DbContextInstance.Doctors.AddRange(doctors);
        _setup.DbContextInstance.SaveChanges();

        var result = _setup.ServiceProviderInstance.GetRequiredService<HospitalRepository>().GetAllDoctors();

        Assert.Equivalent(doctors, result);
    }
}

```
