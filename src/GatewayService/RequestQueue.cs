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
            var res = await request();
            if (!res)
            {
                Console.WriteLine($"requestQueue: false");
                _requestsQueue.Enqueue(request);
            }
            else Console.WriteLine($"requestQueue: true");


            Thread.Sleep(TimeSpan.FromSeconds(TimeoutInSeconds));
        }
    }
}
