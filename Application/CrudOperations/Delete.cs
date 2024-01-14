using AutoMapper;
using MediatR;
using Persistence;

namespace Application.BusinessLogic
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var dataItem = await _context.DomainSpecificDataItems.FindAsync(request.Id);
                _context.Remove(dataItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
