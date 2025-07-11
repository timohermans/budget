namespace Budget.Core.Infrastructure;

public class LoginThrottler(TimeProvider time)
{
    private readonly int _lockoutMax = 5;
    private DateTime _tryAgainTime = time.GetUtcNow().DateTime;
    private int _lockoutCount = 0;

    public void Throttle()
    {
        var now = time.GetUtcNow().DateTime;
        if (_lockoutCount >= _lockoutMax && now < _tryAgainTime)
        {
            throw new InvalidOperationException("Cannot add another lockout when already locked out");
        }

        _lockoutCount++;

        if (_lockoutCount < _lockoutMax)
        {
            _tryAgainTime = now;
        }
        else
        {
            var attemptsTooMuch = _lockoutCount - _lockoutMax + 1; // start with one, not 0
            var secondsToWaitBeforeRetry = Math.Pow(4, attemptsTooMuch);
            _tryAgainTime = now.AddSeconds(secondsToWaitBeforeRetry);
        }
    }

    public int GetSecondsLeftToTryAgain()
    {
        var now = time.GetUtcNow().DateTime;
        if (_lockoutCount < _lockoutMax) return 0;
        var secondsToRetry = (_tryAgainTime - now).TotalSeconds;

        return secondsToRetry < 0 ? 0 : (int)secondsToRetry;
    }

    public void Reset()
    {
        _lockoutCount = 0;
        _tryAgainTime = time.GetUtcNow().DateTime;
    }
}