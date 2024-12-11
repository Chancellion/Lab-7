using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DistributedSystemNode
{
    private readonly string nodeId;
    private readonly ConcurrentDictionary<string, DistributedSystemNode> nodes;
    private readonly ConcurrentQueue<string> messageQueue;
    private bool isActive;
    public DistributedSystemNode(string nodeId)
    {
        this.nodeId = nodeId;
        this.nodes = new ConcurrentDictionary<string, DistributedSystemNode>();
        this.messageQueue = new ConcurrentQueue<string>();
        this.isActive = true;
    }
    public void RegisterNode(DistributedSystemNode node)
    {
        nodes[node.nodeId] = node;
    }
    public void UnregisterNode(string nodeId)
    {
        nodes.TryRemove(nodeId, out _);
    }
    public async Task SendMessageAsync(string targetNodeId, string message)
    {
        if (nodes.TryGetValue(targetNodeId, out var targetNode))
        {
            await Task.Run(() => targetNode.ReceiveMessage(message));
        }
    }
    public void ReceiveMessage(string message)
    {
        messageQueue.Enqueue(message);
        ProcessMessagesAsync();
    }
    private async Task ProcessMessagesAsync()
    {
        while (messageQueue.TryDequeue(out var message))
        {
            await Task.Run(() => Console.WriteLine($"Вузол {nodeId}, отримано повiдомлення: {message}"));
        }
    }
    public void SetStatus(bool isActive)
    {
        this.isActive = isActive;
        NotifyStatusChange();
    }
    private void NotifyStatusChange()
    {
        foreach (var node in nodes.Values)
        {
            node.ReceiveStatusUpdate(nodeId, isActive);
        }
    }
    public void ReceiveStatusUpdate(string nodeId, bool isActive)
    {
        Console.WriteLine($"Вузол {this.nodeId}. Отримана iнформацiя щодо статусу: Вузол {nodeId} наразi {(isActive ? "воскрес" : "здох")}");
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Данiїл Iванченко, КIб-1-23-4.0д");
        var nodeA = new DistributedSystemNode("A");
        var nodeB = new DistributedSystemNode("B");
        var nodeC = new DistributedSystemNode("C");
        nodeA.RegisterNode(nodeB);
        nodeA.RegisterNode(nodeC);
        nodeB.RegisterNode(nodeA);
        nodeB.RegisterNode(nodeC);
        nodeC.RegisterNode(nodeA);
        nodeC.RegisterNode(nodeB);
        await nodeA.SendMessageAsync("B", "Привiтання вiд A до B");
        await nodeB.SendMessageAsync("C", "Привiтання вiд B до C");
        await nodeC.SendMessageAsync("A", "Привiтання вiд C до A");
        nodeA.SetStatus(false);
        nodeB.SetStatus(true);
        nodeC.SetStatus(false);
    }
}
