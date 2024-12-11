using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Event
{
    public string Name { get; }
    public DateTime Timestamp { get; }
    public int NodeId { get; }

    public Event(string name, int nodeId)
    {
        Name = name;
        Timestamp = DateTime.Now;
        NodeId = nodeId;
    }
}
public class DistributedSystemNode
{
    private readonly int nodeId;
    private readonly ConcurrentDictionary<int, DistributedSystemNode> nodes;
    private readonly ConcurrentQueue<Event> eventQueue;
    private readonly List<Action<Event>> eventHandlers;
    private int lamportClock;
    public DistributedSystemNode(int nodeId)
    {
        this.nodeId = nodeId;
        this.nodes = new ConcurrentDictionary<int, DistributedSystemNode>();
        this.eventQueue = new ConcurrentQueue<Event>();
        this.eventHandlers = new List<Action<Event>>();
        this.lamportClock = 0;
    }
    public void RegisterNode(DistributedSystemNode node)
    {
        nodes[node.nodeId] = node;
    }
    public void UnregisterNode(int nodeId)
    {
        nodes.TryRemove(nodeId, out _);
    }
    public void RegisterEventHandler(Action<Event> handler)
    {
        eventHandlers.Add(handler);
    }
    public async Task SendEventAsync(string eventName)
    {
        lamportClock++;
        var newEvent = new Event(eventName, nodeId);
        foreach (var node in nodes.Values)
        {
            await Task.Run(() => node.ReceiveEvent(newEvent));
        }
    }
    public void ReceiveEvent(Event eventReceived)
    {
        eventQueue.Enqueue(eventReceived);
        ProcessEventsAsync();
    }
    private async Task ProcessEventsAsync()
    {
        while (eventQueue.TryDequeue(out var eventToProcess))
        {
            lamportClock = Math.Max(lamportClock, (int)eventToProcess.Timestamp.Millisecond) + 1;
            foreach (var handler in eventHandlers)
            {
                await Task.Run(() => handler(eventToProcess));
            }
        }
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Данiїл Iванченко, КIб-1-23-4.0д");
        var nodeA = new DistributedSystemNode(1);
        var nodeB = new DistributedSystemNode(2);
        var nodeC = new DistributedSystemNode(3);
        nodeA.RegisterNode(nodeB);
        nodeA.RegisterNode(nodeC);
        nodeB.RegisterNode(nodeA);
        nodeB.RegisterNode(nodeC);
        nodeC.RegisterNode(nodeA);
        nodeC.RegisterNode(nodeB);
        nodeA.RegisterEventHandler(e => Console.WriteLine($"Node {e.NodeId} processed event {e.Name} at {e.Timestamp}"));
        nodeB.RegisterEventHandler(e => Console.WriteLine($"Node {e.NodeId} processed event {e.Name} at {e.Timestamp}"));
        nodeC.RegisterEventHandler(e => Console.WriteLine($"Node {e.NodeId} processed event {e.Name} at {e.Timestamp}"));
        await nodeA.SendEventAsync("Event1");
        await nodeB.SendEventAsync("Event2");
        await nodeC.SendEventAsync("Event3");
        nodeA.UnregisterNode(2);
        await nodeA.SendEventAsync("Event4");
    }
}
