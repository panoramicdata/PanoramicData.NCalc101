namespace PanoramicData.NCalc101.Models;

public class Variable(string name, string type, object? @object)
{
	[field: NonSerialized]
	public Guid Id { get; } = Guid.NewGuid();

	public string Type { get; } = type;

	public string Name { get; set; } = name;

	public string Value { get; set; } = @object?.ToString() ?? string.Empty;

	internal object? GetValue() => Type switch
	{
		"System.Boolean" => bool.Parse(Value),
		"System.Byte" => byte.Parse(Value),
		"System.Char" => char.Parse(Value),
		"System.DateTime" => DateTime.Parse(Value),
		"System.DateTimeOffset" => DateTimeOffset.Parse(Value),
		"System.Decimal" => decimal.Parse(Value),
		"System.Double" => double.Parse(Value),
		"System.Single" => float.Parse(Value),
		"System.Int32" => int.Parse(Value),
		"System.Int64" => long.Parse(Value),
		"System.SByte" => sbyte.Parse(Value),
		"System.Int16" => short.Parse(Value),
		"System.String" => Value,
		"null" => null,
		_ => throw new InvalidOperationException($"Unsupported type: {Type}"),
	};

	internal object? GetFontAwesome() => Type switch
	{
		"System.Boolean" => "fa-solid fa-toggle-on",
		"System.DateTime" => "fa-solid fa-clock",
		"System.DateTimeOffset" => "fa-regular fa-clock",
		"System.Double" => "fa-solid fa-temperature-half",
		"System.Int32" => "fa-solid fa-1",
		"System.String" => "fa-solid fa-a",
		_ => "fa-solid fa-ban",
	};
}
