namespace PanoramicData.NCalc101.Interfaces;


public interface IToastService
{
	void Info(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehavior closeBehaviour = ToastServiceCloseBehavior.CloseAutomatically);

	void Success(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehavior closeBehaviour = ToastServiceCloseBehavior.CloseAutomatically);

	void Warning(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehavior closeBehaviour = ToastServiceCloseBehavior.CloseAutomatically);

	void Error(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehavior closeBehaviour = ToastServiceCloseBehavior.CloseAutomatically);
}
