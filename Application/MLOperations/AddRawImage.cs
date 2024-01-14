using DeepServices;
using MediatR;
using Microsoft.AspNetCore.Http;
using Application.Core;


// TODO: Create a service tot converts files into MNIST Inputs
namespace Application.MLOperations
{
    public class AddRawImage<T> where T : class, new()
    {
        public class Command : IRequest<Result<T>>
        {
            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<T>>
        {
            private int _counter = 0;
            public Handler()
            {
            }
            public async Task<Result<T>> Handle(Command request, CancellationToken cancellationToken)
            {
                var file = request.File;

                // Call the service that takes the raw file and returns a ModelInput (T)
                if (file != null) {
                    return Result<T>.Success(new T());
                } else {
                    return Result<T>.Failure("This failed");
                }
            }

        }

    }
}
