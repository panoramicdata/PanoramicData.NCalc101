using MagicSuite.Framework.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NCalc101.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
	.AddMagicSuiteUiFramework<MagicSuiteConfigProvider>();

await builder.Build().RunAsync();
