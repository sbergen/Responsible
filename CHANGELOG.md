# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- HTML documentation generated with DocFX
- Test operation status window in Unity Editor, under `Window/Responsible/Operation State`

### Changed
- Some parameters renamed for better documentation.

### Removed
- Classes and functionality which was intended to be internal, was marked as internal.

## [2.0.0] - 2020-02-26
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

[Unreleased]: https://github.com/sbergen/Responsible/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/sbergen/Responsible/releases/tag/v2.0.0
[1.2.0]: https://github.com/sbergen/Responsible/releases/tag/v1.2.0
[1.1.0]: https://github.com/sbergen/Responsible/releases/tag/v1.1.0
[1.0.0]: https://github.com/sbergen/Responsible/releases/tag/v1.0.0