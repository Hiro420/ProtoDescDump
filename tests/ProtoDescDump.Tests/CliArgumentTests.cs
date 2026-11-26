using google.protobuf;
using ProtoBuf;
using ProtoDescDump.Cli;
using Xunit;

namespace ProtoDescDump.Tests;

public sealed class CliArgumentTests
{
	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(false, false)]
	public void ResolvesImportsByEmbeddedProtoName(bool asSet, bool directoryInput)
	{
		var tempRoot = Path.Combine(Path.GetTempPath(), $"ProtoDescDump.Tests.{Guid.NewGuid():N}");
		var input = Path.Combine(tempRoot, "input");
		var output = Path.Combine(tempRoot, "output");
		var rootPath = Path.Combine(input, "00-root.pb");

		try
		{
			Directory.CreateDirectory(Path.Combine(input, "nested"));

			var root = new FileDescriptorProto { name = "root.proto", package = "example", syntax = "proto3" };
			root.dependency.Add("common/payload.proto");
			var wrapper = new DescriptorProto { name = "Wrapper" };
			wrapper.field.Add(new FieldDescriptorProto
			{
				name = "payload", number = 1,
				label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
				type = FieldDescriptorProto.Type.TYPE_MESSAGE,
				type_name = ".example.Payload"
			});
			root.message_type.Add(wrapper);

			var payload = new FileDescriptorProto { name = "common/payload.proto", package = "example", syntax = "proto3" };
			payload.dependency.Add("leaf.proto");
			payload.message_type.Add(new DescriptorProto { name = "Payload" });
			var leaf = new FileDescriptorProto { name = "leaf.proto", package = "example", syntax = "proto3" };
			leaf.message_type.Add(new DescriptorProto { name = "Leaf" });
			var unrelated = new FileDescriptorProto { name = "unrelated.proto", package = "example", syntax = "proto3" };

			WriteDescriptor(rootPath, root, asSet);
			WriteDescriptor(Path.Combine(input, "nested", "random-name.pb"), payload, asSet);
			WriteDescriptor(Path.Combine(input, "another-name.pb"), leaf, asSet);
			WriteDescriptor(Path.Combine(input, "unrelated.pb"), unrelated, asSet);

			var exitCode = Program.Main([
				"-i", directoryInput ? input : rootPath,
				"-o", output, "--recurse", "--as-set", asSet ? "true" : "false"
			]);

			Assert.Equal(0, exitCode);
			Assert.Contains("import \"common/payload.proto\";", File.ReadAllText(Path.Combine(output, "example", "root.proto")));
			Assert.Contains("message Payload", File.ReadAllText(Path.Combine(output, "example", "common", "payload.proto")));
			Assert.True(File.Exists(Path.Combine(output, "example", "leaf.proto")));
			Assert.Equal(directoryInput, File.Exists(Path.Combine(output, "example", "unrelated.proto")));
		}
		finally
		{
			if (Directory.Exists(tempRoot))
				Directory.Delete(tempRoot, recursive: true);
		}
	}

	private static void WriteDescriptor(string path, FileDescriptorProto descriptor, bool asSet)
	{
		using var stream = File.Create(path);
		if (asSet)
		{
			var set = new FileDescriptorSet();
			set.file.Add(descriptor);
			Serializer.Serialize(stream, set);
		}
		else
		{
			Serializer.Serialize(stream, descriptor);
		}
	}

	[Fact]
	public void AssumeDependenciesExist_AllowsTheCliToWriteOutput()
	{
		var tempRoot = Path.Combine(Path.GetTempPath(), $"ProtoDescDump.Tests.{Guid.NewGuid():N}");
		var inputPath = Path.Combine(tempRoot, "descriptor.pb");
		var outputPath = Path.Combine(tempRoot, "output");

		try
		{
			Directory.CreateDirectory(tempRoot);

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

			using (var stream = File.Create(inputPath))
			{
				Serializer.Serialize(stream, set);
			}

			var exitCode = Program.Main(
				[
					"--input", inputPath,
					"--output", outputPath,
					"--assume-dependencies-exist"
				]);

			Assert.Equal(0, exitCode);

			var generatedPath = Path.Combine(
				outputPath,
				"example",
				"consumer",
				"consumer.proto");

			var schema = File.ReadAllText(generatedPath);
			Assert.Contains("import \"missing.proto\";", schema);
			Assert.Contains("external.Payload payload = 1;", schema);
		}
		finally
		{
			if (Directory.Exists(tempRoot))
			{
				Directory.Delete(tempRoot, recursive: true);
			}
		}
	}
}
