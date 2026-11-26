namespace Shopping.Domain.Items;

/// <summary>What adding a name again did to the item already in the list.</summary>
public enum ReAddOutcome
{
    /// <summary>It was needed: it stays, with the new quantity when one was given.</summary>
    Updated,

    /// <summary>It was in the cart: it is needed again.</summary>
    Restored,
}
