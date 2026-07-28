using ProtoDescDump.Core.Abstractions;

namespace ProtoDescDump.Tests;

internal sealed class TestLogger : ILogger
{
	public List<string> Information { get; } = [];
	public List<string> Warnings { get; } = [];
	public List<string> Errors { get; } = [];

	public void Info(string message) => Information.Add(message);

	public void Warn(string message) => Warnings.Add(message);

	public void Error(string message) => Errors.Add(message);

	public void Error(string message, Exception ex) => Errors.Add($"{message}: {ex.Message}");
}
