﻿namespace JobBank1111.Testing.Common;

public sealed class RedirectConsole : IDisposable
{
    private readonly Action<string> logFunction;
    private readonly TextWriter oldOut = Console.Out;
    private readonly StringWriter sw = new StringWriter();

    public RedirectConsole(Action<string> logFunction)
    {
        this.logFunction = logFunction;
        Console.SetOut(this.sw);
    }

    public void Dispose()
    {
        Console.SetOut(this.oldOut);
        this.sw.Flush();
        this.logFunction(this.sw.ToString());
        this.sw.Dispose();
    }
}