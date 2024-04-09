namespace NCalc101.Client.Pages;

public partial class Home
{
	private MagicSuiteUserInfo? _magicSuiteUserInfo;

	[Inject] private IMagicSuiteService MagicSuiteService { get; set; } = null!;

	protected override async Task OnInitializedAsync()
		=> _magicSuiteUserInfo = await MagicSuiteService.GetUserInfoAsync();
}
