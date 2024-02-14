using Application.Core;
using AutoMapper;
using DeepServices;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Persistence;
using System.Formats.Asn1;

namespace Application.DataIngestion
{
    public class IngestFileFromForm<T, S> where T : class, new() where S : class, new()
    {
        public class Command : IRequest<Result<S>>
        {
            public DataFromForm FormData { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<S>>
        {
            private readonly IMapper _mapper;
            private readonly IPredictionService<T, S> _predictionService;

            public Handler(IMapper mapper, IPredictionService<T, S> predictionService)
            {
                _mapper = mapper;
                _predictionService = predictionService;
            }
            public async Task<Result<S>> Handle(Command request, CancellationToken cancellationToken)
            {
                var image = request.FormData.Image;

                // TODO: Input validation
                string[] validExtensions = { ".jpg", ".png" };
                var extension = Path.GetExtension(image.FileName);
                if (!validExtensions.Contains(extension)) {
                    return Result<S>.Failure("Can only accept .jpg or .png");
                }

                // Call the service that takes the raw file and returns a ModelInput (T)
                if (image == null)
                {
                    return Result<S>.Failure("No file attached");
                }

                var pred = await _predictionService.PredictSingleDataPointFromForm(request.FormData);

                return Result<S>.Success(pred);
            }

        }
    }
}
