To execute the code you need .NET 8 and Rust toolchains. .NET can installed with the package manager of your choice (unless you are on plain Debian, see footnote[^1]):
```sh
sudo apt install dotnet-sdk-8.0
# or
sudo dnf install dotnet-sdk-8.0
# or
brew install dotnet-sdk
```

You can run the demo with
```sh
# will take a bit to build Rust and C# code for the first time
cd CSharp && dotnet run -c Releasr
```

Your output should be something like:
```
Current thread count in the pool: 0
Current thread count in the pool: 1
08:26.19: thread: 15, task: 10
08:26.19: thread: 13, task: 8
08:26.19: thread: 10, task: 5
08:26.19: thread: 4, task: 0
08:26.19: thread: 16, task: 11
08:26.19: thread: 8, task: 3
08:26.19: thread: 9, task: 4
08:26.19: thread: 19, task: 14
08:26.19: thread: 14, task: 9
08:26.19: thread: 11, task: 6
08:26.19: thread: 7, task: 2
08:26.19: thread: 17, task: 12
08:26.19: thread: 12, task: 7
08:26.19: thread: 20, task: 15
08:26.19: thread: 18, task: 13
08:26.19: thread: 6, task: 1
08:26.70: thread: 21, task: 16
08:27.71: thread: 22, task: 17
08:28.22: thread: 23, task: 18
[Omitted n lines]
08:05.69: thread: 68, task: 63
Done: 00:00:39.5031966
Current thread count in the pool: 64
```

Explanation:
In .NET, you don't pay for threadpool taking memory and threads unless you use it. When you do, it starts with Environment.ProcessorCount threads (but because the observation has a race here, you see 1). Which is why the first tasks get handled immediately and before getting un-cooperatively blocked across interop boundary.

After that, subsequent thread injection and removal is subject to hill-climbing algorithm that will slowly but surely counteract threadpool starvation.
In realistic workloads, such bursty starvations are usually either not observed or observed during startup-like scenarios. After reaching the
steady state, the threadpool will settle on optimal amount of threads that are necessary to process the work items without delay.

However, most common "degenerate" cases like above tend to happen within constraints of managed code instead. Situations like soft deadlocks, sync over async or vice versa, simple usage of synchronous lock primitives, etc. In that case, threadpool has a separate mechanism that lets
such blocking operations to interactively notify it about the blocking and allow it to immediately inject more threads without any delay.

In those situations there is no wind-up period and the code above is executed under 500ms on my machine with .NET 9 RC.1 build (which also includes going through JIT compilation, it would be lower with NativeAOT).
The code to reproduce such obsevation can be found here: https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#threading
This repo adapted it to demonstrate un-cooperative blocking across FFI instead. Even without it, I think this article will be interesting to HN readers so give it a skim!

[^1]: https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian#debian-12
