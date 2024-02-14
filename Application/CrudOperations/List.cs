using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using SQLitePCL;

namespace Application.BusinessLogic
{
    public class List
    {
        public class Query : IRequest<List<MLModelMetadata>> { }

        public class Handler : IRequestHandler<Query, List<MLModelMetadata>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<List<MLModelMetadata>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.MLModels.Select(x => new MLModelMetadata { Id = x.Id, Title = x.Title }).ToListAsync();
            }
        }
    }
}
