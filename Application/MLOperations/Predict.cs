using Domain.SentimentAnalysis;
using MediatR;
using DeepServices;

namespace Application.MLOperations
{
    // TODO: Restrict T to model inputs
    public class Predict<T, S> where T : class, new() where S : class, new()
    {
        public class Command : IRequest<S>
        {
            public T ModelInput { get; set; }
        }

        public class Handler : IRequestHandler<Command, S>
        {
            private readonly IPredictionService<T, S> _predictionService;
            public Handler(IPredictionService<T, S> predictionService)
            {
                _predictionService = predictionService;
            }
            public async Task<S> Handle(Command request, CancellationToken cancellationToken)
            {
                return await _predictionService.PredictSingleDataPoint(request.ModelInput);
            }

        }

    }

    //public class Predict
    //{
    //    public class Command : IRequest<SentimentAnalysisModelOutput>
    //    {
    //        public SentimentAnalysisModelInput ModelInput { get; set; }
    //    }

    //    public class Handler : IRequestHandler<Command, SentimentAnalysisModelOutput>
    //    {
    //        private readonly IPredictionService<SentimentAnalysisModelInput, SentimentAnalysisModelOutput> _predictionService;
    //        public Handler(IPredictionService<SentimentAnalysisModelInput, SentimentAnalysisModelOutput> predictionService)
    //        {
    //            _predictionService = predictionService;
    //        }
    //        public async Task<SentimentAnalysisModelOutput> Handle(Command request, CancellationToken cancellationToken)
    //        {
    //            return await _predictionService.PredictSingleDataPoint(request.ModelInput);
    //        }

    //    }

    //}

}
