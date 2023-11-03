using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor;
using PanoramicData.Blazor.Models;
using PanoramicData.NCalc101.Interfaces;
using PanoramicData.NCalc101.Models;
using PanoramicData.NCalcExtensions;

namespace PanoramicData.NCalc101.Components.Pages;

public partial class Home
{
	[Inject]
	public NavigationManager NavigationManager { get; set; } = null!;

	[Inject]
	public IToastService ToastService { get; set; } = null!;

	[Inject]
	public IWorkspaceService WorkspaceService { get; set; } = null!;

	[SupplyParameterFromQuery(Name = "workspace")]
	private string? WorkspaceName { get; set; }

	private readonly string AlphabetLowerCase = new(Enumerable.Range(97, 26).Select(n => (char)n).ToArray());

	private PDTable<Variable>? _table;
	private string _result = string.Empty;
	private string _resultType = string.Empty;
	private string _exceptionMessage = string.Empty;
	private readonly List<MenuItem> _addVariableMenuItems = [
		new MenuItem { Text = "&&String", IconCssClass = "fa-solid fa-a" },
		new MenuItem { Text = "&&Integer", IconCssClass = "fa-solid fa-1" },
		new MenuItem { Text = "&&Double", IconCssClass = "fa-solid fa-temperature-half" },
		new MenuItem { Text = "&&Boolean", IconCssClass = "fa-solid fa-toggle-on" },
		new MenuItem { Text = "D&&ateTime", IconCssClass = "fa-solid fa-clock" },
		new MenuItem { Text = "D&&ateTimeOffset", IconCssClass = "fa-solid fa-clock" },
		new MenuItem { Text = "&&Expression", IconCssClass = "fa-solid fa-calculator" },
	];

	protected override async Task OnInitializedAsync()
	{
		if (string.IsNullOrWhiteSpace(WorkspaceName))
		{
			WorkspaceName = (await WorkspaceService.LastSelectedAsync(default)) ?? "default";

			// Update the deep link
			NavigationManager.NavigateTo($"?workspace={WorkspaceService.Workspace.Name}", false);
		}

		await WorkspaceService.SelectAsync(WorkspaceName, default);

		await _table!.RefreshAsync();

		await EvaluateAsync();

		await base.OnInitializedAsync();
	}

	private async Task NameChangedAsync(string newName)
	{
		await WorkspaceService.RenameAsync(newName, default);
		NavigationManager.NavigateTo($"?workspace={WorkspaceService.Workspace.Name}", true);
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

	private async Task DeleteWorkspaceAsync()
	{
		await WorkspaceService!.DeleteAsync(WorkspaceService.Workspace.Name, default);
		NavigationManager.NavigateTo("/", true);
	}

	private async Task CreateWorkspaceAsync()
	{
		var newName = Guid.NewGuid().ToString();
		await WorkspaceService!.CreateWorkspaceAsync(newName, "1 + 1", [], default);
		NavigationManager.NavigateTo($"/?workspace={newName}", false);
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

	private async Task OnDoubleClickRowAsync()
	{
		await _table!.BeginEditAsync();
	}

	private async Task OnVariableChangedAsync()
	{
		await EvaluateAsync();
	}

	private void TableExceptionHandler(Exception e)
	{
		ToastService.Error(e.Message, "Table Exception");
	}

	private async Task OnExpressionChangedAsync(string expression)
	{
		await WorkspaceService.SetExpressionAsync(expression, default);
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
		}
		catch (Exception ex)
		{
			_result = string.Empty;
			_exceptionMessage = ex.Message;
		}
	}

	private static string TidyExpression(string expression)
		=> expression
			.Replace('\r', ' ')
			.Replace('\n', ' ');
}