using PanoramicData.NCalcExtensions;
using System.Globalization;

namespace PanoramicData.NCalc101.Models;

public class Variable
{
	public string Id { get; set; } = Guid.NewGuid().ToString();

	public string Type { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public string Value { get; set; } = string.Empty;

	internal object? GetValue() => Type switch
	{
		"System.Boolean" => bool.Parse(Value),
		"System.Byte" => byte.Parse(Value, CultureInfo.InvariantCulture),
		"System.Char" => char.Parse(Value),
		"System.DateTime" => DateTime.Parse(Value, CultureInfo.InvariantCulture),
		"System.DateTimeOffset" => DateTimeOffset.Parse(Value, CultureInfo.InvariantCulture),
		"System.Decimal" => decimal.Parse(Value, CultureInfo.InvariantCulture),
		"System.Double" => double.Parse(Value, CultureInfo.InvariantCulture),
		"System.Single" => float.Parse(Value, CultureInfo.InvariantCulture),
		"System.Int32" => int.Parse(Value, CultureInfo.InvariantCulture),
		"System.Int64" => long.Parse(Value, CultureInfo.InvariantCulture),
		"System.SByte" => sbyte.Parse(Value, CultureInfo.InvariantCulture),
		"System.Int16" => short.Parse(Value, CultureInfo.InvariantCulture),
		"PanoramicData.NCalcExtensions.ExtendedExpression" => new ExtendedExpression(Value),
		"System.String" => Value,
		"null" => null,
		_ => throw new InvalidOperationException($"Unsupported type: {Type}"),
	};

	internal object? GetFontAwesome() => "fa-fw " + Type switch
	{
		"System.Boolean" => "fa-solid fa-toggle-on",
		"System.DateTime" => "fa-solid fa-clock",
		"System.DateTimeOffset" => "fa-regular fa-clock",
		"System.Double" => "fa-solid fa-temperature-half",
		"System.Int32" => "fa-solid fa-1",
		"System.String" => "fa-solid fa-a",
		"PanoramicData.NCalcExtensions.ExtendedExpression" => "fa-solid fa-calculator",
		_ => "fa-solid fa-ban",
	};
}
