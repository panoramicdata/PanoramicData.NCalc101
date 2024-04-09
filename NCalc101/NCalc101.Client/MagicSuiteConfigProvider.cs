using MagicSuite.Framework.Services;

namespace NCalc101.Client;

public class MagicSuiteConfigProvider : IMagicSuiteConfigProvider
{
	public string AppTitle { get; } = "NCalc 101";

	public Uri AppImageUri { get; } = new Uri(MagicSuiteServiceCommon.LogoSvgAlertMagic, UriKind.Relative);

	public string AppVersion => ThisAssembly.AssemblyInformationalVersion;

	public ValueTask<NavMenuItems> GetMenuItemsAsync(
		IMagicSuiteService magicSuiteService,
		ILogger<IMagicSuiteConfigProvider> logger,
		CancellationToken cancellationToken)
	{
		var menuItems = new NavMenuItems();
		menuItems.AddItem(new() { Name = "Page 1", Url = "/page1", Icon = "fa-solid fa-cloud-arrow-down" });
		return ValueTask.FromResult(menuItems);
	}
}