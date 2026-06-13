using FluentAssertions;
using Tesseract.Domain.Users;
using Tesseract.Domain.Users.Values;
using Tesseract.Infrastructure.ClientServer.Identity.Dao;
using Tesseract.Infrastructure.ClientServer.Identity.Mappers;

namespace Tesseract.Infrastructure.Tests.ClientServer.Identity.Mappers;

public class UserMapperTests
{
    [Fact]
    internal void ToDomain_ValidInput_ReturnsCorrectlyMappedUser()
    {
        var user = new User(
            new UserId(Guid.NewGuid()),
            new UserHandle("kangaroo", "jump.er")
        );
        var dao = new UserDao
        {
            UserId = user.Id.Value,
            Localpart = user.Handle.Localpart.Value,
            Domain = user.Handle.Domain.Value,
        };

        var result = dao.ToDomain();

        result.Id.Value.Should().Be(user.Id.Value);
        result.Handle.Localpart.Value.Should().Be(user.Handle.Localpart.Value);
        result.Handle.Domain.Value.Should().Be(user.Handle.Domain.Value);
    }
}