using google.protobuf;
using ProtoDescDump.Core.Abstractions;

namespace ProtoDescDump.Core;

public sealed class ProtoSchemaRestorer
{
	private readonly ILogger logger;
	private readonly bool assumeDependenciesExist;

	public ProtoSchemaRestorer(ILogger? logger = null, bool assumeDependenciesExist = false)
	{
		this.logger = logger ?? NullLogger.Instance;
		this.assumeDependenciesExist = assumeDependenciesExist;
	}

	public IReadOnlyDictionary<string, string> Restore(FileDescriptorSet descriptorSet)
	{
		ArgumentNullException.ThrowIfNull(descriptorSet);
		return Restore(descriptorSet.file);
	}

	public IReadOnlyDictionary<string, string> Restore(IReadOnlyList<FileDescriptorProto> descriptors)
	{
		ArgumentNullException.ThrowIfNull(descriptors);

		var names = new HashSet<string>(StringComparer.Ordinal);
		foreach (var descriptor in descriptors)
		{
			if (descriptor is null)
			{
				throw new ArgumentException("The descriptor collection cannot contain null entries.", nameof(descriptors));
			}

			if (string.IsNullOrWhiteSpace(descriptor.name))
			{
				throw new ArgumentException("Every descriptor must have a non-empty file name.", nameof(descriptors));
			}

			if (!names.Add(descriptor.name))
			{
				throw new ArgumentException(
					$"The descriptor collection contains more than one file named '{descriptor.name}'.",
					nameof(descriptors));
			}
		}

		var service = new ProtoDescriptorService(descriptors, logger, assumeDependenciesExist);
		if (!service.Analyze())
		{
			throw new InvalidOperationException(
				"The descriptor graph could not be analyzed because one or more dependencies or referenced types are missing.");
		}

		var restored = new Dictionary<string, string>(descriptors.Count, StringComparer.Ordinal);
		foreach (var descriptor in descriptors)
		{
			restored.Add(descriptor.name, service.FormatFile(descriptor));
		}

		return restored;
	}

	public string Restore(
		FileDescriptorProto descriptor,
		IEnumerable<FileDescriptorProto>? dependencies = null)
	{
		ArgumentNullException.ThrowIfNull(descriptor);

		var descriptors = new List<FileDescriptorProto> { descriptor };
		if (dependencies is not null)
		{
			descriptors.AddRange(dependencies.Where(candidate => !ReferenceEquals(candidate, descriptor)));
		}

		var restored = Restore(descriptors);
		return restored[descriptor.name];
	}
}
