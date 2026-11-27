using Identity.Application.Features.Sessions.Commands.SignIn;
using Identity.Application.Features.Sessions.Commands.SignOut;

namespace UnitTests.Identity;

public sealed class SignInValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void An_Empty_Credential_IsRefused(string credential)
    {
        Assert.False(new SignInValidator().Validate(new SignInCommand(credential)).IsValid);
    }

    [Fact]
    public void A_Credential_LongerThanAnyGoogleToken_IsRefused()
    {
        var command = new SignInCommand(new string('x', SignInValidator.MaxCredentialLength + 1));

        Assert.False(new SignInValidator().Validate(command).IsValid);
        Assert.True(new SignInValidator().Validate(new SignInCommand("header.payload.signature")).IsValid);
    }

    [Fact]
    public void Sign_Out_NeedsAToken()
    {
        Assert.False(new SignOutValidator().Validate(new SignOutCommand("")).IsValid);
        Assert.True(new SignOutValidator().Validate(new SignOutCommand("token")).IsValid);
    }
}
