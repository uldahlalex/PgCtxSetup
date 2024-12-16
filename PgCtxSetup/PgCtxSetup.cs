using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace PgCtx
{
    public class PgCtxSetup<TContext> where TContext : DbContext
    {
        public TContext DbContextInstance { get; private set; }
        public IServiceProvider ServiceProviderInstance { get; }
        public PostgreSqlContainer _postgres { get; set; }
        private readonly string _databaseName;

        private static bool IsRunningInDevContainer()
        {
            return File.Exists("/.dockerenv") || 
                   File.Exists("/.containerenv") ||
                   Environment.GetEnvironmentVariable("REMOTE_CONTAINERS") != null;
        }

        private PostgreSqlContainer CreatePostgresContainer(string postgresImage, string databaseName)
        {
            var builder = new PostgreSqlBuilder()
                .WithImage(postgresImage)
                .WithDatabase(databaseName)
                .WithUsername("postgres")
                .WithPassword("postgres");

            if (IsRunningInDevContainer())
            {
                try
                {
                    return builder
                        .WithEnvironment("DOCKER_HOST", "unix:///var/run/docker.sock")
                        .WithEnvironment("TESTCONTAINERS_HOST_OVERRIDE", "host.docker.internal")
                        .WithPortBinding(5432, true)
                        .WithWaitStrategy(Wait.ForUnixContainer()
                            .UntilPortIsAvailable(5432))
                        .Build();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Warning: DevContainer configuration failed, falling back to default: {e.Message}");
                }
            }

            return builder
                .WithPortBinding(5432, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();
        }

        public PgCtxSetup(
            string postgresImage = "postgres:16-alpine",
            Action<DbContextOptionsBuilder> configureDbContext = null,
            Action<IServiceCollection> configureServices = null)
        {
            _databaseName = $"test_db_{Guid.NewGuid():N}";

            try
            {
                _postgres = CreatePostgresContainer(postgresImage, _databaseName);
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                AsyncHelper.RunSync(() => _postgres.StartAsync(cts.Token));
                
                // Wait a short moment to ensure the container is fully ready
                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to start Postgres container: {e.Message}");
                if (_postgres != null)
                {
                    Console.WriteLine($"Connection string was: {_postgres.GetConnectionString()}");
                }
                throw;
            }

            var configureDbContext1 = configureDbContext;
            configureDbContext1 ??= optionsBuilder =>
            {
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                optionsBuilder.UseNpgsql(_postgres.GetConnectionString());
            };

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            configureDbContext1.Invoke(optionsBuilder);

            try
            {
                // Create instance based on whether it's an IdentityDbContext or regular DbContext
                if (typeof(TContext).IsSubclassOf(typeof(IdentityDbContext)) ||
                    (typeof(TContext).IsGenericType && typeof(TContext).GetGenericTypeDefinition() == typeof(IdentityDbContext<>)))
                {
                    DbContextInstance = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options);
                }
                else
                {
                    DbContextInstance = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options);
                }

                AsyncHelper.RunSync(() => DbContextInstance.Database.EnsureCreatedAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to initialize database: {e.Message}");
                AsyncHelper.RunSync(() => _postgres.DisposeAsync().AsTask());
                throw;
            }

            var services = new ServiceCollection();
            
            // Configure Identity services if using IdentityDbContext
            if (typeof(TContext).IsSubclassOf(typeof(IdentityDbContext)) ||
                (typeof(TContext).IsGenericType && typeof(TContext).GetGenericTypeDefinition() == typeof(IdentityDbContext<>)))
            {
                services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<TContext>()
                    .AddDefaultTokenProviders();
            }

            services.AddSingleton(DbContextInstance ?? throw new InvalidOperationException("DbContextInstance is null"));
            configureServices?.Invoke(services);
            ServiceProviderInstance = services.BuildServiceProvider();
        }

        public async Task TearDown()
        {
            try
            {
                if (DbContextInstance != null)
                {
                    await DbContextInstance.Database.EnsureDeletedAsync();
                    await DbContextInstance.DisposeAsync();
                    DbContextInstance = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during database cleanup: {e.Message}");
            }

            try
            {
                if (_postgres != null)
                {
                    await _postgres.DisposeAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during container cleanup: {e.Message}");
            }
        }
    }

    public static class AsyncHelper
    {
        private static readonly TaskFactory MyTaskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return MyTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            MyTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}