using AccountManagement.Infrastructure.Core.Models;
using MediatR;

namespace AccountManagement.Application
{
    public interface IRequestWrapper<T> : IRequest<ResultObject<T>>
    {
    }

    public interface IRequestHandlerWrapper<Tin, Tout> : IRequestHandler<Tin, ResultObject<Tout>>
        where Tin : IRequestWrapper<Tout>
    {
    }
}
