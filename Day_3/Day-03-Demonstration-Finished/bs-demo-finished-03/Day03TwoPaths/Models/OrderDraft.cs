namespace Day03TwoPaths.Models;

using System.Globalization;

/// <summary>
/// The order as the "No Store" path carries it — an immutable snapshot of the three fields
/// the wizard collects.
/// </summary>
/// <remarks>
/// Note what has to happen for this object to travel between two pages. Blazor has nowhere to
/// hand an object across a navigation, so <see cref="ToQueryString"/> flattens it into text and
/// <see cref="FromQuery"/> rebuilds it on the other side. Every navigation is a
/// serialize/deserialize round trip, and the result is parked in the address bar where the
/// browser's Back and Forward buttons can replay an older copy of it.
/// <para>
/// Compare with <c>OrderState</c>, which holds the same three fields for the Flux path and
/// never has to travel at all.
/// </para>
/// </remarks>
public record OrderDraft
{
    /// <summary>Price of a single unit. Fixed here so the demo has one number to multiply.</summary>
    public const decimal UnitPrice = 24.99m;

    /// <summary>Name of the customer placing the order.</summary>
    public string CustomerName { get; init; } = "Jane Smith";

    /// <summary>City the order is delivered to.</summary>
    public string City { get; init; } = "Edmonton";

    /// <summary>
    /// Number of units ordered. This is the field the wizard makes editable on every step, so
    /// that losing an edit is visible no matter which page the user is on.
    /// </summary>
    public int Quantity { get; init; } = 1;

    /// <summary>
    /// Order total, always derived from <see cref="Quantity"/> rather than stored.
    /// </summary>
    /// <remarks>
    /// Deriving it means the total and the quantity can never disagree. A stored total would be
    /// a second copy of the same fact — the exact problem this whole demo is about.
    /// </remarks>
    public decimal Total => Quantity * UnitPrice;

    /// <summary>
    /// <see cref="Total"/> formatted as currency for display.
    /// </summary>
    /// <remarks>
    /// The culture is named explicitly so the demo shows "$74.97" on every machine. Left to the
    /// server's default, a Linux host running with invariant globalization renders the generic
    /// currency sign instead.
    /// </remarks>
    public string TotalDisplay => Total.ToString("C", CultureInfo.GetCultureInfo("en-CA"));

    /// <summary>
    /// Rebuilds an <see cref="OrderDraft"/> from the three values a page received in its query
    /// string, falling back to the defaults for anything the URL did not carry.
    /// </summary>
    /// <param name="name">Customer name from the query string, or <c>null</c> if absent.</param>
    /// <param name="city">Delivery city from the query string, or <c>null</c> if absent.</param>
    /// <param name="quantity">Quantity from the query string, or <c>null</c> if absent.</param>
    /// <returns>The reconstructed order.</returns>
    /// <remarks>
    /// The fallback is where data loss becomes visible. Land on step 1 with a bare URL and the
    /// quantity comes back as 1 — not because the user chose 1, but because nothing in the URL
    /// said otherwise.
    /// </remarks>
    public static OrderDraft FromQuery(string? name, string? city, int? quantity)
    {
        var fresh = new OrderDraft();

        return new OrderDraft
        {
            CustomerName = string.IsNullOrWhiteSpace(name) ? fresh.CustomerName : name,
            City = string.IsNullOrWhiteSpace(city) ? fresh.City : city,
            Quantity = quantity ?? fresh.Quantity
        };
    }

    /// <summary>
    /// Flattens this order into a query string for the next page to read.
    /// </summary>
    /// <returns>A query string beginning with <c>?</c>, ready to append to a route.</returns>
    /// <remarks>
    /// This string <em>is</em> the transport on the "No Store" path. If a value is not in here,
    /// it does not survive the navigation — which is why only the wizard buttons call this, and
    /// why the browser's own Back and Forward buttons lose whatever the user typed last.
    /// </remarks>
    public string ToQueryString()
    {
        var name = Uri.EscapeDataString(CustomerName);
        var city = Uri.EscapeDataString(City);

        return $"?name={name}&city={city}&qty={Quantity}";
    }
}
