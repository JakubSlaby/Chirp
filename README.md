# Chirp - Unity Logging

## Overview
Simple Unity logging framework easily extendable for custom functionality
- Channels (loggers) for easy message categorisation
- Custom outputs for exporting logs to different targets
  - Only Unity console export for now
- Markdown support for log formatting in Unity Console
- Simplified object property rendering

## Installation
Best way to install Chirp is to download the latest package from [Releases](https://github.com/JakubSlaby/Chirp/releases), full source code is included in the UnityPackage.
Import the package in to your Unity project through `Assets/Import Package/Custom Package`.

## Setup
The framework needs to be enabled for each desired target platform through the settings window. You can find it by opening `Tools/Chirp Framework/Chirp Settings` or by navigating directly to Project Settings window.

## Initialisation through a component
To quickly initialise the logging framework you can create a Chirp Initializer game object in your scene.
Simply go to `Tools/White Sparrow/Chirp Logger/Create Initializer Object`.

![Log Example](Images/package-component.jpg)

Enable the available components (by default it comes with UnityConsoleLogger) and enjoy!

### Initialisation From Code
If you don't want to use the Component initialisation method you can register desired outputs from code.
```csharp
public class Bootstrap : MonoBehaviour
{
  private void Awake()
  {
    Chirp.AddOutput<UnityConsoleOutput>();
  }
}
```

Or use the `Chirp Initializer` GameObject Component to do that automatically. You can create it through `Tools/Chirp Logger/Create Initializer Object`.

# Logging
The default log API with simple message and stack trace functionality.

## Default Logger
```csharp
Chirp.Logger.Debug("Simple debug message");
Chirp.Logger.Log("Little more information needed");
Chirp.Logger.Info("Player has started playing a hurdy-gurdy");
Chirp.Logger.Warning("Some NPCs don't like the song player picked");
Chirp.Logger.Error("Player got in to a fight with NPCs over his choice of entertainment");

```

## Custom Channel Loggers
You can create custom channel loggers to provide more context about the context of logs.
```csharp
var barLogger = Chirp.Channels.Create("Bar", Color.cyan);

barLogger.Debug("Simple debug message");
barLogger.Log("Little more information needed");
barLogger.Info("Player has started playing a hurdy-gurdy");
barLogger.Warning("Some NPCs don't like the song player picked");
barLogger.Error("Player got in to a fight with NPCs over his choice of entertainment");
```

Logs from a channel will add the channel name as metadata. In Unity Console logs will be displayed with a prefix
```
[Bar] Player has started playing a hurdy-gurdy
```
![Log Markdown](Images/log-channel.jpg)


### Log Objects
Each log passed through to Chirp is converted to a ChirpLog object which has additional data like timestamp or stack trace added.
You can convert strings in to ChirpLog objects ahead of time as well.

```csharp
var log = "A message in the bottle".AsChirpLog();
```

# Logs with Markdown
Unity Console supports some of the simpler rich text elements, I've added a custom Markdown parser that will allow you to use basic Markdown tags to simplify customised log creation.

```csharp
Chirp.Logger.Log(@"### User Properties
Health: [c:Green]100[/c]
Strength: **20**
Waffles: [c:red]1[/c]
*Health and Strength are important, but so are waffles*".AsChirpLog().AsMarkdown());
```

![Log Markdown](Images/log-markdown.jpg)

### Inline Code and Code Block
I've also added simple support for inline code and code block support.

```csharp
Chirp.Logger.Log(@"Simple inline code `can be formatted *without nested markdown execution*`

Code block can prettify json and add a title
```Pretty Json
{""name"":""Edward Teach"",""alias"":""Blackbeard"",""nationality"":""English"",""active_years"":""1716-1718"",""ship"":""Queen Anne's Revenge"",""notoriety"":""Known for his fearsome appearance and ruthless tactics""}
```".AsChirpLog().AsMarkdown());
```

![Log Markdown](Images/log-markdown-code.jpg)

# Logging Objects
You can convert any instanced object to `ChirpLog` and the logger will serialise it as json and display it in a code block.

```csharp
PirateData blackbeard = new PirateData()
{
    Name = "Edward Teach",
    Age = 38,
    Ship = "Queen Anne's Revenge"
};

Chirp.Logger.Log(blackbeard.AsChirpLog());
```
![Log Markdown](Images/log-object.jpg)


## Conditional Compilation
All chirp logging API methods are compiled conditionally and controlled through Player Script Define Symbols.
If Chirp is disabled for given platform any calls to the API will be automatically skipped when compiling - this allows for easy changing of log levels and compiled features without the need to remove your own code.

You can easily adjust the settings through the Chirp Logging Framework Project settings (`Tools/White Sparrow/Chirp Logger/Chirp Settings`).
![Log Level Configuration](Images/package-settings.jpg)

## Performance
Since 0.13.0 the logging hot path is allocation-free on the library side: `ChirpLog` instances are pooled (safe to log from any thread), channel prefixes are cached per logger, and the markdown/stack-trace formatters reuse thread-local builders.

Two things follow from the pooling:
- A `ChirpLog` is only valid until `Submit` returns. Custom `IChirpOutput` implementations must not retain the instance beyond `Ingest()` — call `log.Copy()` to keep a caller-owned snapshot (e.g. for file or network outputs that batch). In the editor, accessing a released log raises an error pointing this out.
- Manually constructed logs that are never submitted are simply collected by the GC — nothing breaks, the pool just loses a reuse.

### Stack traces and Unity's internal log handler
Chirp formats its own stack traces — filtered by `[HideInCallstack]`, with Chirp's own frames removed, and resolved correctly across `async`/UniTask continuations, where Unity's native capture tends to point at the continuation rather than the call site. They are appended to the message in Unity's own `Type:Method () (at path/File.cs:12)` format, so double-click-to-source in the Console still works.

For that to be the only trace shown, Unity's native one has to be suppressed. Chirp does this **per log call**, by routing console output through Unity's internal `DebugLogHandler.Internal_Log` (via a cached reflection delegate) and passing `LogOption.NoStacktrace`. Chirp does **not** call `Application.SetStackTraceLogType` — your project's Stack Trace Logging settings are left exactly as you configured them.

This also skips the `string.Format` full-message copy that Unity's public `ILogHandler.LogFormat` API performs on every log.

If the internal method is missing or its signature changes in a future Unity version, Chirp falls back to the public API silently. That API has no per-call option, so on the fallback path Unity supplies the stack trace according to your project settings and Chirp omits its own rather than printing two.

On IL2CPP with high stripping levels, keep the internal handler with a `link.xml` entry:
```xml
<linker>
  <assembly fullname="UnityEngine.CoreModule">
    <type fullname="UnityEngine.DebugLogHandler" preserve="all"/>
  </assembly>
</linker>
```

## Contributing
If you find any issues or think there are missing features that could benefit the package please use [GitHub Issues](/issues) to report any bugs or ideas.

<p align="right">
  <a href="https://www.twitch.tv/sparrowgamedev">
    <img alt="Twitch Status" src="https://img.shields.io/twitch/status/SparrowGameDev?style=social">
  </a>
  <a href="https://twitter.com/jakubslaby">
    <img alt="Twitter Follow" src="https://img.shields.io/twitter/follow/JakubSlaby?style=social">
  </a>
</p>
