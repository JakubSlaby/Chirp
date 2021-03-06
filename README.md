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
```csharp
Chirp.Initialise(new UnityConsoleLogger(), new QuantumConsoleLogger());
```
### Logging
The default log API with simple message and stack trace functionality.
```csharp
Chirp.Debug("Debug Message");
Chirp.Log("Log Message");
Chirp.Warning("Warning Message");
Chirp.Error("Error message");
Chirp.Exception(new Exception(), "Exception Message");
```
![Log Example](Images/examples-default.png)

### Channels
Additional Log Channel identifier for specifying the source or context of the log so that it's easily identifiable in console.
```csharp
Chirp.DebugCh("Inventory","Debug Message");
Chirp.LogCh("ConnectionResolver", "Log Message");
Chirp.WarningCh("PlayerController","Warning Message");
Chirp.ErrorCh("SaveManager","Error message");
Chirp.ExceptionCh( "PlayerController", new Exception(), "Exception Message");
```
![Log Example](Images/examples-channel.png)

> Quantum Console needs aditional integration for custom formatting, filtering and Stack Trace lookups.<br/>
> SRDebugger has built in filtering by text.



<p align="right">
  <a href="https://www.twitch.tv/sparrowgamedev">
    <img alt="Twitch Status" src="https://img.shields.io/twitch/status/SparrowGameDev?style=social">
  </a>
  <a href="https://twitter.com/jakubslaby">
    <img alt="Twitter Follow" src="https://img.shields.io/twitter/follow/JakubSlaby?style=social">
  </a>
</p>

## Integrations
Thanks to it's simple structure Chirp Logging is highly customisable. Most extensions relying on the default Console will work out of the box.

### Quantum Console Integration
I have prepared additional integration with Quantum Console allowing for more detailed information, filtering and search. The full upgrade instructions will be published soon.
