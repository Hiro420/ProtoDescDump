using google.protobuf;
using ProtoBuf;
using ProtoDescDump.Cli.App;
using ProtoDescDump.Core;
using Xunit;

namespace ProtoDescDump.Tests;

public sealed class ProtoDumpServiceTests
{
	[Fact]
	public void Run_DeserializesDescriptorSetAndWritesRestoredFiles()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "echo.proto",
			package = "example.echo",
			syntax = "proto3"
		};
		descriptor.message_type.Add(new DescriptorProto { name = "EchoRequest" });

		var set = new FileDescriptorSet();
		set.file.Add(descriptor);

		byte[] input;
		using (var stream = new MemoryStream())
		{
			Serializer.Serialize(stream, set);
			input = stream.ToArray();
		}

		var fileSystem = new MemoryFileSystem(input);
		var logger = new TestLogger();
		var core = new ProtoDescriptorService([], logger);
		var service = new ProtoDumpService(fileSystem, logger, core, core);

		var exitCode = service.Run("descriptor.pb", "output");

		Assert.Equal(0, exitCode);
		var expectedPath = Path.Combine("output", "example", "echo", "echo.proto");
		Assert.True(fileSystem.Writes.TryGetValue(expectedPath, out var schema));
		Assert.Contains("package example.echo;", schema);
		Assert.Contains("message EchoRequest", schema);
	}

	[Fact]
	public void Run_AssumeDependenciesExist_WritesSchemaWithMissingImport()
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

		var set = new FileDescriptorSet();
		set.file.Add(descriptor);

		byte[] input;
		using (var stream = new MemoryStream())
		{
			Serializer.Serialize(stream, set);
			input = stream.ToArray();
		}

		var fileSystem = new MemoryFileSystem(input);
		var logger = new TestLogger();
		var core = new ProtoDescriptorService([], logger, assumeDependenciesExist: true);
		var service = new ProtoDumpService(fileSystem, logger, core, core);

		var exitCode = service.Run("descriptor.pb", "output");

		Assert.Equal(0, exitCode);
		var expectedPath = Path.Combine("output", "example", "consumer", "consumer.proto");
		Assert.True(fileSystem.Writes.TryGetValue(expectedPath, out var schema));
		Assert.Contains("import \"missing.proto\";", schema);
		Assert.Contains(".external.Payload payload = 1;", schema);
	}

	private sealed class MemoryFileSystem(byte[] input) : IFileSystem
	{
		public Dictionary<string, string> Writes { get; } = new(StringComparer.Ordinal);

		public Stream OpenRead(string path) => new MemoryStream(input, writable: false);

		public void WriteAllText(string path, string contents) => Writes[path] = contents;

		public void EnsureDirectory(string path)
		{
		}
	}
}
