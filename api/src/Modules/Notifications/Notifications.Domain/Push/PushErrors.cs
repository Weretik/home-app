using BuildingBlocks.Domain.Results;

namespace Notifications.Domain.Push;

/// <summary>The rules push subscriptions and test pushes can break, with stable codes for the client (spec notifications/002).</summary>
public static class PushErrors
{
    public static readonly Error NotConfigured = Error.Conflict(
        "notifications.push_not_configured",
        "Push-сповіщення не налаштовано (налаштування App Service WebPush__PublicKey і WebPush__PrivateKey).");

    public static readonly Error NoPerson = Error.Conflict(
        "notifications.push_no_person", "Push-сповіщення доступні лише Віталію і Маші.");

    public static readonly Error DeviceNotFound = Error.NotFound(
        "notifications.push_device_not_found", "Такого пристрою серед ваших немає.");

    public static readonly Error NoDevices = Error.NotFound(
        "notifications.push_no_devices", "Push-сповіщення не ввімкнено на жодному вашому пристрої.");

    public static readonly Error Failed = Error.Conflict(
        "notifications.push_failed", "Жоден пристрій не прийняв push-сповіщення; спробуйте пізніше.");
}
