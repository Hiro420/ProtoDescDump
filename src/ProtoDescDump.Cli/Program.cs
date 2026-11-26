using ProtoDescDump.Cli.App;
using ProtoDescDump.Core;
using System.CommandLine;

namespace ProtoDescDump.Cli;

internal static class Program
{
	internal static int Main(string[] args)
	{
		var root = BuildRootCommand();
		return root.Parse(args).Invoke();
	}

	internal static RootCommand BuildRootCommand()
	{
		var root = new RootCommand("Proto descriptor dumper");

		var inputOption = new Option<string>("--input")
		{
			Description = "Input file or directory path",
			Aliases = { "-i" },
			Required = true
		};

		var outputOption = new Option<string>("--output")
		{
			Description = "Output directory for dumped .proto files",
			Aliases = { "-o" },
			DefaultValueFactory = result => "DumpedProtos"
		};

		var recurseOption = new Option<bool>("--recurse")
		{
			Description = "If set with directory input, process all files recursively",
			Aliases = { "-r" },
			DefaultValueFactory = result => false
		};

		var asSetOption = new Option<bool>("--as-set")
		{
			Description = "Treat each input as a FileDescriptorSet (default). If false, treat as a single FileDescriptorProto",
			Aliases = { "-s" },
			DefaultValueFactory = result => true
		};

		var assumeDependenciesExistOption = new Option<bool>("--assume-dependencies-exist")
		{
			Description = "Continue when imported descriptor files are missing",
			Aliases = { "--no-dep" },
			DefaultValueFactory = result => false
		};

		root.Options.Add(inputOption);
		root.Options.Add(outputOption);
		root.Options.Add(recurseOption);
		root.Options.Add(asSetOption);
		root.Options.Add(assumeDependenciesExistOption);

		int Run(ParseResult parseResult)
		{
			var logger = new ConsoleLogger();
			var fileSystem = new LocalFileSystem();
			var assumeDependenciesExist = parseResult.GetValue(assumeDependenciesExistOption);
			var coreService = new ProtoDescriptorService([], logger, assumeDependenciesExist);
			var app = new ProtoDumpService(fileSystem, logger, coreService, coreService);

			var input = parseResult.GetRequiredValue(inputOption)!;
			var output = parseResult.GetRequiredValue(outputOption)!;
			var recurse = parseResult.GetValue(recurseOption);
			var asSet = parseResult.GetValue(asSetOption);
			int exitCode;

			if (Directory.Exists(input))
			{
				var searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				var files = Directory.GetFiles(input, "*.pb", searchOption).OrderBy(path => path, StringComparer.Ordinal);

                exitCode = app.Run(files, output, asSet);
			}
			else
			{
                var inputPath = Path.GetFullPath(input);
                var searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var pathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                var dependencyPaths = Directory.EnumerateFiles(Path.GetDirectoryName(inputPath)!, "*.pb", searchOption)
                                   .Where(path => !pathComparer.Equals(Path.GetFullPath(path), inputPath))
                                   .OrderBy(path => path, StringComparer.Ordinal);
                
                exitCode = app.Run([input], output, asSet, dependencyPaths);
			}

			return exitCode;
		}
		root.SetAction(Run);

		return root;
	}

}
