using System.Threading;
using System.Threading.Tasks;

namespace FileSearcherMVVM.Models
{
    public class PauseTokenSource
    {
        private volatile TaskCompletionSource<bool> pauser;
        internal static readonly Task completedTask = Task.FromResult(true);

        public bool IsPaused
        {
            get => pauser != null;
            set
            {
                if (value)
                {
                    Interlocked.CompareExchange(ref pauser, new TaskCompletionSource<bool>(), null);
                }
                else
                {
                    while (true)
                    {
                        TaskCompletionSource<bool> tcs = pauser;

                        if (tcs == null)
                        {
                            return;
                        }

                        if (Interlocked.CompareExchange(ref pauser, null, tcs) == tcs)
                        {
                            tcs.SetResult(true);
                            break;
                        }
                    }
                }
            }

        }
        public PauseToken Token => new PauseToken(this);

        internal Task WaitWhilePausedAsync()
        {
            TaskCompletionSource<bool> cur = pauser;
            return cur != null ? cur.Task : completedTask;
        }
    }

    public struct PauseToken
    {
        private readonly PauseTokenSource m_source;
        internal PauseToken(PauseTokenSource source) { m_source = source; }

        public bool IsPaused => m_source != null && m_source.IsPaused;

        public Task WaitWhilePausedAsync()
        {
            return IsPaused ?
                m_source.WaitWhilePausedAsync() :
                PauseTokenSource.completedTask;
        }
    }
}
