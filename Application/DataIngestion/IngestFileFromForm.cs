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
                //var data = request.FormData.Data;
                //var hasHeader = request.FormData.HasHeader;

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

                // TODO: Use factory pattern
                // TODO: Ingest ImageOptions and CSVOptions
                //IFileOptions opts;
                //if (fileType == FileType.Image) {
                //    opts = new ImageOptions { };
                //    _mapper.Map(request.FormData.Options, opts);
                //}
                //else {
                //    opts = new CSVOptions { };
                //    _mapper.Map(request.FormData.Options, opts);
                //}

                //request.FormData.Options = opts;

                //_predictionService.CreateImageIngestionPipelineForModelWithVectorInput("InitialModels/MNIST.zip", 8, "PixelValues");

                var pred = await _predictionService.PredictSingleDataPointFromForm(request.FormData);
                //var pred = await _predictionService.PredictSingleDataPointFromFormDataOnly(request.FormData, hasHeader);

                return Result<S>.Success(pred);
            }

        }
    }
}
