using PanoramicData.NCalc101.Models;

namespace PanoramicData.NCalc101.Interfaces;

public interface INotificationService<T>
{
	public void Subscribe(NotificationType notificationType, Action<T> callback);

	public Task NotifyAsync(T notification, CancellationToken cancellationToken);
}
