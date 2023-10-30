namespace PanoramicData.NCalc101.Models;

public class Variable
{
	public required Type Type { get; set; }

	public required string Name { get; set; } = string.Empty;

	public required string ValueAsString { get; set; }

	public object? Value => Type switch
	{
		Type t when t == typeof(bool) => bool.Parse(ValueAsString),
		Type t when t == typeof(byte) => byte.Parse(ValueAsString),
		Type t when t == typeof(char) => char.Parse(ValueAsString),
		Type t when t == typeof(DateTime) => DateTime.Parse(ValueAsString),
		Type t when t == typeof(DateTimeOffset) => DateTimeOffset.Parse(ValueAsString),
		Type t when t == typeof(decimal) => decimal.Parse(ValueAsString),
		Type t when t == typeof(double) => double.Parse(ValueAsString),
		Type t when t == typeof(float) => float.Parse(ValueAsString),
		Type t when t == typeof(int) => int.Parse(ValueAsString),
		Type t when t == typeof(long) => long.Parse(ValueAsString),
		Type t when t == typeof(sbyte) => sbyte.Parse(ValueAsString),
		Type t when t == typeof(short) => short.Parse(ValueAsString),
		Type t when t == typeof(string) => ValueAsString,
		Type t when t == typeof(uint) => uint.Parse(ValueAsString),
		Type t when t == typeof(ulong) => ulong.Parse(ValueAsString),
		Type t when t == typeof(ushort) => ushort.Parse(ValueAsString),
		_ => throw new InvalidOperationException($"Unsupported type: {Type}"),
	};


	public string FontAwesome => Type switch
	{
		Type t when t == typeof(bool) => "fa-solid fa-ban",
		Type t when t == typeof(byte) => "fa-solid fa-ban",
		Type t when t == typeof(char) => "fa-solid fa-ban",
		Type t when t == typeof(DateTime) => "fa-solid fa-clock",
		Type t when t == typeof(DateTimeOffset) => "fa-regular fa-clock",
		Type t when t == typeof(decimal) => "fa-solid fa-ban",
		Type t when t == typeof(double) => "fa-solid fa-temperature-half",
		Type t when t == typeof(float) => "fa-solid fa-ban",
		Type t when t == typeof(int) => "fa-solid fa-1",
		Type t when t == typeof(long) => "fa-solid fa-ban",
		Type t when t == typeof(sbyte) => "fa-solid fa-ban",
		Type t when t == typeof(short) => "fa-solid fa-ban",
		Type t when t == typeof(string) => "fa-solid fa-a",
		Type t when t == typeof(uint) => "fa-solid fa-ban",
		Type t when t == typeof(ulong) => "fa-solid fa-ban",
		Type t when t == typeof(ushort) => "fa-solid fa-ban",
		_ => "fa-solid fa-ban",
	};
}
