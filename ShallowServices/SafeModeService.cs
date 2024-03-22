using Microsoft.Extensions.Options;

namespace ShallowServices
{
    public class SecurityOptions
    {
        public bool SafeMode { get; set; }
    }

    public class SafeModeService
    {
        private readonly bool _isInSafeMode;

        public SafeModeService(IOptions<SecurityOptions> securityOptions)
        {
            _isInSafeMode = securityOptions.Value.SafeMode;
        }

        public bool IsInSafeMode { get => _isInSafeMode; }

    }
}
