using Fluxor;
using Blazored.LocalStorage;
using Day08Demo.Services;
using Day08Demo.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ScanAssemblies finds and registers every action, reducer, effect, and feature in this
// assembly by reflection — that's why none of the store classes (including the new
// CartPersistenceEffects) are wired up by hand.
builder.Services.AddFluxor(options =>
    options.ScanAssemblies(typeof(Program).Assembly));

// Blazored.LocalStorage is the day's new dependency: it wraps the browser localStorage API
// behind ILocalStorageService so the persistence effects can save/read with await instead of
// hand-writing JS interop. Registering it makes ILocalStorageService injectable everywhere.
builder.Services.AddBlazoredLocalStorage();

// MockCartService is registered but intentionally never injected — it's a worked example,
// not a wired dependency. It's the Result<T> (BYSResults) return-type specimen carried over
// from Day 7: students read GetCartItemsAsync here and meet the same service-layer pattern in
// Lab 1. Today's persistence goes through ILocalStorageService in the effects, and the cart is
// user-built (Add/Remove), so nothing actually consumes this service.
builder.Services.AddScoped<ICartService, MockCartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
