# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes in state strings or Unity Editor utilities are **not** considered breaking changes.

Everything in the `Responsible.Bdd` namespace is currently considered experimental,
and thus not under semantic versioning.

## [Unreleased]

## [4.4.0] - 2023-03-23

### Added
- `RunAsLoop` and `RunAsSimulatedUpdateLoop` operators, for running test instructions synchronously as a loop with a tick callback.
- `Repeatedly` operator, which turns a test responder into an optional responder that executes repeatedly.

### Changed
- Include hint in state strings about being able to provide extra context to wait conditions
- Improve exception message handling by not truncating long messages (I don't remember why I did this originally)
- Add a bit of padding to the Unity operation status window

### Fixed
- Fix some state strings on Windows, where `Environment.NewLine` is more than one character.
- Fix some cancellation issues with Unity 2021 (where cancellation might happen asynchronously later in the same frame)

## [4.3.0] - 2022-04-17

### Added
- New `GroupedAs` operator to allow more control over the state strings of compound operations.
- Experimental: New BDD-style keywords to build scenarios.
- Experimental: `responsible-gherkin` dotnet tool to convert Gherkin feature specifications into Responsible test stubs.
  This is a separate NuGet package, and will not have a changelog until there's a 1.0 release.
  Feel free to try it out by installing with `dotnet tool install -g Beatwaves.ResponsibleGherkin`

## [4.2.0] - 2021-10-13

### Added
- Allow passing a list of exceptions to be rethrown, instead of wrapped in `TestFailureException`. This allows e.g. NUnit's `IgnoreException` to function properly.

### Fixed
- Long descriptions in the operation state window no longer cause tests to fail because Unity spits out an error about too many vertices in a Label.
- Make NUnit's `Assert.Ignore`, `Assert.Inconclusive`, and `Assert.Pass` work properly on Unity (see Added section above for .NET).

## [4.1.2] - 2021-07-17

### Fixed
- `TestInstructionExecutor.Dispose` no longer throws an exception if called twice.
- `Sequence` will no longer include duplicate entires in the test instruction stack section of error messages.

### Changed
- Use `X s ≈ Y frames` instead of `X s and Y frames` in output to clarify the meaning.
- Add stack trace to `UnhandledLogMessageException.Message`
- Unity: Intercept errors logged from non-Unity threads also.

## [4.1.1] - 2021-05-20

### Fixed
- Fix error handling in `ContinueWith` and `AndThen` to publish the correct exception. This was previously causing an internal exception to be published instead under certain conditions.

### Changed
- Change responder state string from `EXPECTED WITHIN` to `CONDITION EXPECTED WITHIN` to better reflect what it actually does.

## [4.1.0] - 2021-05-13

### Fixed
- Fix description of `Return`.
- Handle source paths not under project gracefully when executing in Unity Editor.
- Fix state string if `ContinueWith` or `AndThen` continuation function throws an exception.

### Changed
- Handle aborting tests better in the Unity state window.
- Simplify unnecessarily complex timeout time formatting.
- Allow passing `IGlobalContextProvider` to `UnityTestInstructionExecutor`.

## [4.0.0] - 2021-03-28
### Changed
- Removed the UniRx dependency, now uses `async/await` internally instead.
- Unity-specific functionality was moved to work through abstractions.
- `Unit`-typed operators now use `object` instead. This works better most of the time, due to covariance!
- Operation state notifications use a callback instead of an observable.

### Removed
- All observable-related functions (`ToObservable`, `WaitForLast`).
- `AsUnit...` operators: you can use `BoxResult` or just implicit covariance instead.

### Added
- Support for .NET standard 2.0 (and NuGet publishing)
- `ToTask` methods, which replace `ToObservable`. Note: If you were using these, you can use the UniRx `ToObservable` method to convert a task to an observable.
- `UnityTestInstructionExecutor`, which should provide the same functionality as `TestInstructionExecutor` did before.
- `BoxResult` operators: replace `AsUnit...` operators when working with value types.
- `WaitForTask` operator.
- Support for adding extra global context into failure messages.

## [3.0.1] - 2021-03-14
### Fixed
- Fix inclusion of documentation in releases, no changes in the library itself.

## [3.0.0] - 2021-03-14
### Added
- HTML documentation generated with DocFX
- Test operation status window in Unity Editor, under `Window/Responsible/Operation State`

### Changed
- Some parameters renamed for better documentation.

### Removed
- Classes and functionality which was intended to be internal were marked as internal.

## [2.0.0] - 2021-02-26
### Changed
- Simplifies the public API by reducing the number of overloads used. This should make auto-completion and compiler errors more friendly.
- Removes `WaitForCondition`, `WaitForConditionOn` and `WaitForCoroutine` overrides, which make a result using a selector: `Select` can be used instead.
- Synchronous `ThenRespondWith` overrides renamed to `ThenRespondWithFunc` and `ThenRespondWithAction`
- Splits `WaitForCoroutine` to two versions: `WaitForCoroutine` and `WaitForCoroutineMethod`.

## [1.2.0] - 2020-12-02
### Fixed
- Fix source context of `Responsible.Do` (`"DoAndReturn"` -> `"Do"`)
- Show canceled wait conditions as canceled instead of running in output. A wait may get canceled e.g. as part of an optional test responder.

### Changed
- Simplify output when `ExpectWithinSecond` is used, by showing `EXPECTED WITHIN ...` on the same line as the expected operation, instead of `EXPECT WITHIN ...` with the operation nested below it. This is only applicable to simple wait conditions and responders.

## [1.1.0] - 2020-11-11
### Added
- `WaitForConditionOn` for a more streamlined syntax for waiting for a condition on some object, and then returning that object (or something derived from that object).
- `WaitForFrames`, for those cases when you actually really need it
- Allows executing `ITestOperationState` using `ToYieldInstruction/Observable`, allowing you to log the state while executing it.
- Sample project with sample tests.

### Fixed
- Add context to output when `ExpectWithinSeconds` times out.
- Fix referential transparency of `WaitForConstraint`: `IResolveConstraint.Resolve()` can mutate the condition and produce weird results when called multiple times.

### Changed
- Makes `...Seconds` operators use `double` instead of `int` (_practically_ backwards compatible).
- Improve documentation, especially about getting started.
- Move tests out of package, so that they don't get included in projects that use Responsible.

## [1.0.0] - 2020-09-23
### Added
- Initial public release with basic functionality.

[Unreleased]: https://github.com/sbergen/Responsible/compare/v4.4.0...HEAD
[4.4.0]: https://github.com/sbergen/Responsible/releases/tag/v4.4.0
[4.3.0]: https://github.com/sbergen/Responsible/releases/tag/v4.3.0
[4.2.0]: https://github.com/sbergen/Responsible/releases/tag/v4.2.0
[4.1.2]: https://github.com/sbergen/Responsible/releases/tag/v4.1.2
[4.1.1]: https://github.com/sbergen/Responsible/releases/tag/v4.1.1
[4.1.0]: https://github.com/sbergen/Responsible/releases/tag/v4.1.0
[4.0.0]: https://github.com/sbergen/Responsible/releases/tag/v4.0.0
[3.0.1]: https://github.com/sbergen/Responsible/releases/tag/v3.0.1
[3.0.0]: https://github.com/sbergen/Responsible/releases/tag/v3.0.0
[2.0.0]: https://github.com/sbergen/Responsible/releases/tag/v2.0.0
[1.2.0]: https://github.com/sbergen/Responsible/releases/tag/v1.2.0
[1.1.0]: https://github.com/sbergen/Responsible/releases/tag/v1.1.0
[1.0.0]: https://github.com/sbergen/Responsible/releases/tag/v1.0.0
