using dataaccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PgCtx;
using Xunit;
using Xunit.Abstractions;

namespace xunittests;

public class User : IdentityUser
{
}

public class IdCtx : IdentityDbContext<User>
{
    public IdCtx(DbContextOptions<IdCtx> options) : base(options)
    {
    }
}

// Approach 1: Using IAsyncLifetime (Recommended)
public class ConsumerAsync : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private PgCtxSetup<HospitalContext> _hospitalCtx;
    private PgCtxSetup<IdCtx> _idCtx;

    public ConsumerAsync(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _hospitalCtx = new PgCtxSetup<HospitalContext>();
        _idCtx = new PgCtxSetup<IdCtx>();
    }

    public async Task DisposeAsync()
    {
        if (_hospitalCtx != null)
            await _hospitalCtx.TearDown();
        if (_idCtx != null)
            await _idCtx.TearDown();
    }

    [Fact]
    public async Task TestGenerateScripts()
    {
        var hospitalScript = _hospitalCtx.DbContextInstance.Database.GenerateCreateScript();
        var idScript = _idCtx.DbContextInstance.Database.GenerateCreateScript();

        _output.WriteLine("Hospital Context Script:");
        _output.WriteLine(hospitalScript);
        _output.WriteLine("\nIdentity Context Script:");
        _output.WriteLine(idScript);
        
        Assert.NotEmpty(hospitalScript);
        Assert.NotEmpty(idScript);
    }

    [Fact]
    public async Task TestDatabaseOperations()
    {
        // Test Hospital Context
        var hospitalContext = _hospitalCtx.DbContextInstance;
        Assert.NotNull(hospitalContext);
        Assert.True(await hospitalContext.Database.CanConnectAsync());

        // Test Identity Context
        var identityContext = _idCtx.DbContextInstance;
        Assert.NotNull(identityContext);
        Assert.True(await identityContext.Database.CanConnectAsync());
    }
}

// Approach 2: Using synchronous setup (Alternative)
public class ConsumerSync : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly PgCtxSetup<HospitalContext> _hospitalCtx;
    private readonly PgCtxSetup<IdCtx> _idCtx;

    public ConsumerSync(ITestOutputHelper output)
    {
        _output = output;
        _hospitalCtx = new PgCtxSetup<HospitalContext>();
        _idCtx = new PgCtxSetup<IdCtx>();
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(async () =>
        {
            if (_hospitalCtx != null)
                await _hospitalCtx.TearDown();
            if (_idCtx != null)
                await _idCtx.TearDown();
        });
    }

    [Fact]
    public void TestGenerateScripts()
    {
        var hospitalScript = _hospitalCtx.DbContextInstance.Database.GenerateCreateScript();
        var idScript = _idCtx.DbContextInstance.Database.GenerateCreateScript();

        _output.WriteLine("Hospital Context Script:");
        _output.WriteLine(hospitalScript);
        _output.WriteLine("\nIdentity Context Script:");
        _output.WriteLine(idScript);
        
        Assert.NotEmpty(hospitalScript);
        Assert.NotEmpty(idScript);
    }

    [Fact]
    public void TestDatabaseOperations()
    {
        // Test Hospital Context
        var hospitalContext = _hospitalCtx.DbContextInstance;
        Assert.NotNull(hospitalContext);
        Assert.True(AsyncHelper.RunSync(() => hospitalContext.Database.CanConnectAsync()));

        // Test Identity Context
        var identityContext = _idCtx.DbContextInstance;
        Assert.NotNull(identityContext);
        Assert.True(AsyncHelper.RunSync(() => identityContext.Database.CanConnectAsync()));
    }
}

// Additional test class to demonstrate specific Identity operations
public class IdentityTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private PgCtxSetup<IdCtx> _idCtx;

    public IdentityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _idCtx = new PgCtxSetup<IdCtx>();
    }

    public async Task DisposeAsync()
    {
        if (_idCtx != null)
            await _idCtx.TearDown();
    }

    [Fact]
    public async Task CanCreateUser()
    {
        var userManager = _idCtx.ServiceProviderInstance.GetRequiredService<UserManager<User>>();
        
        var user = new User
        {
            UserName = "testuser@example.com",
            Email = "testuser@example.com"
        };

        var result = await userManager.CreateAsync(user, "TestPassword123!");
        Assert.True(result.Succeeded);

        var foundUser = await userManager.FindByEmailAsync("testuser@example.com");
        Assert.NotNull(foundUser);
        Assert.Equal(user.Email, foundUser.Email);
    }
}