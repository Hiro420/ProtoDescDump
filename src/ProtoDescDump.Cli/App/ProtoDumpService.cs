using google.protobuf;
using ProtoBuf;
using ProtoDescDump.Core.Abstractions;

namespace ProtoDescDump.Cli.App;

public sealed class ProtoDumpService(
	IFileSystem fileSystem,
	ILogger logger,
	IProtoDescriptorAnalyzer analyzer,
	IProtoDescriptorFormatter formatter) : IProtoDumpService
{
	private readonly IFileSystem fileSystem = fileSystem;
	private readonly ILogger logger = logger;
	private readonly IProtoDescriptorAnalyzer analyzer = analyzer;
	private readonly IProtoDescriptorFormatter formatter = formatter;

	public int Run(string pbPath, string outputDir)
	{
		return Run([pbPath], outputDir, asSet: true);
	}

	public int Run(IEnumerable<string> pbPaths, string outputDir, bool asSet, IEnumerable<string>? dependencyPaths = null)
	{
		try
		{
			var descriptors = new Dictionary<string, FileDescriptorProto>(StringComparer.Ordinal);

			foreach (var pbPath in pbPaths)
			{
                foreach (var descriptor in LoadDescriptors(pbPath, asSet))
                    AddDescriptor(descriptors, descriptor, pbPath);
            }
            if (dependencyPaths != null)
                ResolveDependencies(descriptors, dependencyPaths, asSet);

            var allDescriptors = descriptors.Values.ToArray();
			if (!analyzer.Analyze(allDescriptors))
			{
				logger.Error("Dump failed. Not all dependencies and types were found.");
				return -1;
			}

			logger.Info("Analysis succeeded. Dumping proto files...");

			foreach (var buffer in allDescriptors)
				DumpDescriptor(buffer, outputDir);

			logger.Info("Dump completed successfully.");
			return 0;
		}
		catch (Exception ex)
		{
			logger.Error("[FATAL] Failed", ex);
			return -1;
		}
	}

    private IReadOnlyList<FileDescriptorProto> LoadDescriptors(string pbPath, bool asSet)
    {
        logger.Info($"Loading {(asSet ? "FileDescriptorSet" : "FileDescriptorProto")} from \"{pbPath}\"...");
        using var stream = fileSystem.OpenRead(pbPath);
        return asSet
            ? Serializer.Deserialize<FileDescriptorSet>(stream).file
            : [Serializer.Deserialize<FileDescriptorProto>(stream)];
    }

    private void ResolveDependencies(Dictionary<string, FileDescriptorProto> descriptors, IEnumerable<string> dependencyPaths, bool asSet)
    {
        var pending = new Queue<FileDescriptorProto>(descriptors.Values);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        Dictionary<string, FileDescriptorProto>? candidates = null;

        while (pending.TryDequeue(out var descriptor))
        {
            if (!visited.Add(descriptor.name))
                continue;

            foreach (var dependency in descriptor.dependency.Concat(descriptor.option_dependency))
            {
                if (descriptors.ContainsKey(dependency))
                    continue;

                candidates ??= IndexDependencies(dependencyPaths, asSet);

                if (!candidates.TryGetValue(dependency, out var resolved))
                    continue;

                descriptors.Add(dependency, resolved);
                pending.Enqueue(resolved);
                logger.Info($"Resolved dependency \"{dependency}\" for \"{descriptor.name}\".");
            }
        }
    }

    private Dictionary<string, FileDescriptorProto> IndexDependencies(IEnumerable<string> dependencyPaths, bool asSet)
    {
        var candidates = new Dictionary<string, FileDescriptorProto>(StringComparer.Ordinal);

        foreach (var path in dependencyPaths)
        {
            foreach (var descriptor in LoadDescriptors(path, asSet))
                AddDescriptor(candidates, descriptor, path);
        }

        return candidates;
    }

    private void AddDescriptor(Dictionary<string, FileDescriptorProto> descriptors, FileDescriptorProto descriptor, string sourcePath)
	{
		if (string.IsNullOrEmpty(descriptor.name))
			throw new InvalidDataException($"Descriptor in \"{sourcePath}\" has no file name.");

		if (!descriptors.TryAdd(descriptor.name, descriptor))
			logger.Warn($"Duplicate descriptor \"{descriptor.name}\" in \"{sourcePath}\". Using the first copy.");
	}

	private void DumpDescriptor(FileDescriptorProto buffer, string outputDir)
	{
		var packageParts = (buffer.package ?? string.Empty)
			.Split('.', StringSplitOptions.RemoveEmptyEntries);

		// dirty hack to remove double google.protobuf from the path
		if (packageParts.Length >= 2 && packageParts[0] == "google" && packageParts[1] == "protobuf")
			packageParts = packageParts[2..];

		var outDir = Path.Combine([outputDir, .. packageParts]);
		var outputFile = Path.Combine(outDir, buffer.name);

		fileSystem.EnsureDirectory(Path.GetDirectoryName(outputFile)!);
		var protoText = formatter.FormatFile(buffer);

		logger.Info($"Outputting proto to \"{outputFile}\"");
		fileSystem.WriteAllText(outputFile, protoText);
	}
}
