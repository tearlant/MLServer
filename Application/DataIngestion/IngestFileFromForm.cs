using Application.Core;
using AutoMapper;
using DeepServices;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.DataIngestion
{
    public enum FileType
    {
        Image,
        CSV
    }
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
                var file = request.FormData.File;
                var type = request.FormData.Type;

                var extension = System.IO.Path.GetExtension(file.FileName);

                // Call the service that takes the raw file and returns a ModelInput (T)
                if (file == null || type == null)
                {
                    return Result<S>.Failure("No file attached");
                }

                if (!Enum.TryParse(type, out FileType fileType)) {
                    return Result<S>.Failure("Unknown file type");
                }

                // TODO: Use factory pattern
                IFileOptions opts;
                if (fileType == FileType.Image) {
                    opts = new ImageOptions { };
                    _mapper.Map(request.FormData.Options, opts);
                }
                else {
                    opts = new CSVOptions { };
                    _mapper.Map(request.FormData.Options, opts);
                }

                //request.FormData.Options = opts;

                //_predictionService.CreateImageIngestionPipelineForModelWithVectorInput("InitialModels/MNIST.zip", 8, "PixelValues");
                var pred = await _predictionService.PredictSingleDataPointFromForm(request.FormData);

                return Result<S>.Success(pred);
            }

        }
    }
}
