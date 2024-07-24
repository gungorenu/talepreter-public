using Microsoft.Extensions.Logging;
using Talepreter.Contracts.Orleans.System;
using Talepreter.Exceptions;

namespace Talepreter.Operations.Grains
{
    public class ValidationContext
    {
        public static ValidationContext Validate(ILogger logger, string grainName, Func<string> grainIdentifier, string methodName) => new(logger, grainName, grainIdentifier, methodName);

        private readonly string _methodName;
        private readonly string _grainName;
        private readonly Func<string> _grainIdentifier;
        private readonly ILogger _logger;

        private ValidationContext(ILogger logger, string grainName, Func<string> grainIdentifier, string methodName)
        {
            _methodName = methodName;
            _grainName = grainName;
            _grainIdentifier = grainIdentifier;
            _logger = logger;
        }

        public ValidationContext TaleId(Guid value)
        {
            if (value == Guid.Empty) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got null/empty tale id");
            return this;
        }

        public ValidationContext Initialize(Guid value)
        {
            if (value == Guid.Empty) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} is not initialized");
            return this;
        }

        public ValidationContext TaleVersionId(Guid value)
        {
            if (value == Guid.Empty) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got null/empty tale version id");
            return this;
        }

        public ValidationContext Writer(Guid value)
        {
            if (value == Guid.Empty) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got null/empty writer id");
            return this;
        }

        public ValidationContext Chapter(int value)
        {
            if (value < 0) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got negative chapter id");
            return this;
        }

        public ValidationContext Page(int value)
        {
            if (value < 0) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got negative page id");
            return this;
        }

        public ValidationContext Custom(bool condition, string message)
        {
            if (condition) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} {message}");
            return this;
        }

        public ValidationContext IsHealthy(ControllerGrainStatus status, params ControllerGrainStatus[] expected)
        {
            if (!expected.Any(x => x == status)) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} is not in healthy state");
            return this;
        }

        public ValidationContext IsNull(object objectToCheck, string argName)
        {
            if (objectToCheck == null) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got null/empty argument {argName}");
            return this;
        }

        public ValidationContext IsEmpty(string value, string argName)
        {
            if (string.IsNullOrEmpty(value)) throw new GrainOperationException($"[{_grainName}:{_methodName} - Validation] {_grainIdentifier()} got null/empty argument {argName}");
            return this;
        }

        public void Debug(string message)
        {
            _logger.LogDebug($"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }

        public void Information(string message)
        {
            _logger.LogInformation($"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }

        public void Fatal(string message)
        {
            _logger.LogCritical($"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }

        public void Fatal(Exception ex, string message)
        {
            _logger.LogCritical(ex, $"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }

        public void Error(Exception ex, string message)
        {
            _logger.LogError(ex, $"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }

        public void Error(string message)
        {
            _logger.LogError($"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }

        public void Warning(Exception ex, string message)
        {
            _logger.LogWarning(ex, $"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }
        
        public void Warning(string message)
        {
            _logger.LogWarning($"[{_grainName}:{_methodName}] {_grainIdentifier()} {message}");
        }
    }
}
