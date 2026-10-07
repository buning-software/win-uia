
namespace WinUia;

internal static class Retry
{
    public static T OnStale<T>(Func<T> action, Func<bool> refresh, int maxRetries)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return ComExceptionMapper.Invoke(action);
            }
            catch (UiaStaleElementException) when (attempt < maxRetries)
            {
                // Refresh runs here, not in an exception filter: filters run before inner finally blocks and swallow what they throw.
                if (!refresh())
                    throw;
            }
        }
    }
}
