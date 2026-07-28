using google.protobuf;
using ProtoBuf;
using ProtoDescDump.Cli;
using Xunit;

namespace ProtoDescDump.Tests;

public sealed class CliArgumentTests
{
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
			Assert.Contains(".external.Payload payload = 1;", schema);
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
