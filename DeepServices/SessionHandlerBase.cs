using Microsoft.AspNetCore.Http;
using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using Tensorflow;

namespace DeepServices
{
    internal class SessionData
    {
        public SessionData()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
            KeepAlive = true;
        }

        private SessionData(DateTime created, DateTime lastActive, bool keepAlive)
        {
            Created = created;
            LastActive = lastActive;
            KeepAlive = keepAlive;
        }

        public DateTime Created { get; }
        public DateTime LastActive { get; set; }
        public bool KeepAlive { get; set; } // In case a session can be cleaned up early

        public static SessionData Update(SessionData oldData)
        {
            return new SessionData(oldData.Created, DateTime.Now, oldData.KeepAlive);
        }

    }

    public abstract class SessionHandlerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Timer _cleanupTimer;
        private readonly ConcurrentDictionary<string, SessionData> _sessionState = new ConcurrentDictionary<string, SessionData>();
        private readonly TimeSpan _timeout;
        protected SessionHandlerBase(IHttpContextAccessor httpContextAccessor, TimeSpan timeout, TimeSpan cleanupInterval)
        {
            _httpContextAccessor = httpContextAccessor;
            _timeout = timeout;

            // Initialize and start the cleanup timer
            _cleanupTimer = new Timer(async _ => await CleanupExpiredSessionsAsync(), null, TimeSpan.Zero, cleanupInterval);
        }

        protected abstract Task OnNewSessionAsync(string sessionId);

        protected abstract Task OnSessionExpiredAsync(string sessionId);

        protected async Task UpdateSessionAsync(string? sessionId = null, bool initializeIfNew = false)
        {
            sessionId ??= _httpContextAccessor.HttpContext.Session.Id;

            if (initializeIfNew)
            {
                await InitializeSessionIfNeededAsync(sessionId);
                return;
            }

            await UpdateSessionTimeAsync(sessionId);
        }

        protected async Task InitializeSessionIfNeededAsync(string? sessionId)
        {
            sessionId ??= _httpContextAccessor.HttpContext.Session.Id;

            // Check if session has already started
            if (!_sessionState.ContainsKey(sessionId))
            {
                // If session hasn't started, trigger OnNewSessionAsync
                await OnNewSessionAsync(sessionId);

                // This will create a new session
                await UpdateSessionTimeAsync(sessionId);
            }
        }

        protected async Task UpdateSessionTimeAsync(string sessionId)
        {
            var sessionExists = _sessionState.TryGetValue(sessionId, out var session);

            if (!sessionExists)

            _sessionState.AddOrUpdate(sessionId, new SessionData(), (_, session) => SessionData.Update(session));
        }

        private async Task CleanupExpiredSessionsAsync()
        {
            // Logic to check and clean up expired sessions.
            foreach (var sessionId in _sessionState.Keys)
            {
                if (_sessionState.TryGetValue(sessionId, out SessionData sessionData))
                {
                    if (DateTime.Now - sessionData.LastActive > _timeout)
                    {
                        // Perform some operation for the idle session
                        await OnSessionExpiredAsync(sessionId);
                    }
                }
            }
        }

    }


}
