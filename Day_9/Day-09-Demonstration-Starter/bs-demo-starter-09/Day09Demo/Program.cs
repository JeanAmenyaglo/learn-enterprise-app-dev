using Day09Demo.Components;

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
// DEMO STEP 1: Add the fallback route (CODE LIVE)
//
// Right now /nonexistent returns a bare 404 with an empty body — routing finds no endpoint,
// so the browser shows its own error page. Blazor never even runs.
//
// TODO: Add the line below, then create Components/Pages/NotFound.razor (@page "/not-found").
//       app.UseStatusCodePagesWithReExecute("/not-found");
// It re-runs the pipeline against /not-found while KEEPING the 404 status code.
//
// This line is necessary but NOT sufficient. It fixes the SERVER pass only. You must also add
// NotFoundPage to the <Router> in Components/Routes.razor — otherwise the Blazor circuit
// connects a moment later, re-runs the router against /nonexistent, matches nothing, and
// overwrites your styled page with Blazor's bare "Not found". Both pieces, same step.
// ================================================================

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
