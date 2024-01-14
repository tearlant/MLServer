using Domain;
using MediatR;
using Persistence;

namespace Application.BusinessLogic
{
    public class Create
    {
        public class Command : IRequest
        {
            public DomainSpecificDataItem DataItem { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                _context.DomainSpecificDataItems.Add(request.DataItem);
                await _context.SaveChangesAsync();
            }
        }

    }
}
