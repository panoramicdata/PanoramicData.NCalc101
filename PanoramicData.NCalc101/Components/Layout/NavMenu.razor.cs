using Microsoft.AspNetCore.Components;
using PanoramicData.NCalc101.Interfaces;
using PanoramicData.NCalc101.Models;

namespace PanoramicData.NCalc101.Components.Layout;

public partial class NavMenu
{
	[Inject]
	public IWorkspaceService? WorkspaceService { get; set; }

	[Inject]
	public NavigationManager NavigationManager { get; set; } = null!;

	private readonly List<string> _workspaceNames = [];
	private string _currentWorkspaceName = string.Empty;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		WorkspaceService!.Subscribe(
			NotificationType.WorkspaceListUpdated,
			async notification =>
			{
				await RefreshWorkspaceNamesAsync();
				StateHasChanged();
			}
			);
		WorkspaceService!.Subscribe(
			NotificationType.CurrentWorkspaceUpdated,
			async notification =>
			{
				if (_currentWorkspaceName == WorkspaceService.Workspace.Name)
				{
					return;
				}

				_currentWorkspaceName = WorkspaceService.Workspace.Name;
				StateHasChanged();
			}
			);
		await RefreshWorkspaceNamesAsync();
	}

	private async Task RefreshWorkspaceNamesAsync()
	{
		var newNameList = await WorkspaceService!.GetNamesAsync(default);
		_currentWorkspaceName = WorkspaceService.Workspace.Name;
		_workspaceNames.Clear();
		_workspaceNames.AddRange(newNameList);
	}

	private async Task ClickAsync(string workspaceName)
	{
		await WorkspaceService!.SelectAsync(workspaceName, default);
	}

	private async Task DeleteAsync(string workspaceName)
	{
		await WorkspaceService!.DeleteAsync(workspaceName, default);
	}
}
