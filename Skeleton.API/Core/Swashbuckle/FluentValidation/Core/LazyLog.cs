using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Skeleton.API.Core.Swashbuckle.FluentValidation.Core
{
    /// <summary>
    /// Helps to log something only once in some context.
    /// </summary>
    internal class LazyLog(ILogger logger, Action<ILogger> logAction)
    {
        private readonly Lazy<object> _lazyLog = new(() =>
        {
            logAction(logger);
            return new object();
        });

        /// <summary>
        /// Executes log action only once.
        /// </summary>
        public void LogOnce() => IgnoreResult(_lazyLog.Value);

        [MethodImpl(MethodImplOptions.NoInlining)]
        [SuppressMessage("CodeQuality", "IDE0060:Remove unnecessary suppression", Justification = "Ok.")]
        private void IgnoreResult(object obj)
        {
            /* empty body. uses for evaluating input arg. */
        }
    }
}