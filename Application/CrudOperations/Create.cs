using Application.Core;
using Domain;
using MediatR;
using Persistence;

namespace Application.BusinessLogic
{
    public class Create
    {
        public class Command : IRequest<Result<MLModelMetadata>>
        {
            public MLModel Model { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<MLModelMetadata>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Result<MLModelMetadata>> Handle(Command request, CancellationToken cancellationToken)
            {
                //request.Model.Id = request.Model.Id ?? Guid.NewGuid();
                _context.MLModels.Add(request.Model);
                await _context.SaveChangesAsync();
                var metadata = new MLModelMetadata { Id = request.Model.Id, Title = request.Model.Title };
                return Result<MLModelMetadata>.Success(metadata);
            }
        }

    }
}
