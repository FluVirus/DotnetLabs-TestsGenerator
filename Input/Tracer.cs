using System.Collections.Concurrent;
using System.Diagnostics;
using University.DotnetLabs.Lab1.TracerLibrary.Exceptions;

namespace University.DotnetLabs.Lab1.TracerLibrary;

public class Tracer : ITracer
{
    private readonly ConcurrentDictionary<int, Stack<MethodTreeNode>> _stacks = new();
    public TraceResult GetTraceResult()
    {
        LinkedList<ThreadData> threads = new();
        foreach (Stack<MethodTreeNode> stack in _stacks.Values)
        {
            ThreadData threadData = (ThreadData)stack.Last();
            foreach (MethodData methodData in threadData.InternalMethods)
            {
                threadData.Duration = threadData.Duration.Add(methodData.Duration.Elapsed);
            }
            threads.AddLast(threadData);
        }
        return new TraceResult(threads);
    }

    public void StartTrace()
    {
        StackFrame stackFrame = new StackTrace().GetFrame(1); //get super method data
        string methodName = stackFrame.GetMethod().Name;
        string className = stackFrame.GetMethod().DeclaringType.FullName;
        int threadID = Thread.CurrentThread.ManagedThreadId;
        MethodData methodData = new MethodData(methodName, className);
        Stack<MethodTreeNode> stack = _stacks.GetOrAdd(threadID, threadID =>
        {
            ThreadData threadData = new(threadID);
            Stack<MethodTreeNode> newStack = new();
            newStack.Push(threadData);
            _stacks.TryAdd(threadID, newStack);
            return newStack;
        });
        stack.Push(methodData);
        methodData.Duration.Start();
    }

    public void StopTrace()
    {
        int threadID = Thread.CurrentThread.ManagedThreadId;
        Stack<MethodTreeNode> stack;
        bool successed = _stacks.TryGetValue(threadID, out stack);
        if (!successed)
        {
            throw new TracerThreadException("There is no stack tracing with such id");
        }
        MethodTreeNode node;
        successed = stack.TryPop(out node);
        if (!successed || node is ThreadData)
        {
            throw new TracerStackException($"Stack with thread id {threadID} suddenly contains no data");
        }
        MethodData methodData = (MethodData)node;
        methodData.Duration.Stop();
        StackFrame stackFrame = new StackTrace().GetFrame(1);
        string methodName = stackFrame.GetMethod().Name;
        string className = stackFrame.GetMethod().DeclaringType.FullName;
        if (!methodData.MethodName.Equals(methodName) || !methodData.ClassName.Equals(className))
        {
            throw new TracerStackException($"Unexpected StopTrace() in {className}.{methodName} . Expected {methodData.ClassName}.{methodData.MethodName} on stack.");
        }
        stack.Peek().InternalMethods.AddLast(methodData);
        ThreadData threadData = (ThreadData) stack.Last();
    }
}
