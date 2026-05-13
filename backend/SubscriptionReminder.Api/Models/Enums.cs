namespace SubscriptionReminder.Api.Models;

public enum SubscriptionType
{
    Electricity = 1,
    Water = 2,
    Internet = 3,
    Gsm = 4,
    NaturalGas = 5,
    Other = 99
}

public enum SubscriptionStatus
{
    Active = 1,
    Passive = 2
}

public enum PaymentStatus
{
    Successful = 1,
    Failed = 2
}

public enum NotificationChannel
{
    Email = 1,
    Sms = 2
}
