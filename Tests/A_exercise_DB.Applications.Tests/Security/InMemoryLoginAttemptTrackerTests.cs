using A_exercise_DB.Applications.Security;

namespace A_exercise_DB.Applications.Tests.Security;

/// <summary>
/// ログイン失敗回数とロック状態管理のテスト
/// </summary>
[TestClass]
[TestCategory("Security")]
public class InMemoryLoginAttemptTrackerTests
{
    private const string AccountName = "admin01";

    [TestMethod(DisplayName = "初期状態ではロックされていない")]
    public void IsLocked_WhenNoFailures_ShouldReturnFalse()
    {
        var tracker = new InMemoryLoginAttemptTracker();

        var result = tracker.IsLocked(AccountName);

        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "5回目のログイン失敗でロックされる")]
    public void RecordFailure_WhenFailureCountReachesFive_ShouldLockAccount()
    {
        var tracker = new InMemoryLoginAttemptTracker();

        for (var count = 1; count < 5; count++)
        {
            Assert.IsFalse(tracker.RecordFailure(AccountName));
        }

        Assert.IsTrue(tracker.RecordFailure(AccountName));
        Assert.IsTrue(tracker.IsLocked(AccountName));
    }

    [TestMethod(DisplayName = "失敗記録をリセットするとロックが解除される")]
    public void Reset_WhenAccountIsLocked_ShouldClearLockState()
    {
        var tracker = new InMemoryLoginAttemptTracker();

        for (var count = 0; count < 5; count++)
        {
            tracker.RecordFailure(AccountName);
        }

        tracker.Reset(AccountName);

        Assert.IsFalse(tracker.IsLocked(AccountName));
    }

    [TestMethod(DisplayName = "ロックから10分経過するとロックが解除される")]
    public void IsLocked_WhenTenMinutesHavePassed_ShouldReturnFalse()
    {
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 7, 10, 9, 0, 0, TimeSpan.Zero));
        var tracker = new InMemoryLoginAttemptTracker(timeProvider);

        for (var count = 0; count < 5; count++)
        {
            tracker.RecordFailure(AccountName);
        }

        timeProvider.Advance(TimeSpan.FromMinutes(10));

        Assert.IsFalse(tracker.IsLocked(AccountName));
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public ManualTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan elapsed)
        {
            _utcNow = _utcNow.Add(elapsed);
        }
    }
}
