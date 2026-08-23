using Mediator;
using MemoAna.Backend.Application.Common.Responses;
using MemoAna.Backend.Application.Health.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoAna.Backend.Application.Health.Queries;

public sealed record GetHostInfoQuery : IRequest<Response<HealthCheckResponse>>;
