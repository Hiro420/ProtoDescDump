using google.protobuf;
using ProtoDescDump.Core.Abstractions;
using ProtoDescDump.Core.Services.ProtoDescriptor;

namespace ProtoDescDump.Core;

public sealed partial class ProtoDescriptorService : IProtoDescriptorAnalyzer, IProtoDescriptorFormatter
{
	public delegate void ProcessProtobuf(FileDescriptorProto buffer, string proto);

	readonly List<FileDescriptorProto> _protobufs;
	readonly Stack<string> messageNameStack = [];
	readonly Dictionary<string, ProtoNode> protobufMap = [];
	readonly Dictionary<string, ProtoTypeNode> protobufTypeMap = [];
	readonly ILogger _logger;
	readonly bool _assumeDependenciesExist;

	public ProtoDescriptorService(
		IEnumerable<FileDescriptorProto> protobufs,
		ILogger logger,
		bool assumeDependenciesExist = false)
	{
		ArgumentNullException.ThrowIfNull(protobufs);
		ArgumentNullException.ThrowIfNull(logger);

		_protobufs = [.. protobufs];
		_logger = logger;
		_assumeDependenciesExist = assumeDependenciesExist;
	}
}
