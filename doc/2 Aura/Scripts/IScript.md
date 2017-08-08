```csharp
public interface IScript
{
    bool Init();
}
```

Every class in any loaded script file that implements IScript will be instantiated on server start, with its `Init` method getting called afterwards. If `Init` returns `false`, the script manager will log a Debug message, saying that instantiation for that class failed.

```text
[Debug] - LoadScriptAssembly: Failed to initiate 'MyTestScript'.
```

The script manager will also remember all instantiated classes that implement `IDisposable`, `Dispose` being called if the scripts are reloaded.