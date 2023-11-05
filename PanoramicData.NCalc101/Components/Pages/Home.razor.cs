using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using PanoramicData.Blazor;
using PanoramicData.Blazor.Models;
using PanoramicData.NCalc101.Interfaces;
using PanoramicData.NCalc101.Models;
using PanoramicData.NCalcExtensions;
using System.Text;
using System.Text.Json;

namespace PanoramicData.NCalc101.Components.Pages;

public partial class Home
{
	[Inject]
	public NavigationManager NavigationManager { get; set; } = null!;

	[Inject]
	public IToastService ToastService { get; set; } = null!;

	[Inject]
	public IWorkspaceService WorkspaceService { get; set; } = null!;

	[Inject]
	IJSRuntime JS { get; set; } = null!;

	[SupplyParameterFromQuery(Name = "workspace")]
	private string? WorkspaceName { get; set; }

	private InputFile? _inputFile;
	private bool _showInputFile = false;
	private string _expression = string.Empty;
	private string _expression2 = string.Empty;

	private readonly string AlphabetLowerCase = new(Enumerable.Range(97, 26).Select(n => (char)n).ToArray());

	private PDTable<Variable>? _table;
	private string _result = string.Empty;
	private string _resultType = string.Empty;
	private string _exceptionMessage = string.Empty;
	private string _exceptionType = string.Empty;
	private readonly List<MenuItem> _addVariableMenuItems = [
		new MenuItem { Text = "String", IconCssClass = "fa-fw fa-solid fa-a" },
		new MenuItem { Text = "Integer", IconCssClass = "fa-fw fa-solid fa-1" },
		new MenuItem { Text = "Double", IconCssClass = "fa-fw fa-solid fa-temperature-half" },
		new MenuItem { Text = "Boolean", IconCssClass = "fa-fw fa-solid fa-toggle-on" },
		new MenuItem { Text = "DateTime", IconCssClass = "fa-fw fa-solid fa-clock" },
		new MenuItem { Text = "DateTimeOffset", IconCssClass = "fa-fw fa-regular fa-clock" },
		new MenuItem { Text = "Expression", IconCssClass = "fa-fw fa-solid fa-calculator" },
	];
	private readonly static JsonSerializerOptions DefaultJsonSerializerOptions = new()
	{
		WriteIndented = true
	};

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		WorkspaceService.Subscribe(
			NotificationType.CurrentWorkspaceUpdated,
			async notification =>
			{
				NavigationManager.NavigateTo($"?workspace={WorkspaceService.Workspace.Name}", false);
				await _table!.RefreshAsync();
				_expression = WorkspaceService.Workspace.Expression ?? string.Empty;
				_expression2 = WorkspaceService.Workspace.Expression ?? string.Empty;
				await EvaluateAsync();
				StateHasChanged();
			}
		);

		if (string.IsNullOrWhiteSpace(WorkspaceName))
		{
			WorkspaceName = (await WorkspaceService.LastSelectedAsync(default)) ?? "default";
		}

		await WorkspaceService.SelectAsync(WorkspaceName, default);
	}

	private async Task NameChangedAsync(string newName)
	{
		await WorkspaceService.RenameAsync(newName, default);
	}

	private async Task AddVariableAsync(string value)
	{
		switch (value)
		{
			case "String":
				await AddVariableAsync<string>();
				break;
			case "Integer":
				await AddVariableAsync<int>();
				break;
			case "Double":
				await AddVariableAsync<double>();
				break;
			case "DateTime":
				await AddVariableAsync<DateTime>();
				break;
			case "DateTimeOffset":
				await AddVariableAsync<DateTimeOffset>();
				break;
			case "Boolean":
				await AddVariableAsync<bool>();
				break;
			case "TimeSpan":
				await AddVariableAsync<TimeSpan>();
				break;
			case "Guid":
				await AddVariableAsync<Guid>();
				break;
			case "null":
				await AddVariableAsync<object?>();
				break;
			case "Expression":
				await AddVariableAsync<ExtendedExpression>();
				break;
			default:
				throw new InvalidOperationException($"Unsupported type: {value}");
		}
	}

	private async Task AddVariableAsync<T>()
	{
		var variableName = await GetUnusedVariableNameAsync();
		await WorkspaceService!.CreateAsync(new Variable
		{
			Name = variableName,
			Type = typeof(T).ToString(),
			Value = string.Empty
		}, CancellationToken.None);

		await _table!.RefreshAsync();
	}

	private void ImportWorkspace()
	{
		_showInputFile = true;
		StateHasChanged();
	}

	private async Task ExportWorkspaceAsync()
	{
		var workspaceAsJson = JsonSerializer
			.Serialize(
				WorkspaceService.Workspace,
				DefaultJsonSerializerOptions
			);
		// Load workspaceAsJson into a memory stream
		using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(workspaceAsJson));
		using var streamRef = new DotNetStreamReference(stream: memoryStream);
		await JS.InvokeVoidAsync("downloadFileFromStream", $"{WorkspaceService.Workspace.Name}.json", streamRef);
	}

	private async Task UploadWorkspaceAsync(InputFileChangeEventArgs args)
	{
		var browserFiles = args.GetMultipleFiles();

		if (browserFiles.Count == 0)
		{
			ToastService.Error("No files selected.", "Upload Error");
		}

		var firstWorkspaceName = string.Empty;
		var fileIndex = 0;
		foreach (var browserFile in browserFiles)
		{
			fileIndex++;

			await using MemoryStream memoryStream = new();
			await browserFile.OpenReadStream().CopyToAsync(memoryStream);
			memoryStream.Position = 0;
			var workspace = await JsonSerializer.DeserializeAsync<Workspace>(memoryStream, DefaultJsonSerializerOptions);
			if (workspace is null)
			{
				ToastService.Error($"Workspace {fileIndex} failed to deserialize.  Skipping.", "Upload Error");
				return;
			}

			if (string.IsNullOrWhiteSpace(workspace.Name))
			{
				ToastService.Error($"Workspace {fileIndex} has no name.  Skipping.", "Upload Error");
				continue;
			}
			else
			{
				firstWorkspaceName = workspace.Name;
			}

			await WorkspaceService!.CreateWorkspaceAsync(workspace.Name, workspace.Expression, workspace.Variables, default);
			ToastService.Success($"Workspace {fileIndex} imported.", "Workspace Import");
		}

		await Task.Delay(1000);

		if (!string.IsNullOrWhiteSpace(firstWorkspaceName))
		{
			await WorkspaceService.SelectAsync(firstWorkspaceName, default);
		}
	}

	private async Task DeleteWorkspaceAsync()
	{
		await WorkspaceService!.DeleteAsync(WorkspaceService.Workspace.Name, default);
	}

	private async Task CreateWorkspaceAsync()
	{
		var newName = Guid.NewGuid().ToString()[..8];
		await WorkspaceService!.CreateWorkspaceAsync(newName, "1 + 1", [], default);
	}

	private async Task DeleteRowAsync()
	{
		var rowIds = _table!.Selection;
		if (rowIds is null || rowIds.Count == 0)
		{
			return;
		}

		var existingVariables = await WorkspaceService!
			.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
		var existingVariablesForDeletion = existingVariables
			.Items
			.Where(v => rowIds.Contains(v.Id.ToString().ToLowerInvariant()))
			.ToList();

		foreach (var variableForDeletion in existingVariablesForDeletion)
		{
			await WorkspaceService!.DeleteAsync(variableForDeletion, CancellationToken.None);
		}

		await EvaluateAsync();

		await _table!.RefreshAsync();
	}

	private void VariableSelectionChanged()
	{
		StateHasChanged();
	}

	private bool VariablesSelected => _table!.Selection!.Count > 0;

	private async Task<string> GetUnusedVariableNameAsync()
	{
		var existingVariables = await WorkspaceService!
			.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
		var existingVariableNames = existingVariables.Items.Select(v => v.Name).ToList();
		var variableNameChar = AlphabetLowerCase.FirstOrDefault(ch => !existingVariableNames.Contains(ch.ToString()));
		if (variableNameChar == default)
		{
			variableNameChar = 'X';
		}

		var variableName = variableNameChar.ToString();
		return variableName;
	}

	private void TableExceptionHandler(Exception e)
	{
		ToastService.Error(e.Message, "Table Exception");
	}

	private async Task OnExpressionChangedAsync(string expression)
	{
		await WorkspaceService.SetExpressionAsync(expression, default);
		if (_expression != expression)
		{
			_expression = expression;
		}

		if (_expression2 != expression)
		{
			_expression2 = expression;
		}

		await EvaluateAsync();
	}

	private async Task EvaluateAsync()
	{
		try
		{
			var expression = new ExtendedExpression(TidyExpression(WorkspaceService.Workspace.Expression ?? string.Empty));
			var variables = await WorkspaceService!.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
			foreach (var variable in variables.Items)
			{
				expression.Parameters[variable.Name] = variable.GetValue();
			}

			var output = expression.Evaluate();
			_result = output?.ToString() ?? string.Empty;
			_resultType = output is null ? "null" : output.GetType().ToString();
			_exceptionMessage = string.Empty;
			_exceptionType = string.Empty;
		}
		catch (Exception ex)
		{
			_result = string.Empty;
			_exceptionMessage = ex.Message;
			_exceptionType = ex.GetType().ToString();
		}
	}

	private static string TidyExpression(string expression)
		=> expression
			.Replace('\r', ' ')
			.Replace('\n', ' ');
}