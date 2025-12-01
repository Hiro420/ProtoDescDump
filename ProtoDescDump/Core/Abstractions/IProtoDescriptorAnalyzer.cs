using google.protobuf;

namespace ProtoDescDumper.Core.Abstractions;

public interface IProtoDescriptorAnalyzer
{
	bool Analyze(IReadOnlyList<FileDescriptorProto> protobufs);
}
