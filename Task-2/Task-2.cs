using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class Resource
{
    public string Name { get; }
    public SemaphoreSlim Semaphore { get; }
    public Resource(string name, int initialCount)
    {
        Name = name;
        Semaphore = new SemaphoreSlim(initialCount, initialCount);
    }
}
public class ResourceManager
{
    private readonly Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
    public void AddResource(string name, int initialCount)
    {
        resources[name] = new Resource(name, initialCount);
    }
    public async Task<bool> AcquireResourceAsync(string name, int priority, CancellationToken cancellationToken)
    {
        if (!resources.ContainsKey(name))
            throw new ArgumentException($"Resource {name} not found.");
        var resource = resources[name];
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} with priority {priority} is waiting for {name}.");
        await resource.Semaphore.WaitAsync(cancellationToken);
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} with priority {priority} acquired {name}.");
        return true;
    }
    public void ReleaseResource(string name)
    {
        if (!resources.ContainsKey(name))
            throw new ArgumentException($"Resource {name} not found.");
        var resource = resources[name];
        resource.Semaphore.Release();
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} released {name}.");
    }
}
public class TaskSimulator
{
    private readonly ResourceManager resourceManager;
    public TaskSimulator(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }
    public async Task SimulateTaskAsync(string resourceName, int priority, int duration, CancellationToken cancellationToken)
    {
        try
        {
            if (await resourceManager.AcquireResourceAsync(resourceName, priority, cancellationToken))
            {
                await Task.Delay(duration, cancellationToken);
                resourceManager.ReleaseResource(resourceName);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} with priority {priority} was cancelled.");
        }
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Данiїл Iванченко, КIб-1-23-4.0д");
        var resourceManager = new ResourceManager();
        resourceManager.AddResource("CPU", 2);
        resourceManager.AddResource("RAM", 2);
        resourceManager.AddResource("Disk", 1);
        var taskSimulator = new TaskSimulator(resourceManager);
        var cts = new CancellationTokenSource();
        var tasks = new List<Task>
        {
            taskSimulator.SimulateTaskAsync("CPU", 1, 3000, cts.Token),
            taskSimulator.SimulateTaskAsync("RAM", 2, 2000, cts.Token),
            taskSimulator.SimulateTaskAsync("Disk", 1, 1000, cts.Token),
            taskSimulator.SimulateTaskAsync("CPU", 2, 1500, cts.Token),
            taskSimulator.SimulateTaskAsync("RAM", 1, 2500, cts.Token)
        };
        await Task.WhenAll(tasks);
    }
}
