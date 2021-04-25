# CHANGELOG
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


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
