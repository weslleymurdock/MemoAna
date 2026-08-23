//using MemoAna.Backend.Application.Common.Abstractions;
//using MemoAna.Backend.Application.Common.Pipeline.Validation;
//using MemoAna.Backend.Application.Common.Responses;
//using MemoAna.Backend.Application.SignalR.Commands;
//using MemoAna.Backend.Application.SignalR.Notifications;
//using MemoAna.Backend.Application.SignalR.Queries;
//using MemoAna.Backend.Application.SignalR.Responses;
//using MemoAna.Backend.Application.SignalR.Validators;
//using MemoAna.Backend.UnitTests.Common.Fixtures;
//using MemoAna.Backend.UnitTests.Common.Mocks;
//using FluentValidation;
//using Mediator;
//using Microsoft.Extensions.DependencyInjection;
//using Xunit;

//namespace MemoAna.Backend.UnitTests.SignalR;

///// <summary>Validates end-to-end Mediator dispatch.</summary>
//public sealed class SignalRDispatchTests
//{
//    private readonly MemoAna.BackendTestFixture _fixture; 
//    public SignalRDispatchTests()
//    {
//        _fixture = new();
//    }
//    [Fact]
//    public async Task DiscoveryCommand_DispatchesToHandler()
//    {
//        IMediator mediator =  _fixture.Mediator;
//        Response<DiscoveryResponse> result =
//            await mediator.Send(
//                new StartDiscoveryCommand("house", 30),
//                TestContext.Current.CancellationToken);

//        Assert.True(result.Succeeded);
//        Assert.Equal("house", result.Data?.HouseId);
//    }

//    [Fact]
//    public async Task RelayCommand_DispatchesToHandler()
//    {
//        IMediator mediator =  _fixture.Mediator;

//        Response<RelayStateResponse> result =
//            await mediator.Send(
//                new SetRelayCommand("endpoint", true),
//                TestContext.Current.CancellationToken);

//        Assert.True(result.Succeeded);
//        Assert.True(result.Data?.State);
//    }

//    [Fact]
//    public async Task FirmwareCommand_DispatchesToHandler()
//    {
//        IMediator mediator = _fixture.Mediator;

//        Response<FirmwareProgressResponse> result =
//            await mediator.Send(
//                new StartFirmwareUpdateCommand(
//                    "node",
//                    "2.0.0"),
//                TestContext.Current.CancellationToken);

//        Assert.True(result.Succeeded);
//        Assert.Equal("node", result.Data?.NodeId);
//    }

//    [Fact]
//    public async Task Query_DispatchesToHandler()
//    {
//        IMediator mediator = _fixture.Mediator;
//        Response<TelemetrySnapshotResponse> result =
//            await mediator.Send(
//                new GetTelemetrySnapshotQuery("house"),
//                TestContext.Current.CancellationToken);

//        Assert.True(result.Succeeded);
//        Assert.Empty(result.Data!.Items);
//    }

//    [Fact]
//    public async Task Notification_DispatchesToHandler()
//    {
        
//        IMediator mediator =  _fixture.Mediator;
//        TelemetryResponse telemetry = CreateTelemetry();

//        await mediator.Publish(new TelemetryUpdatedNotification(telemetry),
//            TestContext.Current.CancellationToken);

//        FakeSignalRService service = _fixture.FSignalR;
//        Assert.Same(telemetry, service.Telemetry);
//    }

//    [Fact]
//    public async Task Stream_DispatchesToHandler()
//    {
//        IMediator mediator =  _fixture.Mediator;

//        FakeSignalRService service =  _fixture.FSignalR;

//        TelemetryResponse telemetry = CreateTelemetry();
//        service.StreamItems.Add(telemetry);

//        List<TelemetryResponse> items = [];
//        await foreach (TelemetryResponse item in mediator.CreateStream(
//            new WatchTelemetryQuery("house"),
//            TestContext.Current.CancellationToken))
//        {
//            items.Add(item);
//        }

//        Assert.Single(items);
//        Assert.Same(telemetry, items[0]);
//    }

//    [Fact]
//    public async Task Command_InvalidInput_IsRejectedByPipeline()
//    {
//        IMediator mediator =  _fixture.Mediator;

//        await Assert.ThrowsAsync<ValidationException>(
//            async () => await mediator.Send(
//                new SetRelayCommand(string.Empty, true),
//                TestContext.Current.CancellationToken));
//    }
 
//    private static TelemetryResponse CreateTelemetry()
//    {
//        return new TelemetryResponse(
//            "house",
//            "node",
//            "endpoint",
//            DateTimeOffset.UtcNow,
//            220,
//            1,
//            220,
//            100,
//            true);
//    }

   
//}
