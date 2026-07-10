using System.Collections.Concurrent;

namespace A_exercise_DB.Applications.Security;

/// <summary>
/// ログイン失敗回数とロック状態をメモリ上で管理する
/// </summary>
public sealed class InMemoryLoginAttemptTracker : ILoginAttemptTracker
{
    private const int MaximumFailureCount = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<string, LoginAttemptState> _states =
        new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="timeProvider">現在時刻の取得元</param>
    public InMemoryLoginAttemptTracker(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public bool IsLocked(string accountName)
    {
        if (!_states.TryGetValue(accountName, out var state) ||
            state.LockedUntil is null)
        {
            return false;
        }

        if (state.LockedUntil > _timeProvider.GetUtcNow())
        {
            return true;
        }

        _states.TryRemove(accountName, out _);
        return false;
    }

    /// <inheritdoc />
    public bool RecordFailure(string accountName)
    {
        var now = _timeProvider.GetUtcNow();

        var state = _states.AddOrUpdate(
            accountName,
            _ => new LoginAttemptState(1, null),
            (_, current) => CreateNextState(current, now));

        return state.LockedUntil > now;
    }

    /// <inheritdoc />
    public void Reset(string accountName)
    {
        _states.TryRemove(accountName, out _);
    }

    private static LoginAttemptState CreateNextState(
        LoginAttemptState current,
        DateTimeOffset now)
    {
        if (current.LockedUntil > now)
        {
            return current;
        }

        var failureCount = current.LockedUntil is null
            ? current.FailureCount + 1
            : 1;

        return failureCount >= MaximumFailureCount
            ? new LoginAttemptState(failureCount, now.Add(LockDuration))
            : new LoginAttemptState(failureCount, null);
    }

    private sealed record LoginAttemptState(
        int FailureCount,
        DateTimeOffset? LockedUntil);
}
