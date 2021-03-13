# Chirp - Unity Logging


## Overview
Simple Unity logging framework easily exandable for custom functionality
- Log Channels for easy message categorisation
- Customisable Loggers allow for custom processing
- Default Unity console integration

## Usage
Chirp Logger hooks up to the default unity `Debug.Log` methods but also allows you to use it's dedicated API.
To start using Chirp, initialise it with the desired Loggers.

> Examples use an additional integration with AssetStore packages: [Quantum Console](https://assetstore.unity.com/packages/tools/utilities/quantum-console-128881), [SRDebugger](https://assetstore.unity.com/packages/tools/gui/srdebugger-console-tools-on-device-27688)

### Initialisation
Call the initialise method at the beginning of your runtime code and add `LogLevelDebug` to ProjectSettings/Player/Script Define Symbols (read more about Conditional Compilation).
```csharp
Chirp.Initialise(new UnityConsoleLogger(), new QuantumConsoleLogger());
```


### Logging
The default log API with simple message and stack trace functionality.
```csharp
Chirp.Debug("Debug Message"); // Detailed logs, best for cases like logging rpc responses or method outputs.
Chirp.Log("Log Message"); // Typical log message, most common use case.
Chirp.Info("Info Message"); // State change or any significant message that would have less detailed data.
Chirp.Warning("Warning Message");
Chirp.Error("Error message");
Chirp.Exception(new Exception(), "Exception Message");
```
![Log Example](Images/examples-default.png)

### Channels
Add Log Channel identifier for specifying the source or context of the log so that it's easily recognisable in console.
```csharp
Chirp.DebugCh("Inventory","Debug Message");
Chirp.LogCh("ConnectionResolver", "Log Message");
Chirp.InfoCh("SaveManager", "Info Message");
Chirp.WarningCh("PlayerController","Warning Message");
Chirp.ErrorCh("SaveManager","Error message");
Chirp.ExceptionCh( "PlayerController", new Exception(), "Exception Message");
```
![Log Example](Images/examples-channel.png)

> Quantum Console needs aditional integration for custom formatting, filtering and Stack Trace lookups.<br/>
> SRDebugger has built in filtering by text.

#### Automatic channels
You can automatically detect channels based on class Types that are found in the stack trace. To tag a type as a LogChannel all you need to do is add the `[LogChannel]` attribute and generate list of channels.
```csharp
[LogChannel]
public class PlayerController
{
  // ...
}
```
You can find the list generator under `Tools/Chirp Logger/Generate Log Channels List` menu.

## Conditional Compilation
All Chirp log methods can be compiled conditionally allowing you to easily remove calls to these methods while compiling.
Each log method corelates to a Log Level, which are:
```csharp
Debug = 0,
Log = 1,
Info = 2,
Warning = 3,
Assert = 4,
Error = 5,
Exception = 6
```
The script defines corelate to Log Levels
```
LogLevelDebug // Debug and above
LogLevelDefault // Log and above
LogLevelInfo // Info and above
LogLevelWarning // Warning and above
LogLevelAssert // Assert and above
```
Additionally you can use numbered defines
```
LogLevel0 // Debug and above
LogLevel1 // Log and above
LogLevel2 // Info and above
LogLevel3 // Warning and above
LogLevel4 // Assert and above
```
Conditional compilation is very useful when preparing release builds and dyou don't want to include debug logs.

## Integrations
Thanks to it's simple structure Chirp Logging is highly customisable. Most extensions relying on the default Console will work out of the box.

### Quantum Console Integration
I have prepared additional integration with Quantum Console allowing for more detailed information, filtering and search. The full upgrade instructions will be published soon.


<p align="right">
  <a href="https://www.twitch.tv/sparrowgamedev">
    <img alt="Twitch Status" src="https://img.shields.io/twitch/status/SparrowGameDev?style=social">
  </a>
  <a href="https://twitter.com/jakubslaby">
    <img alt="Twitter Follow" src="https://img.shields.io/twitter/follow/JakubSlaby?style=social">
  </a>
</p>
