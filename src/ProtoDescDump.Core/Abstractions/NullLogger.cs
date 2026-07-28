namespace ProtoDescDump.Core.Abstractions;

public sealed class NullLogger : ILogger
{
	public static NullLogger Instance { get; } = new();

	private NullLogger()
	{
	}

	public void Info(string message)
	{
	}

	public void Warn(string message)
	{
	}

	public void Error(string message)
	{
	}

	public void Error(string message, Exception ex)
	{
	}
}
