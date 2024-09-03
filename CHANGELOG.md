# CHANGELOG
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
