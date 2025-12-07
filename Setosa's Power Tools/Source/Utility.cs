using System;

namespace Setosa;

public static class Logger {
	internal static bool Enabled =
#if DEBUG
		true;
#else
		false;
#endif

	private const string LogPrefix = $"[{ThisAssembly.Info.Title}] ";

	public static void Message(string text) => Log(Verse.Log.Message, text);

	public static void Warning(string text, bool once = false) {
		if (once)
			LogOnce(Verse.Log.WarningOnce, text);
		else
			Log(Verse.Log.Warning, text);
    }

	public static void Error(string text, bool once = false) {
		if (once)
			LogOnce(Verse.Log.ErrorOnce, text);
		else
			Log(Verse.Log.Error, text);
    }

    private static void Log(Action<string> method, string text) {
		if (Enabled)
			method(LogPrefix + text);
    }

	private static void LogOnce(Action<string, int> method, string text) {
		if (Enabled)
			method(LogPrefix + text, text.GetHashCode());
	}
}
