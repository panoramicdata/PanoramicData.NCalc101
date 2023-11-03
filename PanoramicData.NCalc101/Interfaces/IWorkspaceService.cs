using PanoramicData.Blazor.Interfaces;
using PanoramicData.NCalc101.Models;

namespace PanoramicData.NCalc101.Interfaces;

public interface IWorkspaceService : IDataProviderService<Variable>
{
	public Task<List<string>> GetNamesAsync(CancellationToken cancellationToken);

	public Task SelectAsync(string name, CancellationToken cancellationToken);

	public Workspace Workspace { get; }

	public Task RenameAsync(string name, CancellationToken cancellationToken);

	public Task DeleteAsync(string name, CancellationToken cancellationToken);

	public Task SetExpressionAsync(string expression, CancellationToken cancellationToken);

	public Task CreateWorkspaceAsync(string name, string expression, List<Variable> variables, CancellationToken cancellationToken);

	public Task<string?> LastSelectedAsync(CancellationToken cancellationToken);

	public IReadOnlyList<Variable> Variables { get; }
}
