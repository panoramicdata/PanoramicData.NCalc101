using Newtonsoft.Json;
using PanoramicData.NCalc101.Models;
using PanoramicData.NCalcExtensions;
using System.Web;

namespace PanoramicData.NCalc101.Examples;

public record Example
{
	private bool _hasEvaluated = false;
	private object? _result;
	private string _resultTypeString = string.Empty;
	private string _resultAsString = string.Empty;
	private Exception? _exception;

	public required string Name { get; set; }

	public required string Expression { get; set; }

	public required string Explanation { get; set; }

	public Uri Uri => new($"/?expression={Uri.EscapeDataString(Expression)}&variables={HttpUtility.UrlEncode(JsonConvert.SerializeObject(Variables))}", UriKind.Relative);

	public required List<Variable> Variables { get; set; }

	public string GetId() => Name.Replace(" ", "_");

	public string GetEvaluationString()
	{
		EvaluateIfRequired();

		return _resultAsString;
	}
	public string GetEvaluationType()
	{
		EvaluateIfRequired();

		return _resultTypeString;
	}

	private void EvaluateIfRequired()
	{
		if (_hasEvaluated)
		{
			return;
		}

		try
		{
			var expression = new ExtendedExpression(Expression);
			foreach (var variable in Variables)
			{
				expression.Parameters[variable.Name] = variable.GetValue();
			}

			_result = expression.Evaluate();
			_resultTypeString = _result is null ? "null" : _result.GetType().ToString();
			_resultAsString = _result?.ToString() ?? string.Empty;
		}
		catch (Exception ex)
		{
			_exception = ex;
			_resultTypeString = ex.GetType().ToString();
			_resultAsString = $"Exception thrown of type {ex.GetType()} with message: '{ex.Message}'.";
		}
		finally
		{
			_hasEvaluated = true;
		}
	}

	public static List<Example> Examples { get; } =
	[
		new()
		{
			Name = "Hello World",
			Expression = "'Hello World'",
			Explanation = "A simple expression that returns the string 'Hello World'",
			Variables = []
		},
		new()
		{
			Name = "Hello World using a variable",
			Expression = "message",
			Explanation = "The response is returned from a variable called 'message' containing the string 'Hello World'.",
			Variables = [
				new()
				{
					Name = "message",
					Type = "System.String",
					Value = "Hello World"
				}
			]
		},
		new()
		{
			Name = "Hello World using many different capabilities",
			Expression = "itemAtIndex(list('Heaven', 'Hell', 'Purgatory'), 1) + a + substring(b, 2, 3) + 'l' + first(list('a', 'b', 'c', 'd'), 'x', 'x > \\'c\\'')",
			Explanation = "The response is returned by string concatenating different functions using the '+' operator.",
			Variables = [
				new()
				{
					Name = "a",
					Type = "System.String",
					Value = "o "
				},
				new()
				{
					Name = "b",
					Type = "System.String",
					Value = "xJWorYvVs"
				},
			]
		},
		new()
		{
			Name = "Integer maths",
			Expression = "1 + 1",
			Explanation = "Adding two 32-bit integers together results in another 32-bit integer.",
			Variables = []
		},
		new()
		{
			Name = "Floating point maths",
			Expression = "1 + 1.1",
			Explanation = "Adding 32-bit integers to a double-precision floating point number results in a double-precision floating point number.",
			Variables = []
		},
	];
}
