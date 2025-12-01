using ProtoBuf;

namespace ProtoDescDumper.Core.Services.ProtoDescriptor;

[ProtoContract]
sealed class ExtensionPlaceholder : IExtensible
{
	IExtension? extensionObject;

	IExtension IExtensible.GetExtensionObject(bool createIfMissing)
	{
		return Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
	}
}
