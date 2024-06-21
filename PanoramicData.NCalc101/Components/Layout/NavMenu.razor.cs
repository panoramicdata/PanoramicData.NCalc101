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
			notification =>
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

	private Task ClickAsync(string workspaceName)
		=> WorkspaceService!.SelectAsync(workspaceName, default);

	private Task DeleteAsync(string workspaceName)
		=> WorkspaceService!.DeleteAsync(workspaceName, default);
}
