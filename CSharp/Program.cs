using System.Diagnostics;
using System.Runtime.InteropServices;

[DllImport("Rust", EntryPoint = "init")]
static extern void Init(nint threads);

[DllImport("Rust", EntryPoint = "wait")]
static extern void Wait();

PrintThreadCount();
var count = Environment.ProcessorCount * 4;
Init(count); // Initialize the Barrier at the Rust side

var sw = Stopwatch.StartNew();
var tasks = Enumerable
    .Range(0, count)
    .Select(i => Task.Run(() => {
        var threadId = Environment.CurrentManagedThreadId;
        Console.WriteLine(
            $"{DateTime.UtcNow:MM:ss.ff}: thread: {threadId}, task: {i}");
        Wait(); // Un-cooperative wait
    }))
    .ToArray();

PrintThreadCount();
Task.WaitAll(tasks);

Console.WriteLine($"Done: {sw.Elapsed}");
PrintThreadCount();

static void PrintThreadCount() =>
    Console.WriteLine($"Current thread count in the pool: {ThreadPool.ThreadCount}");
