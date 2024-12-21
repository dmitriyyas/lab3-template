using System.Collections.Concurrent;

namespace GatewayService;

public class RequestQueue
{
    private readonly ConcurrentQueue<Func<Task<bool>>> _requestsQueue = new();
    private const int TimeoutInSeconds = 10;

    public RequestQueue()
    {
        StartWorker();
    }

    public void StartWorker()
    {
        new Thread(Start).Start();
        Console.WriteLine("Thread started");
    }

    public void AddRequestToQueue(Func<Task<bool>> request)
    {
        _requestsQueue.Enqueue(request);
    }

    private async void Start(object? state)
    {
        while (true)
        {
            if (!_requestsQueue.TryPeek(out var request))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                continue;
            }

            _requestsQueue.TryDequeue(out _);
            _ = Task.Run(async () =>
            {
                var start = DateTime.UtcNow;
                var res = await request();
                var end = DateTime.UtcNow;
                if (!res)
                {
                    _ = Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds - (end - start).TotalSeconds))
                        .ContinueWith(_ => _requestsQueue.Enqueue(request));
                }
            });
        }
    }
}
