namespace Modules.Shared.Attributes;

/// <summary>
///     Allows method to be executed without the token validation proccess
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AllowWithoutTokenValidationAttribute : Attribute
{
}