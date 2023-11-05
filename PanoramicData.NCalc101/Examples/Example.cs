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
			Explanation = "A simple expression that returns the string 'Hello World'. In NCalc, strings are stored in single quoted.",
			Variables = []
		},
		new()
		{
			Name = "Hello World using a variable",
			Expression = "message",
			Explanation = "The response is returned from a variable called 'message' containing the string 'Hello World'. There is no need to provide single quotes in variables.",
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
			Explanation = "The response is returned by string concatenating different functions using the '+' operator. The list() function creates a list of nullable objects. The itemAtIndex() function takes the nth one, zero indexed - in this case 'Hell' to form the first part of the word 'Hello'.  The variable [a] contains the string 'o ', giving us the word 'Hello' followed by a space.  The substring() function takes 3 letters from the variable [b], starting at position 2 (zero indexed), adding the string 'Wor' and giving us 'Hello Wor'.  The letter 'l' is added, giving us 'Hello Worl'.  Now for the complex one!  The first() function can take a lamda.  Note that the final lambda term contains single quotes, which must be escaped out using the '\\' character. The result is the equivalent of C#: list.First(x => x > 'c') - the first string in the list that is greater than 'c' so the final letter 'd'.",
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
		new()
		{
			Name = "Add a number to a string",
			Expression = "'a' + 1",
			Explanation = "Adding numbers to strings results in a string.  Note that you cannot add strings to numbers.",
			Variables = []
		},
	];
}
