using Day09Practice.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// ================================================================
// EXERCISE 1: Add the fallback route
//
// Run the app and navigate to /nonexistent. You get the browser's own error page — routing
// finds no endpoint, returns a bare 404, and Blazor never runs. No navbar, no way back.
//
// TODO: Add this line:
//       app.UseStatusCodePagesWithReExecute("/not-found");
// TODO: Create Components/Pages/NotFound.razor with @page "/not-found" and a styled
//       404 message + "Go Home" link.
//
// The middleware re-runs the pipeline against /not-found while KEEPING the 404 status code.
// Because NotFound.razor is an ordinary routed page, it picks up MainLayout automatically —
// the navbar comes along for free, with no <LayoutView> wrapper needed.
// ================================================================

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
