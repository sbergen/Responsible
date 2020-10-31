# Responsible - Reactive Asynchronous Testing

*Responsible* is an automated testing utility primarily designed for,
but not limited to be used in high level system tests in [Unity](https://unity.com/).
It is built on top of [UniRx](https://github.com/neuecc/UniRx) and [NUnit](https://nunit.org/).
Responsible currently runs only in Unity,
but could easily be [ported](#portability) to work in other .NET environments also.

The primary benefits of using Responsible are:
* Detailed output on test failures and timeouts
* Declarative, composable, and reusable test code

Additionally, using Responsible will circumvent a
[Unity bug](https://issuetracker.unity3d.com/issues/unitytests-do-not-fail-when-nested-coroutines-throws-an-exception),
where test execution will continue after errors within nested coroutines,
sometimes even hiding the first exception.

While the design of Responsible was inspired by [Rx](http://reactivex.io/),
and it is being used under the hood,
knowing Rx is not *necessary* for using Responsible.
However, being familiar with reactive and/or functional programming in C# (e.g. LINQ)
should ease the learning curve.

## Usage and Error Output Example

Here's a simple example of what using Responsible could look like
(with `using static Responsible.Responsibly;`) 

```cs
yield return WaitForCondition("Foo to be ready", () => foo.IsReady)
    .AndThen(WaitForCondition("Bar to be completed", () => bar.IsCompleted))
    .ThenRespondWith("Consume bar", _ => foo.Consume(bar))
    .ExpectWithinSeconds(10)
    .ContinueWith(Do("Continue operation", foo.ContinueOperation))
    .ToYieldInstruction(this.TestInstructionExecutor);
```

If `foo.Consume` were to throw an error, the output could look something like this:
```
Test operation execution failed!
 
Failure context:
[!] EXPECT WITHIN 0:00:10 (Failed after 0.10s and 7 frames)
  [!] Consume bar (Failed after 0.00s and 0 frames)
    WAIT FOR
      [✓] Foo to be ready (Completed in 0.06s and 3 frames)
      [✓] Bar to be completed (Completed in 0.04s and 4 frames)
    THEN RESPOND WITH
      [!] Consume bar (Failed after 0.00s and 0 frames)
 
        Failed with:
          System.Exception: 'Something failed'
 
        Test operation stack:
          [ThenRespondWith] MethodName (at Path/To/Source.cs:36)
          [ExpectWithinSeconds] MethodName (at Path/To/Source.cs:37)
          [ToYieldInstruction] MethodName (at Path/To/Source.cs:38)
 
[ ] Continue operation
 
Error: System.Exception: Something failed
  at <normal exception stack trace>
```

## Getting Started

To start using Responsible, you'll need to install the package and
reference the `Responsible` assembly from your test assemblies.

### Installation Using the Unity Package Manager

If you are on Unity Unity 2019.3.4f1, 2020.1a21 or later,
you can add the following to `Packages/manifest.json`:
```json
"com.beatwaves.responsible": "https://github.com/sbergen/Responsible.git?path=/Packages/Responsible"
``` 
if you are not already using UniRx, you may install that via the package manager also,
by adding also the following line:
```json
"com.neuecc.unirx": "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts"
```

### Installation Without the Unity Package Manager

If you can't or don't want to use the package manager, you can also directly incorporate
the `Packages/Responsible/Runtime` directory into your project.
This could be done manually, or e.g. with a symlink and git submodule.

### Referencing the Assembly

With either installation option, you'll end up with an assembly called `Responsible`,
with the `UNITY_INCLUDE_TESTS` define constraint.
To use Responsible, simply reference this from your test assemblies.

## Design

The primary motivation for building Responsible was the idea of *responders*,
which form an asynchronous if-then operation.
The *if* part is implemented as a *wait condition*,
and the *then* part as an *instruction*.
Both responders and wait conditions can be *expected*, turning them into instructions.
One or more responders can additionally be reacted to optionally,
until another condition is met, or another responder is ready to execute.
Operations can be combined and chained using operators.

### Core Principles

The core design principles of Responsible are:
* **Useful output on failures**:
  You are required to name your operations,
  and full information on operations is provided on failures and timeouts.
* **Manageable test execution times**:
  Tests using Responsible will terminate on the first failure,
  and providing timeouts is mandatory.
  This leads to as-early-as-possible failures, keeping test execution times manageable.
  This is especially important for large test suites being run on CI.
* **Operations as data**: Building and composing operations is
  [referentially transparent](https://en.wikipedia.org/wiki/Referential_transparency),
  making code declarative, reusable, and easy to reason about.
  When an operation is executed, a separate object representing its state is created.
* Responsible is an **extendable execution engine**:
  Responsible provides the mechanism for declaring operations,
  and operators to combine operations.
  Responsible does *not* contain a collection of useful operations,
  but instead provides mechanisms to build your own.

### Types of operations

Responsible is built on four different operation types,
which can be combined in multiple ways, using operators.

**About `Unit` and not returning values:** If you are not familiar with Rx,
you might find the concept of using `Unit` as a return value confusing at first.
`Unit` represents an *empty* value (defined as the empty tuple in many languages).
Using `Unit` vastly simplifies building generic operations which may or may not return a useful value,
as you don't need overrides for functions returning void (e.g. `Action` vs `Func`).
You can treat `Unit` very similarly to `void` except that it *is* a value.

#### Instructions

Instructions are represented by the `ITestInstruction<T>` interface.
They represent a synchronous or asynchronous operation producing a single result.
Instructions can be executed by a `TestInstructionExecutor`,
either as a yield instruction, or as an observable,
using the `ToYieldInstruction` and `ToObservable` extension methods.
All instructions should have an internal timeout.
The basic operations for chaining instructions are `ContinueWith` and `Sequence`.

#### Wait Conditions

Wait conditions are represented by the `ITestWaitCondition<T>` interface.
A wait condition produces a single result,
which can be extremely useful when chaining wait conditions and building responders.
The basic operations on conditions are chaining them using `AndThen`,
converting to a responder using `ThenRespondWith`,
or into an instruction with `ExpectWithinSeconds`.

#### Responders

A responder is represented by the `ITestResponder<T>` interface.
A responder produces a single result,
and is usually built from a wait condition and an instruction,
using the `ThenRespondWith` operator.
This forms an if-then relationship between the wait condition and instruction.
Responders are guaranteed to either not be triggered at all, or be fully executed.

#### Optional Responders

Optional responders are represented by the `IOptionalTestResponder` interface.
Unlike the other operation types, optional responders do not produce a result,
as handling these results would get complicated due to their optional nature
(this is something that may be implemented later).
One or more responders can be optionally executed using the `RespondToAnyOf` operator,
or the `Optionally` extension method, which which can be applied to a single responder.
Optional responders can be executed until a wait conditions becomes fulfilled,
using the `Until` operator, or until some other responder becomes ready to execute,
using the `UntilReadyTo` operator.

### Operators

The class `Responsibly` contains the basic operators for creating operations,
and a bunch of operators for combining operations,
which are not idiomatic extension methods.
Following the pattern used by e.g. `IEnumerable<T>` and `IObservable<T>`,
all extension methods on operators can be found in a similarly named class.
E.g. `TestInstruction` for extension methods on `ITestInstruction<T>`.

Some operators worth mentioning are the `AsUnit...` and `Select` operators,
which are very useful in conjunction with operators requiring operators of the same type,
such as `RespondToAllOf` and `WaitForAllOf`.

## Extending Responsible

While Responsible is already useful on it's own, it is built to be extensible,
and becomes even more powerful when you write your own operators on top of it.
It's recommended to build your own wait conditions and instructions for common use cases
by wrapping or combining the basic operations.
You could e.g. create a method with the following signature:  
`ITestWaitCondition<GameObject> WaitForDescendantByNameToBeActive(GameObject gameObject, string name)`.

You may want to pass in explicit `CallerMemberName` etc. arguments to your own low-level operations
to capture the original caller location.
Putting these operations into their own file and
suppressing warnings about explicit argument use is recommended.

The methods that build wait conditions take an argument for creating extra context.
The provided function will be called on failures
(or when [manually logging state](#tips-and-tricks))
to create extra details that might help figuring out why a wait timed out.
An example use would be to e.g. list all open menus in your menu system,
when waiting for a specific menu to be open.

## About TestInstructionExecutor

Passing in a `TestInstructionExecutor` instance for running instructions is mandatory.
There are a few reasons for this:
* You may want to customize your executor for different tests (see below).
* The UniRx `EveryUpdate` method has some internal delays when invoked for the first time.
  For this reason, an internal proxy is used in the executor to speed up execution.
* If your code tears down everything, including `DontDestroyOnLoad` objects,
  the executor may stop functioning after such tear down (again due to UniRx internals).

The `TestInstructionExecutor` constructor takes three optional arguments:
* `IScheduler scheduler`, which is `UniRx.Scheduler.MainThread` by default.
  The scheduler is used for timeouts, keeping time for output purposes,
  and in the `WaitFor` operator.
  If for any reason you wish to keep track of time in a differently,
  you may override this default.
* `IObservable<Unit> pollObservable`,
  which is `UniRx.Observable.EveryUpdate().AsUnitObservable()` by default.
  If for any reason you don't want to poll for conditions on every frame,
  you may override this default.
* `ILogger logger`, which is `UnityEngine.Debug.unityLogger` by default.
  The logger is used to log an error in addition to throwing an exception on failures.
  This is due to the way Unity does not handle errors in nested coroutines very well.
  If you are using Responsible at every level of your tests,
  you may wish to not log these errors,
  which can be accomplished by implementing a no-op logger.

### Unity Error Log Handling

The Unity Tests Runner will fail tests on logged errors.
While running a test instruction, Responsible will intercept this process,
by listening to logged errors, and overriding the default behavior.
The following points are worth noting:
* If you are expecting an error, you must call `TestInstructionExecutor.ExpectLog` 
  instead of only calling `LogAssert.Expect`.
* Logging errors from any other thread but the main thread is **not** handled.
  As test instructions are executed on the main thread,
  it would be impossible to attribute a logged error to the correct test instruction.
* `LogAssert.ignoreFailingMessages` is respected at the time of logging.

## Tips and Tricks

If the method names in the `Responsibly` class do not conflict with anything else,
using `using static Responsible.Responsibly;` can make your code more concise.
The method names were designed to work well like this. 

All the basic operations (excluding optional responders) implement the
`ITestOperation<T>` interface, which contains the `CreateState` method,
producing a `ITestOperationState<T>` instance.
All implementations of this interface declare a `ToString` override,
which is used to produce the failure output.
If you wish to produce output during execution,
you may manually call `CreateState`, start executing it using a `TestInstructionExecutor`,
and periodically log the state of the operation.

## For Imperative Programmers

You will notice that `Func<T>` and `Func<T1, T2>` are used a lot in Responsible.
This is because test operations are not executed on creation,
but only once you actually request an execution.
This means that the same *instance* of an operation can be reused,
meaning the following code will work as expected:
```cs
var waitForFoo = WaitForFoo(...);
var waitForBar = WaitForBar(...);
var waitForQuux = WaitForQuux(...);

var waitForFooAndBar = waitForFoo.AndThen(waitForBar);
var waitForFooAndQuux = waitForFoo.AndThen(waitForQuux);
``` 
Here, `waitForFoo` is being reused in two different contexts,
and reusing e.g. `waitForFooAndBar` later is also fine.
What happens under the hood, is that for each *execution* of an operation,
a separate state object will be created.

What this means in the context of building your own operations,
is that you must not read or write any state without deferring the operation.
E.g. the following will **not** work as expected:
```cs
ITestWaitCondition<T> WaitForFoo(...)
{
    var foo = GameObject.Find("Foo");
    return WaitForCondition(
        "Foo to be ...",
        () => foo...);
}
```
while the following will:
```cs
ITestWaitCondition<T> WaitForFoo(...)
{
    return WaitForCondition(
        "Foo to be ...",
        () => GameObject.Find("Foo")...);
}
```

## For Functional Programmers

Test operations are stateless and referentially transparent.
Calling `CreateState` could be considered a form of reification,
producing a stateful instance of the abstract operation.
When building custom operations, ensure that you defer any stateful operations
(both reading and writing state).

`ITestInstruction<T>` ended up being a monad,
where `Return` is `return` (obviously), and `ContinueWith` is `bind`.
This may not have any practical implications in C# beyond being able to use the LINQ query syntax.

## LINQ query syntax

While not recommended, due to it muddling call-site details,
it is possible to use the LINQ query syntax with test instructions.
For example, the following query will return an instruction returning 10:
```cs
from a in Return(2)
from b in Return(3)
let c = a + b
from result in Return(2 * c)
select result;
```

While the example uses simple `Return` calls for brevity,
this syntax will work for sequencing asynchronous instructions also.

## Portability
 
The core functionality of Responsible does't really depend on anything Unity-specific.
Only the following details should need changing to port Responsible to some other .NET environment: 
* `using UniRx` directives would need replacing with standard Rx directives.
* `TestInstructionExecutor` uses the Unity `ILogger` interface, which would need replacing.
* `ToYieldInstruction`: not needed outside of Unity
* `WaitForCoroutine`: not needed outside of Unity
* Tests are written using the `UnityTest` attribute and `yield return null`,
  to best mimic the environment the library would actually be used in.
* The `IScheduler` interface is slightly different in UniRx than in standard Rx,
  and the tests use a custom `TestScheduler`.