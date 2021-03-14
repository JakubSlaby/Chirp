# Chirp Logging Framework
----
You can find Installation more detailed instructions at the github repository webpage https://github.com/JakubSlaby/Chirp/.

## Installation
Put the Chirp source files in Assets/Plugins.


## Initialisation
To start using Chirp initialise the framework by calling `Chirp.Initialise();` and pass desired loggers.
```csharp
Chirp.Initialise(new UnityConsoleLogger());
```
Add `LogLevelDebug` to ProjectSettings/Player/Script Define Symbols (read more about [Conditional Compilation](#Conditional-Compilation)).

## Usage
Default usage:
```csharp
Chirp.Debug("Debug Message"); // Detailed logs, best for cases like logging rpc responses or method outputs.
Chirp.Log("Log Message"); // Typical log message, most common use case.
Chirp.Info("Info Message"); // State change or any significant message that would have less detailed data.
Chirp.Warning("Warning Message");
Chirp.Error("Error message");
Chirp.Exception(new Exception(), "Exception Message");
```

Read more at [GutHub Chirp Repository](https://github.com/JakubSlaby/Chirp/)
