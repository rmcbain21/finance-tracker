using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using FinanceTracker.Core;
using FinanceTracker.Core.Ports;
using FinanceTracker.Web;
using FinanceTracker.Web.Adapters;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddSingleton<IRecordStore, DexieRecordStore>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<AppCore>();

await builder.Build().RunAsync();
