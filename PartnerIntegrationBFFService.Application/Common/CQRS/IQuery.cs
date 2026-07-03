using MediatR;

namespace PartnerIntegrationBFFService.Application.Common.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
    {
    }
}

