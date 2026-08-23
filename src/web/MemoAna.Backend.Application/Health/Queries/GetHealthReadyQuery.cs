using Mediator;
using MemoAna.Backend.Application.Common.Responses;
using MemoAna.Backend.Application.Health.Responses;

namespace MemoAna.Backend.Application.Health.Queries;

public sealed record GetHealthReadyQuery : IRequest<Response<HealthCheckResponse>>;
