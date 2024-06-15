using System.Collections.Concurrent;
using Sense.Devices.Protocol.Envelopes;
using Sense.Devices.Protocol.Rpc;

namespace ClockTransactionsTransmiter.Devices.Connectivity;

public sealed class RequestsManager : IDisposable
{
    private readonly DeviceToCloudConnection _connection;
    private readonly ConcurrentDictionary<Guid, OngoingRequest> _pending;
    private readonly CancellationTokenSource _disposeSource;

    public RequestsManager(DeviceToCloudConnection connection)
    {
        _connection = connection;
        _pending = new ConcurrentDictionary<Guid, OngoingRequest>();
        _disposeSource = new CancellationTokenSource();
    }

    public Task Handle(CloudMessageEnvelope envelope, CancellationToken cancellationToken)
    {
        if (envelope.Message.Body is CloudResponse response &&
            _pending.TryRemove(response.Header.RequestId, out OngoingRequest? pendingRequest))
        {
            pendingRequest.Complete(response);
        }

        return Task.CompletedTask;
    }

    public async Task<CloudResponse> Request(DeviceRequest request, CancellationToken cancellationToken)
    {
        var ongoingRequest = new OngoingRequest(request.Header.RequestId);

        _pending.TryAdd(ongoingRequest.Id, ongoingRequest);

        try
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _disposeSource.Token);

            await _connection.Send(request, cancellation.Token);

            Task canceled = Task.Delay(-1, cancellation.Token);
            Task completed = await Task.WhenAny(ongoingRequest.Task, canceled);

            if (completed == canceled)
            {
                await canceled;
            }

            return await ongoingRequest.Task;
        }
        finally
        {
            _pending.TryRemove(ongoingRequest.Id, out _);
        }
    }

    public void Dispose()
    {
        _disposeSource.Cancel();
        _disposeSource.Dispose();
    }

    private sealed class OngoingRequest
    {
        public Guid Id { get; }

        public Task<CloudResponse> Task => _promise.Task;

        private readonly TaskCompletionSource<CloudResponse> _promise;

        public OngoingRequest(Guid id)
        {
            Id = id;

            _promise = new TaskCompletionSource<CloudResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Complete(CloudResponse response)
        {
            _promise.SetResult(response);
        }
    }
}
