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

// FALLBACK ROUTE (the "bad URL" 404) — when routing finds no endpoint for the requested URL,
// this re-runs the pipeline against /not-found while KEEPING the 404 status code the browser
// should see. Without this line, /nonexistent returns a bare 404 with an empty body and the
// user gets the browser's default error page — no navbar, no way back.
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
