namespace Gifts.Application.Purchases;

/// <summary>Links to a gift on the site, for the to-do and the Telegram reminder (setting <c>Site:Url</c>).</summary>
public sealed class GiftLinks(string siteUrl)
{
    public const string SettingName = "Site:Url";

    public string For(Guid giftId) => $"{siteUrl.TrimEnd('/')}/gifts?gift={giftId}";
}
