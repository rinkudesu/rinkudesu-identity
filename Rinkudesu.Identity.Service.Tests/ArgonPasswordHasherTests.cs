using Microsoft.AspNetCore.Identity;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Services;

namespace Rinkudesu.Identity.Service.Tests;

public class ArgonPasswordHasherTests
{
    private readonly ArgonPasswordHasher _hasher;
    private readonly User _mockUser;

    public ArgonPasswordHasherTests()
    {
        Environment.SetEnvironmentVariable("RINKU_ARGON_SECRET", "this is a test but it cannot be ignored");
        _hasher = new ArgonPasswordHasher();
        _mockUser = new User();
    }

    [Theory]
    [InlineData("this is a test")]
    [InlineData("a very long password that argon should hopefully handle correctly sfhalskdflaskjdflkasjadflkajhdlfkjhaslkdfjhaslkdjhlskajdhflkasjdflkasjdfhlkwasjhfaalioweahfliweufghw3e8i7rfyhwe67ufgYw8ie7ufyh8edi7ufvyhws8ed7ufY87weeyhrfv8w7uedyhf8w7sedyhf8iqwedyhrf87ewdfyh8ifr7uetdfgh8i67ytghfwe6wesdyiwed687wesdesdf788ywesd87uiwywesd7iuh8wesd")]
    [InlineData("password with funny symbols SOD&HF*OA&WEFHO*&H*OQW&@*$!^@&!*@#!(@$^($&!(POP][9,]9,0,90''.'")]
    public void HashThenVerify_SamePassword_ReturnsTrue(string password)
    {
        var hash = _hasher.HashPassword(_mockUser, password);
        var verificationResult = _hasher.VerifyHashedPassword(_mockUser, hash, password);

        Assert.Equal(PasswordVerificationResult.Success, verificationResult);
    }

    [Fact]
    public void HashThenVerify_DifferentPasswords_ReturnsFalse()
    {
        var hash = _hasher.HashPassword(_mockUser, "password one");
        var verificationResult = _hasher.VerifyHashedPassword(_mockUser, hash, "password two");

        Assert.Equal(PasswordVerificationResult.Failed, verificationResult);
    }
}
