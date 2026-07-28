namespace ProtoDescDump.Cli.App;

public interface IProtoDumpService
{
	int Run(string pbPath, string outputDir);
}
