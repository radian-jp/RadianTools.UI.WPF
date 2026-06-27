using System.IO;
using System.Runtime.CompilerServices;

namespace RadianTools.UI.WPF.Logging;

public enum ErrorLevel
{
    Debug,
    Info,
    Warn,
    Error
}

public class Logger
{
    public static Logger Shared = new Logger();

    public ErrorLevel MinErrorLevel { get; set; } = ErrorLevel.Info;

    public void Debug(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Write(ErrorLevel.Debug, message, memberName, filePath, lineNumber);

    public void Info(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Write(ErrorLevel.Info, message, memberName, filePath, lineNumber);

    public void Warn(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Write(ErrorLevel.Warn, message, memberName, filePath, lineNumber);

    public void Error(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Write(ErrorLevel.Error, message, memberName, filePath, lineNumber);

    private void Write(
        ErrorLevel level,
        string message,
        string memberName,
        string filePath,
        int lineNumber)
    {
        if (level < MinErrorLevel)
            return;

        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var className = Path.GetFileNameWithoutExtension(filePath);
        var output = $"[{time} {level, -5} {className}.{memberName}({lineNumber})] {message}";
        System.Diagnostics.Debug.WriteLine(output);
    }
}