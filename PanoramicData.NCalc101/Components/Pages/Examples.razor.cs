using Microsoft.AspNetCore.Components;
using PanoramicData.NCalc101.Examples;
using PanoramicData.NCalc101.Interfaces;

namespace PanoramicData.NCalc101.Components.Pages;

public partial class Examples
{
	[Inject]
	public NavigationManager NavigationManager { get; set; } = null!;

	[Inject]
	public IWorkspaceService? WorkspaceService { get; set; }

	public async Task CreateExampleWorkspaceAsync(Example example)
	{
		await WorkspaceService!.CreateWorkspaceAsync(example.Name, example.Expression, example.Variables, default);
		NavigationManager.NavigateTo($"/?workspace={example.Name}", false);
	}
}
