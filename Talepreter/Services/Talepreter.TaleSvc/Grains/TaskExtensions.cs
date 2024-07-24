namespace Talepreter.TaleSvc.Grains
{
    public static class TaskExtensions
    {
        /// <summary>
        /// try this if orleans OneWay does not work as intended
        /// </summary>
        public static void Forget(this Task task, ILogger logger)
        {
            try
            {
                task.ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion: return;
                        default: logger.LogError($"Fire/forget operation did not ran to completion but {t.Status}"); return;
                    }
                }).Ignore();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Fire/forget operation failed eith exception: {ex.GetType().Name}, {ex.Message}");
            }
        }
    }
}
