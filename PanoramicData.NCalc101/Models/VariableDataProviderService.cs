using Newtonsoft.Json;
using PanoramicData.Blazor.Interfaces;
using PanoramicData.Blazor.Models;
using System.Web;

namespace PanoramicData.NCalc101.Models
{
	public class VariableDataProviderService : IDataProviderService<Variable>
	{
		private readonly List<Variable> _variables = [];

		public VariableDataProviderService(string? httpEncodedJsonString)
		{
			// If we have a JSON string, then decode it and use it to populate _variables.
			if (!string.IsNullOrWhiteSpace(httpEncodedJsonString))
			{
				var jsonString = HttpUtility.UrlDecode(httpEncodedJsonString);
				_variables = JsonConvert.DeserializeObject<List<Variable>>(jsonString) ?? new();
			}
		}

		/// <summary>
		/// The contents of _variables as an HTTP-encoded JSON string.
		/// </summary>
		public string HttpEncodedVariables => HttpUtility.UrlEncode(JsonConvert.SerializeObject(_variables));

		public Task<OperationResponse> CreateAsync(Variable item, CancellationToken cancellationToken)
		{
			var existing = _variables.FirstOrDefault(v => v.Name == item.Name);
			if (existing != null)
			{
				return Task.FromResult(new OperationResponse
				{
					Success = false,
					ErrorMessage = $"Variable with name '{item.Name}' already exists."
				});
			}

			_variables.Add(item);

			return Task.FromResult(new OperationResponse
			{
				Success = true
			});
		}

		public Task<OperationResponse> DeleteAsync(Variable item, CancellationToken cancellationToken)
		{
			var existing = _variables.FirstOrDefault(v => v.Name == item.Name);
			if (existing is null)
			{
				return Task.FromResult(new OperationResponse
				{
					Success = false,
					ErrorMessage = $"Variable with name '{item.Name}' does not exist."
				});
			}

			_variables.Remove(existing);

			return Task.FromResult(new OperationResponse
			{
				Success = true
			});
		}

		public Task<DataResponse<Variable>> GetDataAsync(DataRequest<Variable> request, CancellationToken cancellationToken)
		{
			var response = new DataResponse<Variable>(_variables, _variables.Count);
			return Task.FromResult(response);
		}

		public Task<OperationResponse> UpdateAsync(Variable item, IDictionary<string, object?> delta, CancellationToken cancellationToken)
		{
			var existing = _variables.FirstOrDefault(v => v.Name == item.Name);
			if (existing is null)
			{
				return Task.FromResult(new OperationResponse
				{
					Success = false,
					ErrorMessage = $"Variable with name '{item.Name}' does not exist."
				});
			}

			existing.ValueAsString = item.ValueAsString;

			return Task.FromResult(new OperationResponse
			{
				Success = true
			});
		}
	}
}
