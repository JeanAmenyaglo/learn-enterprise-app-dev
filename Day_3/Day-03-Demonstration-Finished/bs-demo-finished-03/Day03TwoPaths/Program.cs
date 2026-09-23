using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Day03TwoPaths.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The store is registered ONCE, here. Every page on the /flux path reads from this one
// instance — that is what makes it a single source of truth. The /classic path registers
// nothing, because it has no store: it hand-carries its data in the address bar instead.
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

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
