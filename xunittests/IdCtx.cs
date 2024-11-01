using dataaccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PgCtx;
using Xunit.Abstractions;

namespace xunittests;

public class User : IdentityUser
{
}

public class IdCtx : IdentityDbContext<User>
{
    // Add this constructor
    public IdCtx(DbContextOptions<IdCtx> options) : base(options)
    {
    }
}

public class Consumer(ITestOutputHelper output)
{
    public PgCtxSetup<HospitalContext> ctx = new();
    public PgCtxSetup<IdCtx> idCtx = new();

    [Fact]
    public void Test()
    {
        var hospitalScript = ctx.DbContextInstance.Database.GenerateCreateScript();
        var idScript = idCtx.DbContextInstance.Database.GenerateCreateScript();

       // output.WriteLine(hospitalScript);
        output.WriteLine(idScript);
    }
}