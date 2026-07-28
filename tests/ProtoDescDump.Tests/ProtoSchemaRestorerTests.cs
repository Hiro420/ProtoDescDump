using google.protobuf;
using ProtoDescDump.Core;
using Xunit;

namespace ProtoDescDump.Tests;

public sealed class ProtoSchemaRestorerTests
{
	[Fact]
	public void Restore_ReconstructsMessagesOptionsAndStreamingRpc()
	{
		var descriptor = CreateEchoDescriptor();
		var restorer = new ProtoSchemaRestorer();

		var schema = restorer.Restore(descriptor);

		Assert.Contains("syntax = \"proto3\";", schema);
		Assert.Contains("package example.echo;", schema);
		Assert.Contains("option csharp_namespace = \"Example.Echo\";", schema);
		Assert.Contains("message EchoRequest", schema);
		Assert.Contains("string text = 1;", schema);
		Assert.Contains("service EchoService", schema);
		Assert.Contains(
			"rpc Chat (stream .example.echo.EchoRequest) returns (stream .example.echo.EchoReply);",
			schema);
	}

	[Fact]
	public void Restore_SetReturnsEveryFileByDescriptorName()
	{
		var common = new FileDescriptorProto
		{
			name = "common.proto",
			package = "example.common",
			syntax = "proto3"
		};
		common.message_type.Add(new DescriptorProto { name = "Shared" });

		var consumer = new FileDescriptorProto
		{
			name = "consumer.proto",
			package = "example.consumer",
			syntax = "proto3"
		};
		consumer.dependency.Add("common.proto");

		var wrapper = new DescriptorProto { name = "Wrapper" };
		wrapper.field.Add(new FieldDescriptorProto
		{
			name = "value",
			number = 1,
			label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
			type = FieldDescriptorProto.Type.TYPE_MESSAGE,
			type_name = ".example.common.Shared"
		});
		consumer.message_type.Add(wrapper);

		var set = new FileDescriptorSet();
		set.file.Add(common);
		set.file.Add(consumer);

		var restored = new ProtoSchemaRestorer().Restore(set);

		Assert.Equal(2, restored.Count);
		Assert.Contains("message Shared", restored["common.proto"]);
		Assert.Contains("import \"common.proto\";", restored["consumer.proto"]);
		Assert.Contains(".example.common.Shared value = 1;", restored["consumer.proto"]);
	}

	[Fact]
	public void Restore_ThrowsWhenDescriptorNamesAreDuplicated()
	{
		var first = new FileDescriptorProto { name = "duplicate.proto", syntax = "proto3" };
		var second = new FileDescriptorProto { name = "duplicate.proto", syntax = "proto3" };
		var restorer = new ProtoSchemaRestorer();

		var exception = Assert.Throws<ArgumentException>(() => restorer.Restore([first, second]));

		Assert.Contains("more than one file", exception.Message);
	}

	[Fact]
	public void Restore_AssumeDependenciesExist_PreservesExternalTypeReferences()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "consumer.proto",
			package = "example.consumer",
			syntax = "proto3"
		};
		descriptor.dependency.Add("missing.proto");

		var message = new DescriptorProto { name = "Wrapper" };
		message.field.Add(new FieldDescriptorProto
		{
			name = "payload",
			number = 1,
			label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
			type = FieldDescriptorProto.Type.TYPE_MESSAGE,
			type_name = ".external.Payload"
		});
		descriptor.message_type.Add(message);

		var restorer = new ProtoSchemaRestorer(assumeDependenciesExist: true);

		var schema = restorer.Restore(descriptor);

		Assert.Contains("import \"missing.proto\";", schema);
		Assert.Contains(".external.Payload payload = 1;", schema);
	}

	private static FileDescriptorProto CreateEchoDescriptor()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "echo.proto",
			package = "example.echo",
			syntax = "proto3",
			options = new google.protobuf.FileOptions { csharp_namespace = "Example.Echo" }
		};

		var request = new DescriptorProto { name = "EchoRequest" };
		request.field.Add(new FieldDescriptorProto
		{
			name = "text",
			number = 1,
			label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
			type = FieldDescriptorProto.Type.TYPE_STRING
		});

		var reply = new DescriptorProto { name = "EchoReply" };
		reply.field.Add(new FieldDescriptorProto
		{
			name = "text",
			number = 1,
			label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
			type = FieldDescriptorProto.Type.TYPE_STRING
		});

		descriptor.message_type.Add(request);
		descriptor.message_type.Add(reply);

		var service = new ServiceDescriptorProto { name = "EchoService" };
		service.method.Add(new MethodDescriptorProto
		{
			name = "Chat",
			input_type = ".example.echo.EchoRequest",
			output_type = ".example.echo.EchoReply",
			client_streaming = true,
			server_streaming = true
		});
		descriptor.service.Add(service);

		return descriptor;
	}
}
