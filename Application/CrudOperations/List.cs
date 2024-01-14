using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using SQLitePCL;

namespace Application.BusinessLogic
{
    public class List
    {
        public class Query : IRequest<List<DomainSpecificDataItem>> { }

        public class Handler : IRequestHandler<Query, List<DomainSpecificDataItem>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<List<DomainSpecificDataItem>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.DomainSpecificDataItems.ToListAsync();
            }
        }
    }
}
