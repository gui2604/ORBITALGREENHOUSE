using Microsoft.Extensions.Configuration;
using Moq;
using OrbitalGreenhouse.Api.Dtos;
using OrbitalGreenhouse.Api.Exceptions;
using OrbitalGreenhouse.Api.Models;
using OrbitalGreenhouse.Api.Repositories;
using OrbitalGreenhouse.Api.Services;
using OrbitalGreenhouse.Api.Tests.Support;
using Xunit;
using Xunit.Abstractions;

namespace OrbitalGreenhouse.Api.Tests.Services;

public class UserServiceTests
{
    private readonly TestScenarioLogger _log;

    public UserServiceTests(ITestOutputHelper output) => _log = new TestScenarioLogger(output);

    [Fact(DisplayName = "CT-05 — Login com e-mail inexistente retorna credenciais inválidas")]
    public async Task CT05_LoginUnknownEmail_ThrowsValidation()
    {
        _log.Begin("CT-05", "Login e-mail inexistente — 400");

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByEmailAsync("desconhecido@fiap.test")).ReturnsAsync((User?)null);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "orbital-greenhouse-unit-test-secret-key-32chars!!",
                ["Jwt:Issuer"] = "orbital-greenhouse",
                ["Jwt:Audience"] = "orbital-greenhouse-clients"
            })
            .Build();

        var service = new UserService(repo.Object, config);
        var dto = new UserLoginDto { Email = "desconhecido@fiap.test", Password = "qualquer" };

        _log.Data("E-mail", dto.Email);
        _log.Step("Usuário não existe em OGH_USERS");

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.LoginAsync(dto));

        _log.Data("Mensagem API", ex.Message);
        Assert.Equal("Credenciais inválidas.", ex.Message);
        _log.Pass("Mesma mensagem vista no Swagger/Postman sem register prévio.");
    }
}
