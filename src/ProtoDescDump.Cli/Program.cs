using google.protobuf;
using ProtoBuf;
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
				// all files from a path (recursively or not)
				var searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				var files = Directory.GetFiles(input, "*.pb", searchOption);

				exitCode = 0;
				foreach (var file in files)
				{
					int code = asSet
						? app.Run(file, output)
						: RunSingleProto(fileSystem, logger, coreService, file, output);

					if (code != 0)
					{
						exitCode = code;
					}
				}
			}
			else
			{
				// specific file
				exitCode = asSet
					? app.Run(input, output)
					: RunSingleProto(fileSystem, logger, coreService, input, output);
			}

			return exitCode;
		}
		root.SetAction(Run);

		return root;
	}

	static int RunSingleProto(LocalFileSystem fileSystem, ConsoleLogger logger, ProtoDescriptorService coreService, string protoPath, string outputDir)
	{
		try
		{
			logger.Info($"Loading FileDescriptorProto from \"{protoPath}\"...");
			using var stream = fileSystem.OpenRead(protoPath);
			var single = Serializer.Deserialize<FileDescriptorProto>(stream);

			if (!coreService.Analyze([single]))
			{
				logger.Error("Dump failed. Not all dependencies and types were found.");
				return -1;
			}

			var packageParts = (single.package ?? string.Empty)
				.Split('.', StringSplitOptions.RemoveEmptyEntries);

			// dirty hack to remove double google.protobuf from the path
			if (packageParts.Length >= 2 && packageParts[0] == "google" && packageParts[1] == "protobuf")
				packageParts = packageParts[2..];

			var outDir = Path.Combine([outputDir, .. packageParts]);
			var outputFile = Path.Combine(outDir, single.name);

			fileSystem.EnsureDirectory(Path.GetDirectoryName(outputFile)!);
			var protoText = coreService.FormatFile(single);

			logger.Info($"Outputting proto to \"{outputFile}\"");
			fileSystem.WriteAllText(outputFile, protoText);
			logger.Info("Dump completed successfully.");
			return 0;
		}
		catch (Exception ex)
		{
			logger.Error("[FATAL] Failed", ex);
			return -1;
		}
	}
}
