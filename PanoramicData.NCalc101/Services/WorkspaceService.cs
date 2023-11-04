using Blazored.LocalStorage;
using PanoramicData.Blazor.Models;
using PanoramicData.NCalc101.Interfaces;
using PanoramicData.NCalc101.Models;

namespace PanoramicData.NCalc101.Services;

public class WorkspaceService(
	ILocalStorageService localStorageService,
	ILogger logger) : IWorkspaceService
{
	private const string LocalStorageWorkspacePrefix = "workspace_";
	private const string LocalStorageLastSelectedStorageKey = "lastSelected";
	private readonly ILocalStorageService localStorageService = localStorageService;
	private readonly ILogger logger = logger;

	public async Task<List<string>> GetNamesAsync(CancellationToken cancellationToken)
	{
		var workspaceNames = (await localStorageService.KeysAsync(cancellationToken))
			.Where(k => k.StartsWith(LocalStorageWorkspacePrefix))
			.Select(k => k[LocalStorageWorkspacePrefix.Length..])
			.ToList() ?? [];

		if (workspaceNames.Count == 0)
		{
			await SelectAsync("default", cancellationToken);
			workspaceNames.Add("default");
		}

		return workspaceNames;
	}

	public async Task SelectAsync(string name, CancellationToken cancellationToken)
	{
		var key = LocalStorageName(name);
		if (await localStorageService.ContainKeyAsync(key, cancellationToken))
		{
			Workspace = await localStorageService.GetItemAsync<Workspace>(key, default);

			await localStorageService.SetItemAsync(LocalStorageLastSelectedStorageKey, name, cancellationToken);
		}
		else
		{
			Workspace = new Workspace
			{
				Name = name,
				Expression = "1 + 1"
			};

			await localStorageService.SetItemAsync(key, Workspace, cancellationToken);
		}
	}

	public Workspace Workspace { get; private set; } = new Workspace
	{
		Name = "default",
		Expression = "1 + 1"
	};

	public async Task DeleteAsync(string name, CancellationToken cancellationToken)
	{
		if (name == "default")
		{
			throw new ArgumentOutOfRangeException(nameof(name), "Cannot delete default workspace.");
		}

		try
		{
			await localStorageService.RemoveItemAsync(LocalStorageName(name), cancellationToken);
			if (await LastSelectedAsync(default) == name)
			{
				await localStorageService.RemoveItemAsync(LocalStorageLastSelectedStorageKey, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{Message}", ex.Message);
		}
	}

	public IReadOnlyList<Variable> Variables => Workspace.Variables.AsReadOnly();

	public async Task<OperationResponse> CreateAsync(Variable item, CancellationToken cancellationToken)
	{
		try
		{
			var existing = Workspace.Variables.FirstOrDefault(v => v.Id == item.Id);
			if (existing != null)
			{
				return new OperationResponse
				{
					Success = false,
					ErrorMessage = $"Variable with id '{item.Id}' already exists."
				};
			}

			Workspace.Variables.Add(item);

			await UpdateLocalStorageAsync(cancellationToken);

			return new OperationResponse
			{
				Success = true
			};
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{Message}", ex.Message);

			return new OperationResponse
			{
				Success = false,
				ErrorMessage = $"Failed to create variable: {ex.Message}"
			};
		}
	}

	private ValueTask UpdateLocalStorageAsync(CancellationToken cancellationToken)
	{
		return localStorageService.SetItemAsync(LocalStorageName(Workspace.Name), Workspace, cancellationToken);
	}

	private static string LocalStorageName(string name) => $"{LocalStorageWorkspacePrefix}{name}";

	public async Task<OperationResponse> DeleteAsync(Variable item, CancellationToken cancellationToken)
	{
		try
		{
			var existing = Workspace.Variables.FirstOrDefault(v => v.Id == item.Id);
			if (existing is null)
			{
				return new OperationResponse
				{
					Success = false,
					ErrorMessage = $"Variable with id '{item.Id}' does not exist."
				};
			}

			Workspace.Variables.Remove(existing);


			await UpdateLocalStorageAsync(cancellationToken);

			return new OperationResponse
			{
				Success = true
			};
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{Message}", ex.Message);

			return new OperationResponse
			{
				Success = false,
				ErrorMessage = $"Failed to delete variable: {ex.Message}"
			};
		}
	}

	public Task<DataResponse<Variable>> GetDataAsync(DataRequest<Variable> request, CancellationToken cancellationToken)
		=> Task.FromResult(new DataResponse<Variable>(Workspace.Variables, Workspace.Variables.Count));

	public async Task<OperationResponse> UpdateAsync(Variable item, IDictionary<string, object?> delta, CancellationToken cancellationToken)
	{
		try
		{
			var existing = Workspace.Variables.FirstOrDefault(v => v.Id == item.Id);
			if (existing is null)
			{
				return new OperationResponse
				{
					Success = false,
					ErrorMessage = $"Variable with id '{item.Id}' does not exist."
				};
			}

			existing.Value = item.Value;

			await UpdateLocalStorageAsync(cancellationToken);

			return new OperationResponse
			{
				Success = true
			};
		}
		catch (Exception e)
		{
			logger.LogError(e, "{Message}", e.Message);
			return new OperationResponse
			{
				Success = false,
				ErrorMessage = $"Failed to update variable: {e.Message}"
			};
		}
	}

	public async Task SetExpressionAsync(string expression, CancellationToken cancellationToken)
	{
		Workspace.Expression = expression;
		await UpdateLocalStorageAsync(cancellationToken);
	}

	public async Task CreateWorkspaceAsync(string name, string expression, List<Variable> variables, CancellationToken cancellationToken)
	{
		Workspace = new Workspace
		{
			Name = name,
			Expression = expression,
			Variables = variables
		};

		await UpdateLocalStorageAsync(cancellationToken);
	}

	public async Task<string?> LastSelectedAsync(CancellationToken cancellationToken)
	{
		return await localStorageService
			.GetItemAsync<string>(LocalStorageLastSelectedStorageKey, cancellationToken);
	}

	public async Task RenameAsync(string name, CancellationToken cancellationToken)
	{
		var oldName = Workspace.Name;
		Workspace = new Workspace
		{
			Name = name,
			Expression = Workspace.Expression,
			Variables = Workspace.Variables
		};

		await DeleteAsync(oldName, cancellationToken);

		await UpdateLocalStorageAsync(cancellationToken);
	}
}