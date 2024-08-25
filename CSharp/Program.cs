// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.InteropServices;

const string Lib = "../Rust/target/release/Rust";

[DllImport(Lib, EntryPoint = "init")]
static extern void Init(nint threads);

[DllImport(Lib, EntryPoint = "wait")]
static extern void Wait();

PrintThreadCount();
var count = Environment.ProcessorCount * 4;
// var tasks = new List<Task>(count);
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