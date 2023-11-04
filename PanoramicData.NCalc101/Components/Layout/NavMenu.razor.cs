using Microsoft.AspNetCore.Components;
using PanoramicData.NCalc101.Interfaces;

namespace PanoramicData.NCalc101.Components.Layout;

public partial class NavMenu
{
	[Inject]
	public IWorkspaceService? WorkspaceService { get; set; }

	[Inject]
	public NavigationManager NavigationManager { get; set; } = null!;

	private readonly List<string> _workspaceNames = [];

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		await UpdateWorkspaceNamesAsync();
	}

	private async Task UpdateWorkspaceNamesAsync()
	{
		if (WorkspaceService is null)
		{
			return;
		}

		_workspaceNames.Clear();
		_workspaceNames.AddRange(await WorkspaceService.GetNamesAsync(default));
	}

	private Task ClickAsync(string workspaceName)
	{
		NavigationManager.NavigateTo($"/?workspace={workspaceName}", true);
		return Task.CompletedTask;
	}

	private async Task DeleteAsync(string workspaceName)
	{
		await WorkspaceService!.DeleteAsync(workspaceName, default);
		await UpdateWorkspaceNamesAsync();
	}
}
