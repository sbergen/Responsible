# Responsible - Reactive Asynchronous Testing

[![License](https://img.shields.io/github/license/sbergen/Responsible.svg)](https://github.com/sbergen/Responsible/blob/main/LICENSE)
[![CI status](https://github.com/sbergen/Responsible/workflows/CI/badge.svg)](https://github.com/sbergen/Responsible/actions?query=workflow%3ACI+branch%3Amain+)
[![codecov](https://codecov.io/gh/sbergen/Responsible/branch/main/graph/badge.svg)](https://codecov.io/gh/sbergen/Responsible)
![Last commit](https://img.shields.io/github/last-commit/sbergen/Responsible)  
[![openupm](https://img.shields.io/npm/v/com.beatwaves.responsible?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.beatwaves.responsible/)
[![Releases](https://img.shields.io/github/v/release/sbergen/Responsible)](https://github.com/sbergen/Responsible/releases)
[![Release date](https://img.shields.io/github/release-date/sbergen/Responsible)](https://github.com/sbergen/Responsible/releases)

*Responsible* is an automated testing utility primarily designed for,
but not limited to be used in high level system tests in [Unity](https://unity.com/).
It is built on top of [UniRx](https://github.com/neuecc/UniRx) and [NUnit](https://nunit.org/).
Responsible currently runs only in Unity,
but could easily be [ported](Packages/Responsible/Runtime/README.md#portability) to work in other .NET environments also.

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
    .ThenRespondWith("Foo the bar", Do("Consume bar", () => foo.Consume(bar)))
    .ExpectWithinSeconds(10)
    .ContinueWith(Do("Continue operation", foo.ContinueOperation))
    .ToYieldInstruction(this.TestInstructionExecutor);
```

If `foo.Consume` were to throw an error, the output could look something like this:
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

## Getting Started

To start using Responsible, you'll need to install the package and
reference the `Responsible` assembly from your test assemblies.
Since `Unit` is quite commonly used in Responsible,
no effort has been made to avoid having to reference UniRx also.

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

### Installation Using OpenUPM

After [setting up OpenUMP](https://openupm.com/docs/),
you can add Responsible to your project by running
```
openupm add com.beatwaves.responsible
``` 

### Installation Without a Package Manager

If you can't or don't want to use the package manager, you can also directly incorporate
the [Packages/Responsible/Runtime](Packages/Responsible/Runtime) directory into your project.
This could be done manually, or e.g. with a symlink and git submodule.

If you are not already using UniRx, you'll need to install it from the Asset Store,
or in a similar fashion to what is outlined above.

### Referencing the Assembly

With either installation option, you'll end up with an assembly called `Responsible`,
with the `UNITY_INCLUDE_TESTS` define constraint.
To use Responsible, simply reference this from your test assemblies.
You'll also need to reference UniRx to use many features of Responsible.

### Examples

For a simple example of a basic test setup, 
there's an [example project](Packages/Responsible/Samples~/ResponsibleGame)
with some [basic tests](Packages/Responsible/Samples~/ResponsibleGame/PlayModeTests) included.
This sample game can be installed from the Unity Package Manager,
and is also symlinked into the main Unity project in this repository, for convenience.

The library itself also has [extensive test coverage](Assets/Responsible/Tests/Runtime),
which may be used as more extensive examples of usage.
Note that these tests live outside of the package,
so that they do not get included into projects referencing Responsible.
(This has to do with undocumented reasons related to using `"type": "tests"` in `package.json`).

## Design Documentation

To read more about the design principles behind Responsible, and how it works,
see the [Design Documentation](Packages/Responsible/Runtime/README.md).
