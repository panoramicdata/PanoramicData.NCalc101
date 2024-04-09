using PanoramicData.ReportMagic.Data;

namespace NCalc101.Client.Pages;

public partial class Home
{
	private Person? _person;

	[Inject] private IMagicSuiteService MagicSuiteService { get; set; } = null!;

	protected override async Task OnInitializedAsync()
		=> _person = await MagicSuiteService.GetUserInfoAsync(default);
}
