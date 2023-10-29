﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
	private string Expression { get; set; } = "1 + 2";

	private PDTable<Variable>? _table;
	private string _result = string.Empty;
	private readonly VariableDataProviderService _variableDataProviderService = new();
	private string _exceptionMessage = string.Empty;

	private async Task AddVariableAsync<T>()
	{
		await _variableDataProviderService.CreateAsync(new Variable
		{
			Type = typeof(T),
			Name = "New Variable",
			ValueAsString = default(T)?.ToString() ?? string.Empty
		}, CancellationToken.None);

		await _table!.RefreshAsync();
	}

	private async Task OnVariableChangedAsync()
	{
		await EvaluateAsync();
	}

	private async Task OnKeyPressedAsync(KeyboardEventArgs e)
	{
		await EvaluateAsync();
	}

	private async Task EvaluateAsync()
	{
		// Update the deep link
		NavigationManager.NavigateTo($"?expression={Uri.EscapeDataString(Expression ?? string.Empty)}", false);

		try
		{
			var expression = new ExtendedExpression(TidyExpression(Expression));
			var variables = await _variableDataProviderService.GetDataAsync(new DataRequest<Variable>(), CancellationToken.None);
			foreach (var variable in variables.Items)
			{
				expression.Parameters[variable.Name] = variable.Value;
			}

			_result = expression.Evaluate()?.ToString() ?? string.Empty;
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