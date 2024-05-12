# Experimental Godot support

[Godot 4](https://godotengine.org/) support (C# only)
in Responsible is currently **experimental**
and requires a bit of manual setup.
As I don't use Godot myself,
I am not confident in publishing a package for Godot support
until I've received some feedback from more experienced Godot developers.

Opening [issues](https://github.com/sbergen/Responsible/issues) for Godot support with reproduction steps is highly welcome, though!

## Installation & setup

### Overview

1. You will need to install some unit testing framework,
   for example [GdUnit4](https://mikeschulze.github.io/gdUnit4/).
2. Add a NuGet reference to
   [Beatwaves.Responsible](https://www.nuget.org/packages/Beatwaves.Responsible).
   (Using `<PackageReference>` or a NuGet GUI in your IDE).
3. Add implementations for a `TestInstructionExecutor` and `ITestScheduler` (examples below).
4. Write tests using the executor.

### Example implementations

The examples below have been tested with GdUnit4,
and use C# 11.

#### `GodotTestScheduler`

The test scheduler provides Responsible the concepts of
frames, time, and frame-based polling.
In Godot, frame-based polling requires an active `Node`,
so the scheduler requires a parent node in its constructor.

```cs
using System;
using Godot;
using Responsible.Utilities;

namespace Responsible.Godot;

internal partial class GodotTestScheduler : Node, ITestScheduler
{
    private readonly RetryingPoller _poller = new RetryingPoller();

    public GodotTestScheduler(Node parent)
    {
        parent.AddChild(this);
    }

    public int FrameNow => (int)Engine.GetProcessFrames();

    public DateTimeOffset TimeNow => DateTimeOffset.Now;

    public IDisposable RegisterPollCallback(Action action) =>
        _poller.RegisterPollCallback(action);

    public override void _Process(double delta)
    {
        _poller.Poll();
    }
}
```

#### `GodotTestInstructionExecutor`

The implementation below is a minimum viable implementation.
As noted in the TODO comment,
something was only printing the inner exception on failures,
so a quick and dirty workaround for this was included.

```cs
using System;
using Godot;

namespace Responsible.Godot;

internal class GodotTestInstructionExecutor : TestInstructionExecutor
{
    public GodotTestInstructionExecutor(
        Node parent,
        IGlobalContextProvider globalContextProvider = null)
        : base(
                new GodotTestScheduler(parent),
                externalResultSource: null,
                failureListener: new GodotFailureListener(),
                globalContextProvider,
                rethrowableExceptions: null)
    {
    }

    private class GodotFailureListener : IFailureListener
    {
        public void OperationFailed(Exception exception, string failureMessage)
        {
            // TODO: Wrap the message in a new exception,
            // as something (GdUnit4?) is only printing the inner exception details.
            throw new Exception(failureMessage);
        }
    }
}
```

#### Example test (using GdUnit4)

This test is just a proof of concept that proves the test scheduler works:
It waits for 42 frames and 1 second, and then fails.

```cs
using GdUnit4;
using System.Threading.Tasks;
using Responsible;
using Responsible.Godot;
using static Responsible.Responsibly;

namespace MyTestNamespace;

[TestSuite]
public class PaddleTest
{
    [TestCase]
    public async Task TestResponsible()
    {
        var sceneRunner = ISceneRunner.Load("res://MyScene.tscn");
        var executor = new GodotTestInstructionExecutor(sceneRunner.Scene());
        await WaitForFrames(42)
            .ContinueWith(WaitForSeconds(1))
            .ContinueWith(Do("Throw an error", () => throw new Exception("Fail!")))
            .ToTask(executor);
    }
}
```

Running the test should provide output similar to the below:
```
Test operation execution failed!
 
Failure context:
[�] WAIT FOR 42 FRAME(S) (Completed in 0,68 s � 43 frames)
[�] WAIT FOR 0:00:01 (Completed in 1,01 s � 61 frames)
[!] Throw an error (Failed after 0,00 s � 0 frames)
 
  Failed with:
    System.Exception:
      Fail!
 
  Test operation stack:
    [Do] TestResponsible (at <path-to-file>)
    [ContinueWith] TestResponsible (at <path-to-file>)
    [ToTask] TestResponsible (at <path-to-file>)
 
Error: System.Exception: Fail!
  <stack-trace-here>
```

Yes, there's something odd going on with the UTF-8 characters -
this has not yet been investigated further.