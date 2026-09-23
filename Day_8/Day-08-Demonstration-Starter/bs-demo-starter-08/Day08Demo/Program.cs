using Fluxor;
using Blazored.LocalStorage;
using Day08Demo.Services;
using Day08Demo.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluxor(options =>
    options.ScanAssemblies(typeof(Program).Assembly));

builder.Services.AddBlazoredLocalStorage();
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
