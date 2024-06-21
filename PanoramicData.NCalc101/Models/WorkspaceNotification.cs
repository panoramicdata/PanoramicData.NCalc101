namespace PanoramicData.NCalc101.Models;

public record WorkspaceNotification
{
	public required NotificationType Type { get; init; }

	public required string Message { get; init; } = string.Empty;
}
