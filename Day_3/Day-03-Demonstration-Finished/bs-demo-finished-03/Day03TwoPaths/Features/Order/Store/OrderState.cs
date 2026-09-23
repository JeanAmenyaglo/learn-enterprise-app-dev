namespace Day03TwoPaths.Features.Order.Store;

using System.Globalization;
using Day03TwoPaths.Models;

/// <summary>
/// The store slice for the Flux path — the single source of truth for the order.
/// </summary>
/// <remarks>
/// This holds the same three fields as <see cref="OrderDraft"/>, but it lives in the store
/// rather than in a component, so navigating does not destroy it and no page ever owns a
/// second copy. That is the whole difference between the two paths in this app.
/// <para>
/// The record is immutable by design: every property is <c>init</c>-only, so a reducer cannot
/// edit state in place — it produces a new <see cref="OrderState"/> with a <c>with</c>
/// expression instead.
/// </para>
/// </remarks>
public record OrderState
{
    /// <summary>Name of the customer placing the order.</summary>
    public string CustomerName { get; init; } = "Jane Smith";

    /// <summary>City the order is delivered to.</summary>
    public string City { get; init; } = "Edmonton";

    /// <summary>
    /// Number of units ordered — the field every step of the wizard can edit.
    /// </summary>
    public int Quantity { get; init; } = 1;

    /// <summary>Order total, derived from <see cref="Quantity"/> rather than stored.</summary>
    public decimal Total => Quantity * OrderDraft.UnitPrice;

    /// <summary>
    /// <see cref="Total"/> formatted as currency, with the culture named explicitly so the
    /// demo shows the same symbol on every machine.
    /// </summary>
    public string TotalDisplay => Total.ToString("C", CultureInfo.GetCultureInfo("en-CA"));
}
