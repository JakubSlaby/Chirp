# CHANGELOG
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.13.0] - 2026-07-13
Allocation and string optimisation pass across the logging hot path. A plain `Chirp.Logger.Log("...")` submitted through the pipeline is now allocation-free on the library side.

### Added
- `ChirpLog` instances are now pooled (per-thread, safe for logging from any thread: main thread, Task thread pool, Unity job workers, UniTask continuations).
- `ChirpLog.Copy()` — creates a non-pooled, caller-owned snapshot for outputs that need to retain log data beyond `Ingest()` (file writers, network batches).
- Editor-only diagnostics: accessing a `ChirpLog` after it was released back to the pool logs an error pointing at `Copy()`. Zero cost in players.
- `Tests/` assembly with per-shape allocation budget tests and markdown golden tests that pin parser output byte-for-byte against the previous implementation.
- Optional `CHIRP_UNITY_INTERNAL_LOG` scripting define: routes console output through Unity's internal `DebugLogHandler.Internal_Log` via a cached reflection delegate, skipping the full-message `string.Format` copy Unity's public `ILogHandler` API performs on every log. Falls back to the public API silently if the internal method is missing (see README for IL2CPP `link.xml` note).

### Changed
- **Lifetime contract**: a `ChirpLog` is valid only until `Submit` returns. `IChirpOutput` implementations must not retain instances beyond `Ingest()` — use `ChirpLog.Copy()`. (Retaining was never supported: previously `Submit` disposed and nulled the log immediately after dispatch.)
- The `[<color=#…>Name</color>] ` channel prefix is now built once per `ChirpLogger` instead of being re-formatted on every log.
- Markdown parser rewritten from Remove/Insert patching to single-pass cursor-walk emission — same output (golden-tested), a fraction of the intermediate string churn, and plain text without markdown characters now bypasses the regex entirely.
- Code-block JSON pretty-printing is only attempted when the block content starts with `{` or `[` — non-JSON blocks no longer pay for a thrown `JsonReaderException` per log. (Bare JSON scalars in code blocks are no longer reformatted.)
- Stack-trace formatter caches per-method signature fragments and avoids per-frame `Substring`/`Replace`/`int.ToString` allocations.
- `ChirpLog.Context` now holds a strong reference for the (sub-millisecond) lifetime of the pooled log instead of allocating a `WeakReference` per contextful log.
- Intercepted `Debug.Log(object)` calls skip the `string.Format` copy for the standard `"{0}"` format.

### Removed
- Finalizers on `ChirpLog` and `ChirpLogger` — every log no longer passes through the GC finalization queue.

## [0.12.0] - 2026-02-23
Better handling of plugin lifecycles.
### Added
- Plugin lifecycle handling
- `Chirp.AddPlugin` and `Chirp.RemovePlugin` API for managing plugins

### Changed
- `IChirpInput` and `IChirpOutput` now are inheriting from IChirpPlugin
- `AbstractChirpPlugin` now has base functionality for disposal handling

### Removed
- Removed `Chirp.AddInput` and `Chirp.AddOutput` API in favour of `Chirp.AddPlugin`


## [0.11.1] - 2026-02-22
Tighter Unity Integration
### Added
- Ability to pass Context as part of string APIs
- Context tracking on ChirpLog
- Add extra AddInput, AddOutput oveloads to Chirp class
- Add RemoveOutput for removing an output

### Changed
- Add Input for UnityConsoleLogger so we can intercept logs as well as push them to the default Console - useful if we want all logs to be redirected
- Rename UnityConsoleOutput to UnityConsolePlugin

## [0.10.1] - 2025-11-15
Unity 6 compatibility
### Fixed
- Fix: Builds failing due to LoggingMarkdownUtil editor testing methods.
- Fix: Editor settings window would throw errors due to changes in embedded icons

## [0.10.0] - 2024-08-26
### Changed
- Updated readme
- Big changes to the structure and way of interacting with the system.
- Use Chirp.Logger.Log/Info/Warn... for logging on the default channel.
- Use Chirp.Channels.Create() to create a specific channel logger.

### Added
- Strings can now be converted to ChirpLogs by calling `"Some string".AsChirpLog()` and provide extra options.
- `"Some string".AsChirpLog().AsMarkdown()` will indicate that the logs have any of the supported Markdown tags
- Use ChirpLogs when logging `Chirp.Logger.Log("Some string".AsChirpLog().AsMarkdown())`
- Use `Chirp.Logger.Log("Some string".AsMarkdownLog())`
- You can use any object and convert it to a json representation in markdown `Chirp.Logger.Log(someObjectInstance.AsChirpLog())`

### Removed
- All "Ch" APIs: `DebugCh`, `LogCh`, `InfoCh`, `WarningCh`, `ErrorCh`, `ExceptionCh` - moving towards dedicated channel loggers

## [0.9.1] - 2023-05-24
### Added
- LogChannel instance can now be used to invoke specific logs with that Channel

### Changed 
- New Unity no longer allows you to paste stack traces in the message - reverted back to using UnityEngine native console stack trace.
- You can select Console > Strip Logging Callback to strip unnecessary stack trace entries.

### Fixed
- Fix: Formatting of Unity logs with {} will no longer cause exceptions

## [0.9.1] - 2021-08-18
### Changed
- Docs update, added screenshots

## [0.9.0] - 2021-04-25
### Added
- Chirp Initializer and component structure for creating easy initialization objects.

### Changed
- Added conditional to Chirp.Initialize() method.
- Fixes to how Chirp interacts with Unity Debug Logging methods.
- Fixes to Stack Trace construction

## [0.8.1] - 2021-04-17
### Changed
- Fix for the default order of execution between Awake and RuntimeInitializeOnLoadMethod attributed methods.

## [0.8.0] - 2021-04-11
### Added
- Added a Project Settings window for configuring Chirp per target platform.
- Added CHIRP script define for enabling the framework.

### Changed
- Updated the readmes and install instructions.
- Exceptions logging is always enabled when Chirp is enabled.
- Errors and Exceptions added to conditional logging customisation.

## [0.7.0] - 2021-03-14
- Initial release of Chirp framework.
