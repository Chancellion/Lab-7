using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Operation
{
    public DateTime Timestamp { get; }
    public int ThreadId { get; }
    public string Resource { get; }
    public string Action { get; }
    public Operation(string resource, string action)
    {
        Timestamp = DateTime.Now;
        ThreadId = Thread.CurrentThread.ManagedThreadId;
        Resource = resource;
        Action = action;
    }
}
public class LiveLog
{
    private readonly ConcurrentQueue<Operation> log = new ConcurrentQueue<Operation>();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> resourceLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
    public void AddOperation(Operation operation)
    {
        log.Enqueue(operation);
        Console.WriteLine($"Operation added: {operation.Action} on {operation.Resource} by thread {operation.ThreadId} at {operation.Timestamp}");
    }
    public async Task<bool> TryExecuteOperationAsync(Operation operation)
    {
        var resourceLock = resourceLocks.GetOrAdd(operation.Resource, new SemaphoreSlim(1, 1));

        if (await resourceLock.WaitAsync(0))
        {
            try
            {
                AddOperation(operation);
                return true;
            }
            finally
            {
                resourceLock.Release();
            }
        }
        else
        {
            Console.WriteLine($"Conflict detected: {operation.Action} on {operation.Resource} by thread {operation.ThreadId} at {operation.Timestamp}");
            return false;
        }
    }
    public void ResolveConflict(Operation operation)
    {
        Console.WriteLine($"Resolving conflict for operation: {operation.Action} on {operation.Resource} by thread {operation.ThreadId} at {operation.Timestamp}");
        AddOperation(operation);
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Данiїл Iванченко, КIб-1-23-4.0д");
        var liveLog = new LiveLog();

        var tasks = new List<Task>
        {
            Task.Run(async () => await ExecuteOperationAsync(liveLog, "Resource1", "Write")),
            Task.Run(async () => await ExecuteOperationAsync(liveLog, "Resource1", "Read")),
            Task.Run(async () => await ExecuteOperationAsync(liveLog, "Resource2", "Write")),
            Task.Run(async () => await ExecuteOperationAsync(liveLog, "Resource2", "Read"))
        };
        await Task.WhenAll(tasks);
    }
    static async Task ExecuteOperationAsync(LiveLog liveLog, string resource, string action)
    {
        var operation = new Operation(resource, action);
        if (!await liveLog.TryExecuteOperationAsync(operation))
        {
            liveLog.ResolveConflict(operation);
        }
    }
}
