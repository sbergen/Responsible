# Design Overview

The primary motivation for building Responsible was the idea of *responders*,
which form an asynchronous if-then operation.
The *if* part is implemented as a *wait condition*,
and the *then* part as an *instruction*.
Both responders and wait conditions can be *expected*, turning them into instructions.
One or more responders can additionally be reacted to optionally,
until another condition is met, or another responder is ready to execute.
Operations can be combined and chained using operators.

## Core Principles

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
  Responsible does *not* contain a large collection of high-level operations,
  but focuses on useful low-level operations,
  which can be composed to build your own high-level operations.

## Types of Operations

Responsible is built on four different operation types,
which can be combined in multiple ways, using operators.

**About the use of `object` and covariance:**
Some operations in Responsible return an undefined non-null `object` instance.
Using this approach vastly simplifies the implementation,
as you don't need separate overloads for operations returning values and operations not returning values
(similar to `Func` vs `Action` or `Task<T>` vs `Task`).
Because all the operation interfaces are covariant,
you can easily use a more derived operation where a less derived type is required.
However, if you are working with value types,
you may need to use the `BoxResult` operator to convert the type to `object`.

### Instructions

Instructions are represented by the
[`ITestInstruction<T>`](xref:Responsible.ITestInstruction`1) interface.
They represent a synchronous or asynchronous operation producing a single result.

Instructions can be executed by a
[`TestInstructionExecutor`](xref:Responsible.TestInstructionExecutor)
as an asynchronous task using the `ToTask` extension method.
On Unity you can also use the `ToYieldInstruction` extension method,
to get a yield instruction for a `[UnityTest]` play mode test.
Additionally, the
[`RunAsSimulatedUpdateLoop`](xref:Responsible.TestInstruction.RunAsSimulatedUpdateLoop)
and [`RunAsLoop`](xref:Responsible.TestInstruction.RunAsLoop)
operators can be used to synchronously run instructions in a loop,
using a user-provided tick callback to simulate step-by-step updates.

All instructions should have an internal timeout.
The basic operations for chaining instructions are `ContinueWith` and `Sequence`.
See [`TestInstruction`](xref:Responsible.TestInstruction) for all extension methods.

### Wait Conditions

Wait conditions are represented by the
[`ITestWaitCondition<T>`](xref:Responsible.ITestWaitCondition`1) interface.
A wait condition produces a single result,
which can be extremely useful when chaining wait conditions and building responders.
The basic operations on conditions are chaining them using `AndThen`,
converting to a responder using `ThenRespondWith`,
or into an instruction with `ExpectWithinSeconds`
See [`TestWaitCondition`](xref:Responsible.TestWaitCondition) for all extension methods.

### Responders

A responder is represented by the
[`ITestResponder<T>`](xref:Responsible.ITestResponder`1) interface.
A responder produces a single result,
and is usually built from a wait condition and an instruction,
using the `ThenRespondWith` operator.
This forms an if-then relationship between the wait condition and instruction.
Responders are guaranteed to either not be triggered at all, or be fully executed.
See [`TestResponder`](xref:Responsible.TestResponder) for all extension methods.

### Optional Responders

Optional responders are represented by the
[`IOptionalTestResponder`](xref:Responsible.IOptionalTestResponder) interface.
Unlike the other operation types, optional responders do not produce a result,
as handling these results would get complicated due to their optional nature
(this is something that may be implemented later).
One or more responders can be optionally executed using the `RespondToAnyOf` operator,
or the `Optionally` extension method, which can be applied to a single responder.
Optional responders can be executed until a wait conditions becomes fulfilled,
using the `Until` operator, or until some other responder becomes ready to execute,
using the `UntilReadyTo` operator.
See [`OptionalTestResponder`](xref:Responsible.OptionalTestResponder) for all extension methods.

## Operators

The class [`Responsibly`](xref:Responsible.Responsibly)
contains the basic operators for creating operations,
and a bunch of operators for combining operations,
which are not idiomatic extension methods.
Following the pattern used by e.g. `IEnumerable<T>` and `IObservable<T>`,
all extension methods on operations can be found in a similarly named class.
E.g. `TestInstruction` for extension methods on `ITestInstruction<T>`.

Some operators worth mentioning separately, are the `BoxResult` and `Select` operators,
which are very useful in conjunction with operators requiring operators of the same type,
such as `RespondToAllOf` and `WaitForAllOf`.
However, note that due to covariance,
more derived types can be used where a less derived type is expected.

# Extending Responsible

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

# About ITestScheduler

Most wait conditions in Responsible are built using polling.
Implementing polling requires the possibility to register poll callbacks, and the
[`ITestScheduler`](xref:Responsible.ITestScheduler) interface is used to achieve this.
* When working in Unity, the main thread `Update` event is used by default.
* If you are working in a non-Unity environment,
some kind of main event loop is expected to exist,
and you should implement you scheduler to poll from that event loop.

# Thread safety

Responsible was designed for single-threaded use.
As all operations and operators are pure, they should be thread-safe,
but once you start executing instructions,
everything is expected to happen in a single thread.

# Tips and Tricks

If the method names in the [`Responsibly`](xref:Responsible.Responsibly)
class do not conflict with anything else,
using `using static Responsible.Responsibly;` can make your code more concise.
The method names were designed to work well like this.

All the basic operations (excluding optional responders) implement the
`ITestOperation<T>` interface, which contains the `CreateState` method,
producing a `ITestOperationState<T>` instance.
All implementations of this interface declare a `ToString` override,
which is used to produce the failure and state window output.
If you wish to produce output at specific points during test execution,
you may manually call `CreateState`, start executing it using a `TestInstructionExecutor`,
and log the state of the operation at a specific time.

# For Imperative Programmers

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

# For Functional Programmers

Test operations are stateless and referentially transparent.
Calling `CreateState` could be considered a form of reification,
producing a stateful instance of the abstract operation.
When building custom operations, ensure that you defer any stateful operations
(both reading and writing state).

`ITestInstruction<T>` ended up being a monad,
where `Return` is `return` (obviously), and `ContinueWith` is `bind`.
This may not have any practical implications in C# beyond being able to use the LINQ query syntax.

# LINQ query syntax

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
