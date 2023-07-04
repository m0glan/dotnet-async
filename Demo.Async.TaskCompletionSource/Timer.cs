using Timers = System.Timers;
using Tasks = System.Threading.Tasks;

namespace Demo.Async.TaskCompletionSource;

public static class Timer
{
    /// <summary>
    ///     Invokes the parameter action after a specified time span.
    /// </summary>
    /// <param name="a">The action to be invoked at the timeout.</param>
    /// <param name="timeout">The timeout after which the parameter action is invoked.</param>
    public static void SetTimeout(Action a, TimeSpan timeout)
    {
        var timer = CreateInner(timeout);

        void OnElapsed(object? sender, Timers.ElapsedEventArgs e)
        {
            a();
            timer.Elapsed -= OnElapsed;
            timer.Stop();
            timer.Dispose();
        }

        timer.Elapsed += OnElapsed;
        timer.Start();
    }

    /// <summary>
    ///     Creates a delay for the specified time span using <see cref="SetTimeout(Action, TimeSpan)"/>. 
    ///     This is a naive approach since it wastes an extra thread to wait for the timeout to finish.
    /// </summary>
    public static Task DelayNaivelyAsync(TimeSpan delay)
    {
        bool isElapsed = false;
        SetTimeout(() => isElapsed = true, delay);
        return Task.Run(() => SpinWait.SpinUntil(() => isElapsed));
    }

    /// <summary>
    ///     The proper way to create a delay using <see cref="SetTimeout(Action, TimeSpan)"/> and
    ///     <see cref="Tasks.TaskCompletionSource"/>: the task gets completed after the timeout is
    ///     elapsed and no extra thread is wasted.
    /// </summary>
    /// <returns>
    ///     The task representing the delay.
    /// </returns>
    public static Task DelayAsync(TimeSpan delay)
    {
        Tasks.TaskCompletionSource taskCompletionSource = new();
        SetTimeout(() =>
        {
            taskCompletionSource.SetResult();
        }, delay);
        return taskCompletionSource.Task;
    }

    private static Timers.Timer CreateInner(TimeSpan timeSpan)
    {
        return new Timers.Timer
        {
            Interval = timeSpan.TotalMilliseconds,
            AutoReset = false
        };
    }
}
