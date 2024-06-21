using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

	private bool _showInputFile;
	private string _expression = string.Empty;
	private string _expression2 = string.Empty;

	private readonly string AlphabetLowerCase = new(Enumerable.Range(97, 26).Select(n => (char)n).ToArray());

	private PDTable<Variable>? _table;
	private string _result = string.Empty;
	private string _resultType = string.Empty;
	private string _exceptionMessage = string.Empty;
	private string _exceptionType = string.Empty;
	private readonly SortCriteria _sortCriteria = new(nameof(Variable.Name), SortDirection.Ascending);
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
				//_enableWorkspaceDelete = WorkspaceService.Workspace.Name != "default";
				await EvaluateAsync();
				StateHasChanged();
			}
		);

		WorkspaceService.Subscribe(
			NotificationType.VariablesUpdated,
			async notification =>
			{
				await VariableEditCommitedAsync();
			}
		);

		if (string.IsNullOrWhiteSpace(WorkspaceName))
		{
			WorkspaceName = (await WorkspaceService.LastSelectedAsync(default)) ?? "default";
		}

		await WorkspaceService.SelectAsync(WorkspaceName, default);
	}

	private async Task SelectWorkspaceDropdownActionAsync(string menuItemText)
	{
		switch (menuItemText)
		{
			case "Create":
				await CreateWorkspaceAsync();
				break;
			case "Import":
				ImportWorkspace();
				break;
			case "Export":
				await ExportWorkspaceAsync();
				break;
			case "Delete":
				await DeleteWorkspaceAsync();
				break;
		}
	}

	private async Task VariableEditCommitedAsync()
	{
		await _table!.RefreshAsync();
		_expression = WorkspaceService.Workspace.Expression ?? string.Empty;
		_expression2 = WorkspaceService.Workspace.Expression ?? string.Empty;
		await EvaluateAsync();
		StateHasChanged();
	}

	private async Task NameChangedAsync(string newName)
	{
		await WorkspaceService.RenameAsync(newName, default);
	}

	private async Task VariableDropdownItemSelectedAsync(string value)
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
			case "Boolean":
				await AddVariableAsync<bool>();
				break;
			case "DateTime":
				await AddVariableAsync<DateTime>();
				break;
			case "DateTimeOffset":
				await AddVariableAsync<DateTimeOffset>();
				break;
			case "Expression":
				await AddVariableAsync<ExtendedExpression>();
				break;
			case "Delete selected":
				await DeleteSelectedVariablesAsync();
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
	}

	private void ImportWorkspace()
	{
		_showInputFile = true;
		StateHasChanged();
	}

	private async Task ExportWorkspaceAsync()
	{
		var workspaceAsJson = System.Text.Json.JsonSerializer
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
			var workspace = await System.Text.Json.JsonSerializer.DeserializeAsync<Workspace>(memoryStream, DefaultJsonSerializerOptions);
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

	private async Task DeleteSelectedVariablesAsync()
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
		//_enableVariableDelete = _table!.Selection?.Count is not null and > 0;
		StateHasChanged();
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

	private void TableExceptionHandler(Exception e)
	{
		ToastService.Error(e.Message, "Table Exception");
	}

	private async Task OnExpressionChangedAsync(string expression)
	{
		await WorkspaceService.SetExpressionAsync(expression, default);
		_expression = _expression2 = expression;

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

			var inputSet = variables.Items.SingleOrDefault(variables => variables.Name == "inputSet");

			_exceptionMessage = string.Empty;
			_exceptionType = string.Empty;

			if (inputSet is null)
			{
				var output = expression.Evaluate();
				_result = GetString(output);
				_resultType = output switch
				{
					null => "null",
					List<string> => "List<string>",
					List<object?> => "List<object?>",
					_ => output.GetType().ToString()
				};
			}
			else
			{
				switch (inputSet.Type.ToString())
				{
					case "PanoramicData.NCalcExtensions.ExtendedExpression":
						var inputExpression = new ExtendedExpression(inputSet.Value);
						foreach (var variable in variables.Items)
						{
							inputExpression.Parameters[variable.Name] = variable.GetValue();
						}

						var inputExpressionResult = inputExpression.Evaluate();
						var inputExpressionResultType = inputExpressionResult.GetType();
						var inputSetValues = new List<object?>();
						var outputSetValues = new List<object?>();

						if (inputExpressionResultType is IList<object?> x)
						{
							foreach (var item in x)
							{
								inputSetValues.Add(item);
							}

							expression.Parameters["input"] = inputSetValues;
						}
						else if (inputExpressionResultType is IList<string> y)
						{
							foreach (var item in y)
							{
								inputSetValues.Add(item);
							}

							expression.Parameters["input"] = inputSetValues;
						}
						else
						{
							throw new InvalidOperationException($"Unsupported inputSet type after evaluation: {inputExpressionResultType}");
						}

						_resultType = "Result Set";
						foreach (var inputSetValue in inputSetValues)
						{
							expression.Parameters["InputSetValue"] = inputSetValue;
							var output = expression.Evaluate();
							outputSetValues.Add(output);
						}

						_result = string.Join("\r\n", outputSetValues.Select(x => $"{x?.GetType().ToString() ?? "null"} : {x}"));

						break;
					default:
						throw new InvalidOperationException($"Unsupported inputSet type: {inputSet.Type}");
				}
			}
		}
		catch (Exception ex)
		{
			_result = string.Empty;
			_resultType = string.Empty;
			_exceptionMessage = ex.Message;
			_exceptionType = ex.GetType().ToString();
		}
	}

	private static string GetString(object? output, bool isTopLevel = true)
	{
		return output switch
		{
			null => "null",
			JObject jObject => JsonConvert.SerializeObject(jObject, Formatting.Indented),
			JArray jArray => JsonConvert.SerializeObject(jArray, Formatting.Indented),
			IEnumerable<string> => string.Join(isTopLevel ? "\r\n" : " ", output as IEnumerable<string> ?? []),
			IEnumerable<object?> => string.Join(isTopLevel ? "\r\n" : " ", (output as IEnumerable<object?>)!.Select(ob => GetString(ob, false) ?? "null")),
			_ => output.ToString()
		} ?? string.Empty;
	}

	private static string TidyExpression(string expression)
		=> expression
			.Replace('\r', ' ')
			.Replace('\n', ' ');
}