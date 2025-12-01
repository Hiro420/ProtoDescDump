using google.protobuf;

namespace ProtoDescDumper.Core.Abstractions;

public interface IProtoDescriptorFormatter
{
	string FormatFile(FileDescriptorProto proto);
}
