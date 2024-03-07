using DeepServices;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.MLOperations
{
    // TODO: Restrict T to model inputs
    public class PredictFromJSON<T, S> where T : class, new() where S : class, new()
    {
        public class Command : IRequest<S>
        {
            public T ModelInput { get; set; }
        }

        public class Handler : IRequestHandler<Command, S>
        {
            private readonly IPredictionService<T, S> _predictionService;
            private IHttpContextAccessor _httpContextAccessor;

            public Handler(IPredictionService<T, S> predictionService, IHttpContextAccessor httpContextAccessor)
            {
                _predictionService = predictionService;
                _httpContextAccessor = httpContextAccessor;
            }
            public async Task<S> Handle(Command request, CancellationToken cancellationToken)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var sessionId = httpContext.Session.Id;

                return await _predictionService.PredictSingleDataPointAsync(sessionId, request.ModelInput);
            }

        }

    }
}
