namespace Notifications.Application;

/// <summary>Where the site is (setting <c>Site:Url</c>), for the links in the messages.</summary>
public sealed record NotificationsSettings(string SiteUrl)
{
    public string Page(string path) => $"{SiteUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}
