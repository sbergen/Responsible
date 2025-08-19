# Responsible - Reactive Asynchronous Testing

[![License](https://img.shields.io/github/license/sbergen/Responsible.svg)](https://github.com/sbergen/Responsible/blob/main/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Beatwaves.Responsible)](http://nuget.org/packages/Beatwaves.Responsible)
[![openupm](https://img.shields.io/npm/v/com.beatwaves.responsible?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.beatwaves.responsible/)

*Responsible* helps you write maintainable high level asynchronous tests in C#:
* Get highly readable and informative output on test failures and timeouts
* Write declarative, composable, and reusable test code

Additionally, in [Unity](https://unity.com/):
* Observe test execution progress while they are running in the Editor
* Stop worrying about a specific long-standing [Unity bug](https://issuetracker.unity3d.com/issues/unitytests-do-not-fail-when-nested-coroutines-throws-an-exception)

Responsible now also has [experimental support](https://www.beatwaves.net/Responsible/godot.html) for [Godot](https://godotengine.org/)!

## Documentation

Extensive documentation is available at the
[documentation site](https://sbergen.github.io/Responsible/index.html):
* [Overview](https://sbergen.github.io/Responsible/index.html)
* [.NET Documentation](https://sbergen.github.io/Responsible/dotnet.html)
* [Unity Documentation](https://sbergen.github.io/Responsible/unity.html)
* [Experimental Godot support](https://www.beatwaves.net/Responsible/godot.html)
* [Design Documentation](https://sbergen.github.io/Responsible/design.html)
* [API Reference](https://sbergen.github.io/Responsible/api/Responsible.html)
* [Changelog](https://sbergen.github.io/Responsible/CHANGELOG.html)

The online documentation is created from the main branch.
Starting with version 3.0.1, the documentation for specific versions is also available as static HTML
in [releases](https://github.com/sbergen/Responsible/releases).

## Questions? Ideas?

If you have any questions or ideas, don't hesitate to head over to the
[GitHub Discussions](https://github.com/sbergen/Responsible/discussions)!


## Repository Structure

Due to Responsible targeting both Unity and .NET,
the repository structure is a bit unorthodox:
* `com.beatwaves.responsible` contains the Unity Package.
* `com.beatwaves.responsible/Runtime` contains the main runtime, shared with .NET.
  * Unity files are excluded in the `.csproj` file.
* `src` contains the .NET solution and .NET testing/CI related files.
* `ResponsibleUnity` contains the Unity project and Unity-specific tests.
* `src/Responsible.Tests` contains the tests for the pure .NET classes.
  * `package.json` is used for including them in the Unity project as a package.
  * `Responsible.Tests.csproj` is used for including them in the main .NET solution.
