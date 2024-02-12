﻿using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests
{
    [TestFixture]
    public class RegisterServiceTests : IocTestFixture
    {
        private IRegisterService CreateSut()
        {
            return Container.GetRequiredService<IRegisterService>();
        }

        [Test]
        public async Task RegisterAsync_WhenModel_ThenNewUser()
        {
            var model = new RegisterUser
            {
                Email = "test@test.com",
                Password = "Password1!"
            };

            var sut = CreateSut();
            await sut.RegisterAsync(model).ConfigureAwait(false);
        }
    }
}
