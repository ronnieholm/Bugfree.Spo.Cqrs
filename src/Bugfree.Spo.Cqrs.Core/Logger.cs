using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Console;

namespace Bugfree.Spo.Cqrs.Core
{
    public interface ILogger
    {
        void Verbose(string format, params object[] args);
        void Warning(string format, params object[] args);
        void Error(string format, params object[] args);
    }

    public class TraceLogger : ILogger
    {
        protected enum LogLevel
        {
            None = 0,
            Verbose,
            Warning,
            Error
        };

        [MethodImpl(MethodImplOptions.NoInlining)] public virtual void Verbose(string format, params object[] args) => WriteLine(format, LogLevel.Verbose, args);
        [MethodImpl(MethodImplOptions.NoInlining)] public virtual void Warning(string format, params object[] args) => WriteLine(format, LogLevel.Warning, args);
        [MethodImpl(MethodImplOptions.NoInlining)] public virtual void Error(string format, params object[] args) => WriteLine(format, LogLevel.Error, args);
        [MethodImpl(MethodImplOptions.NoInlining)] private void WriteLine(string format, LogLevel l, params object[] args) => Trace.WriteLine(Format(format, l, args));
        [MethodImpl(MethodImplOptions.NoInlining)] protected string Format(string format, LogLevel l, params object[] args)
        {
            var frame = new StackFrame(3);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            return $"{DateTime.Now.ToUniversalTime()} {l} {type}.{name} {string.Format(format, args)}";
        }
    }

    public class TextWriterLogger : TraceLogger
    {
        readonly TextWriter _writer;

        public TextWriterLogger(TextWriter tw) => _writer = tw;

        [MethodImpl(MethodImplOptions.NoInlining)] public override void Verbose(string format, params object[] args) => WriteLine(format, LogLevel.Verbose, args);
        [MethodImpl(MethodImplOptions.NoInlining)] public override void Warning(string format, params object[] args) => WriteLine(format, LogLevel.Warning, args);
        [MethodImpl(MethodImplOptions.NoInlining)] public override void Error(string format, params object[] args) => WriteLine(format, LogLevel.Error, args);
        [MethodImpl(MethodImplOptions.NoInlining)] private void WriteLine(string format, LogLevel l, params object[] args) => _writer.WriteLine(Format(format, l, args));
    }

    public class ColoredConsoleLogger : TraceLogger
    {
        [MethodImpl(MethodImplOptions.NoInlining)] public override void Verbose(string format, params object[] args) => WriteLine(format, LogLevel.Verbose, ForegroundColor, args);
        [MethodImpl(MethodImplOptions.NoInlining)] public override void Warning(string format, params object[] args) => WriteLine(format, LogLevel.Warning, ConsoleColor.Yellow, args);
        [MethodImpl(MethodImplOptions.NoInlining)] public override void Error(string format, params object[] args) => WriteLine(format, LogLevel.Error, ConsoleColor.Red);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteLine(string s, LogLevel l, ConsoleColor foregroundColor, params object[] args)
        {
            var original = ForegroundColor;
            var logLine = Format(s, l, args);

            Trace.Listeners
                .Cast<TraceListener>()
                .Where(tl => tl.GetType() != typeof(ConsoleTraceListener))
                .ToList()
                .ForEach(tl =>
                {
                    tl.WriteLine(logLine);
                    tl.Flush();
                });

            switch (l)
            {
                case LogLevel.Verbose:
                    ForegroundColor = foregroundColor;
                    break;
                case LogLevel.Warning:
                    ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentException($"Unsupported log level: {l}");
            }

            Console.WriteLine(logLine);
            ForegroundColor = original;
        }
    }
}