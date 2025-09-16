namespace Modules.Auth.Shared.ActionsRequests;

public record TokenValidationRequest(string? Token, string? Email);