using SubscriptionReminder.Api.DTOs;

namespace SubscriptionReminder.Api.Services;

public interface IPaymentGatewayService
{
    Task<PaymentGatewayResponse> PayAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);
}
