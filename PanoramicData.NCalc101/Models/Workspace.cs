namespace PanoramicData.NCalc101.Models;

public class Workspace
{
	public string Name { get; set; } = string.Empty;

	public string Expression { get; set; } = string.Empty;

	public List<Variable> Variables { get; set; } = [];
}
