using Dzaba.PathoAutho.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests
{
    internal class IocTestFixture
    {
        private ServiceProvider container;

        protected IServiceProvider Container => container;

        [SetUp]
        public void SetupContainer()
        {
            var services = new ServiceCollection();
            services.RegisterDzabaPathoAuthoLib(o => o.UseInMemoryDatabase("IntegrationTestPathoAutho"));

            container = services.BuildServiceProvider();

            InitDb();
        }

        private void InitDb()
        {
            Container.GetRequiredService<DbContext>().Database.EnsureCreated();
        }

        [TearDown]
        public void DisposeContainer()
        {
            container?.Dispose();
        }
    }
}
