using PanoramicData.NCalc101.Interfaces;
using Sotsera.Blazor.Toaster;

namespace PanoramicData.NCalc101.Services;


public class ToastService(IToaster toaster) : IToastService
{
	private readonly IToaster _toaster = toaster;

	private static readonly int _infoTimeOutSeconds = 10;

	private static readonly int _warningTimeOutSeconds = 15;

	private static readonly int _errorTimeOutSeconds = 20;

	public int MaxChars { get; set; } = 500;

	public void Info(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehaviour closeBehaviour = ToastServiceCloseBehaviour.CloseAutomatically)
		=> _toaster.Info(ShortMessage(message), title, options =>
			{
				options.RequireInteraction = closeBehaviour == ToastServiceCloseBehaviour.CloseAutomatically;
				options.EscapeHtml = !allowHtml;
				options.VisibleStateDuration = _infoTimeOutSeconds * 1000;
			}
		);

	public void Success(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehaviour closeBehaviour = ToastServiceCloseBehaviour.CloseAutomatically)
		=> _toaster.Success(ShortMessage(message), title, options =>
			{
				options.RequireInteraction = closeBehaviour == ToastServiceCloseBehaviour.ManualCloseRequired;
				options.EscapeHtml = !allowHtml;
				options.VisibleStateDuration = _infoTimeOutSeconds * 1000;
			}
);

	public void Warning(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehaviour closeBehaviour = ToastServiceCloseBehaviour.CloseAutomatically)
		=> _toaster.Warning(ShortMessage(message), title, options =>
			{
				options.RequireInteraction = closeBehaviour == ToastServiceCloseBehaviour.ManualCloseRequired;
				options.EscapeHtml = !allowHtml;
				options.VisibleStateDuration = _warningTimeOutSeconds * 1000;
			}
		);

	public void Error(
		string message,
		string? title = null,
		bool allowHtml = false,
		ToastServiceCloseBehaviour closeBehaviour = ToastServiceCloseBehaviour.CloseAutomatically)
		=> _toaster.Error(ShortMessage(message), title, options =>
			{
				options.RequireInteraction = closeBehaviour == ToastServiceCloseBehaviour.ManualCloseRequired;
				options.EscapeHtml = !allowHtml;
				options.VisibleStateDuration = _errorTimeOutSeconds * 1000;
			}
		);

	private string ShortMessage(string message)
		=> message == null
			? throw new ArgumentNullException(nameof(message))
			: message.Length <= MaxChars ? message : message[..MaxChars] + "...";
}