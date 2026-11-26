namespace Gifts.Domain.Wishlist;

/// <summary>
/// Where a gift idea stands. <see cref="Wanted"/> is the active list; <see cref="Gifted"/> (given or bought, the sheet's
/// «Куплено») and <see cref="Dropped"/> («Не актуально») are the archive.
/// </summary>
public enum GiftStatus
{
    Wanted,
    Gifted,
    Dropped,
}
