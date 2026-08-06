using GymManager.Application.Auth;

namespace GymManager.Application.Abstractions;

public interface ITokenGenerator
{
    string Generate(UserCredentials user);
}