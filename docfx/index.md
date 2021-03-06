# Responsible - Reactive Asynchronous Testing

[![License](https://img.shields.io/github/license/sbergen/Responsible.svg)](https://github.com/sbergen/Responsible/blob/main/LICENSE)
[![codecov](https://codecov.io/gh/sbergen/Responsible/branch/main/graph/badge.svg)](https://codecov.io/gh/sbergen/Responsible)
[![Mutation testing](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fsbergen%2FResponsible%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/sbergen/Responsible/main)
[![Nuget](https://img.shields.io/nuget/v/Beatwaves.Responsible)](http://nuget.org/packages/Beatwaves.Responsible)
[![openupm](https://img.shields.io/npm/v/com.beatwaves.responsible?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.beatwaves.responsible/)

*Responsible* helps you write maintainable high level asynchronous tests in C#:
* Get highly readable and informative output on test failures and timeouts
* Write declarative, composable, and reusable test code

Additionally, in [Unity](https://unity.com/):
* Observe test execution progress while they are running in the Editor
* Stop worrying about a specific long-standing [Unity bug](https://issuetracker.unity3d.com/issues/unitytests-do-not-fail-when-nested-coroutines-throws-an-exception)


## Usage and Output Example

```cs
// with 'using static Responsible.Responsibly;'

yield return WaitForCondition("Foo to be ready", () => foo.IsReady)
    .AndThen(WaitForCondition("Bar to be completed", () => bar.IsCompleted))
    .ThenRespondWith("Foo the bar", Do("Consume bar", () => foo.Consume(bar)))
    .ExpectWithinSeconds(10)
    .ContinueWith(Do("Continue operation", foo.ContinueOperation))
    .ToYieldInstruction(this.TestInstructionExecutor);
```

If `foo.Consume` were to throw an error, the output would look like this:
```
Test operation execution failed!
 
Failure context:
[!] Foo the bar EXPECTED WITHIN 10.00 s (Failed after 0.15 s and 7 frames)
  WAIT FOR
    [✓] Foo to be ready (Completed in 0.11 s and 3 frames)
    [✓] Bar to be completed (Completed in 0.04 s and 4 frames)
  THEN RESPOND WITH
    [!] Consume bar (Failed after 0.00 s and 0 frames)
 
      Failed with:
        System.Exception: 'Something failed'
 
      Test operation stack:
        [Do] MethodName (at Path/To/Source.cs:41)
        [ExpectWithinSeconds] MethodName (at Path/To/Source.cs:42)
        [ContinueWith] MethodName (at Path/To/Source.cs:43)
        [ToYieldInstruction] MethodName (at Path/To/Source.cs:44)
 
[ ] Continue operation
 
Error: System.Exception: Something failed
  at <normal exception stack trace comes here>
```

In Unity, by using the Editor window available under `Window -> Responsible -> Operation State`,
you can observe the progress of your tests executing.
The contents of the window updates in real time and matches the output produced on failures,
except for the operation stack and stack trace.

## Is That It?

The above sample introduces the responder pattern:
an if-then relationship between a wait condition and (optionally) asynchronous response.
However, just like it wouldn't be fair to dismiss [ReactiveX](http://reactivex.io/)
as *just an implementation of the observer pattern*,
Responsible is also a lot more than just this pattern:
the real power of Responsible comes from its composable operators.
To read more about the design principles behind Responsible,
see the [Design Documentation](design.md).

## Reactive Programming? 

While the design of Responsible was inspired by [ReactiveX](http://reactivex.io/),
it's debatable if Responsible really fits the reactive programming paradigm.
While knowing Rx is not *necessary* for using Responsible,
being familiar with reactive and/or functional programming in C# (e.g. LINQ)
should ease the learning curve.

## Getting Started

* If you are working with Unity, see the [Unity documentation](unity.md).
* If you are working on .NET standard, see the [.NET documentation](dotnet.md).
