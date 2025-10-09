namespace FreakyFashion.Contracts.Auth;

public record LoginResponse(string access_token, string token_type, int expires_in);
