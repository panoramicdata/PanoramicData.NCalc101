
using PanoramicData.NCalc101.Interfaces;

namespace PanoramicData.NCalc101.Services;

public class ToastLoggerService(IToastService toastService) : ILogger
{
	private readonly IToastService _toastService = toastService;
	private const LogLevel MinimumLogLevel = LogLevel.Debug;

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (logLevel < MinimumLogLevel)
		{
			return;
		}

		switch (logLevel)
		{
			case LogLevel.Trace:
				_toastService.Info(formatter(state, exception), "Trace");
				break;
			case LogLevel.Debug:
				_toastService.Info(formatter(state, exception), "Debug");
				break;
			case LogLevel.Information:
				_toastService.Info(formatter(state, exception));
				break;
			case LogLevel.Warning:
				_toastService.Warning(formatter(state, exception));
				break;
			case LogLevel.Error:
			case LogLevel.Critical:
				_toastService.Error(formatter(state, exception));
				break;
			case LogLevel.None:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);

		}
	}
}
