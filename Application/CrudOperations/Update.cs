using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.BusinessLogic
{
    public class Update
    {
        public class Command : IRequest
        {
            public DomainSpecificDataItem DataItem { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var dataItem = await _context.DomainSpecificDataItems.FindAsync(request.DataItem.Id);
                _mapper.Map(request.DataItem, dataItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
