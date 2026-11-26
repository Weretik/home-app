namespace Shopping.Contracts;

/// <summary>What the Notifications module may read from Shopping; implemented by the Shopping module.</summary>
public interface IShoppingDigest
{
    /// <summary>Every needed item, in the department order, then by name.</summary>
    Task<IReadOnlyList<NeededItem>> ListNeededAsync(CancellationToken cancellationToken);
}
