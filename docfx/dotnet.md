# Using Responsible With .NET Standard

Responsible was originally written for Unity,
and only later ported to .NET standard,
so there are some things you'll need to set up manually,
depending on the environment you are using Responsible in.

To start using Responsible, you'll need to
* install the package,
* implement a [`ITestScheduler`](xref:Responsible.ITestScheduler), and
* set up your tests using [`TestInstructionExecutor`](xref:Responsible.TestInstructionExecutor).

## Installation

Install the [`Beatwaves.Responsible`](https://www.nuget.org/packages/Beatwaves.Responsible/)
package via NuGet, either from your IDE or the command line, using
```
dotnet add <YouProject.csproj> package Beatwaves.Responsible
```

## Examples

There are currently no vanilla .NET examples,
but you may want to take a look at the simple
[example Unity project](https://github.com/sbergen/responsible/tree/main/com.beatwaves.responsible/Samples~/ResponsibleGame)
with some [basic tests](https://github.com/sbergen/responsible/tree/main/com.beatwaves.responsible/Samples~/ResponsibleGame/PlayModeTests) included.

The library itself also has extensive test coverage,
which may be used as more extensive examples of usage,
but does not represent typical usage of Responsible.
