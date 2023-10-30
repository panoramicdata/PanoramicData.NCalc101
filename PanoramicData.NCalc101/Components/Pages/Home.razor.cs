using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor;
using PanoramicData.Blazor.Models;
using PanoramicData.NCalc101.Models;
using PanoramicData.NCalcExtensions;

namespace PanoramicData.NCalc101.Components.Pages;

public partial class Home
{
	[Inject]
	public NavigationManager NavigationManager { get; set; } = null!;

	[SupplyParameterFromQuery(Name = "expression")]
	private string? Expression { get; set; }

	[SupplyParameterFromQuery(Name = "variables")]
	private string? HttpEncodedJsonVariables { get; set; }

	private PDTable<Variable>? _table;
	private string _result = string.Empty;
	private string _resultType = string.Empty;
	private VariableDataProviderService _variableDataProviderService = new(default);
	private string _exceptionMessage = string.Empty;

	protected override async Task OnInitializedAsync()
	{
		if (!string.IsNullOrWhiteSpace(HttpEncodedJsonVariables))
		{
			_variableDataProviderService = new(HttpEncodedJsonVariables);
		}

		await EvaluateAsync();
		await base.OnInitializedAsync();
	}

	private async Task AddVariableAsync<T>()
	{
		var variableName = await GetUnusedVariableNameAsync();
		await _variableDataProviderService.CreateAsync(new Variable(variableName, typeof(T).ToString(), default(T)), CancellationToken.None);
		await _table!.RefreshAsync();
	}

	private async Task DeleteRowAsync()
	{
		var rowIds = _table!.Selection;
		if (rowIds is null || rowIds.Count == 0)
		{
			return;
		}

		var existingVariables = await _variableDataProviderService
			.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
		var existingVariablesForDeletion = existingVariables
			.Items
			.Where(v => rowIds.Contains(v.Id.ToString().ToLowerInvariant()))
			.ToList();

		foreach (var variableForDeletion in existingVariablesForDeletion)
		{
			await _variableDataProviderService.DeleteAsync(variableForDeletion, CancellationToken.None);
		}

		await EvaluateAsync();

		await _table!.RefreshAsync();
	}

	private async Task<string> GetUnusedVariableNameAsync()
	{
		var existingVariables = await _variableDataProviderService
			.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
		var existingVariableNames = existingVariables.Items.Select(v => v.Name).ToList();
		var variableNameChar = "abcdefghijklmnopqrstuvwxyz".FirstOrDefault(ch => !existingVariableNames.Contains(ch.ToString()));
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

	private async Task OnExpressionChangedAsync(string expression)
	{
		Expression = expression;
		await EvaluateAsync();
	}

	private async Task EvaluateAsync()
	{
		// Update the deep link
		NavigationManager.NavigateTo($"?expression={Uri.EscapeDataString(Expression ?? string.Empty)}&variables={_variableDataProviderService.HttpEncodedJsonVariables}", false);

		try
		{
			var expression = new ExtendedExpression(TidyExpression(Expression ?? string.Empty));
			var variables = await _variableDataProviderService.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
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