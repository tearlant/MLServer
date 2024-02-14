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
            public MLModel Model { get; set; }
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
                var dataItem = await _context.MLModels.FindAsync(request.Model.Id);
                _mapper.Map(request.Model, dataItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
