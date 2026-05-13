namespace SubscriptionReminder.Api.DTOs;

public record CreateCustomerRequest(string FullName, string Email, string? PhoneNumber);
public record CustomerResponse(int Id, string FullName, string Email, string? PhoneNumber, DateTime CreatedAt);
