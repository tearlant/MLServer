using Domain;
using MediatR;
using Persistence;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BusinessLogic
{
    public class Details
    {
        public class Query : IRequest<DomainSpecificDataItem> {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, DomainSpecificDataItem>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<DomainSpecificDataItem> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.DomainSpecificDataItems.FindAsync(request.Id);
            }
        }
    }
}
