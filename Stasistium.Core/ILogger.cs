using System;

namespace Stasistium.Documents
{
    public interface ILogger
    {
        internal ILogger WithName(string name)
        {
            if (this is Logger baseLogger)
                return new LoggerWrapper(baseLogger, name);
            if (this is LoggerWrapper wrapperLogger)
                return new LoggerWrapper(wrapperLogger.BaseLogger, name);
            throw new NotSupportedException("This Logger is not supported with name.");
        }

        IDisposable Indent();
        void Info(string text);

        void Error(string text);
        void Verbose(string text);
    }
}