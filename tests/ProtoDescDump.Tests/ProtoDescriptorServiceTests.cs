using google.protobuf;
using ProtoDescDump.Core;
using Xunit;

namespace ProtoDescDump.Tests;

public sealed class ProtoDescriptorServiceTests
{
	[Fact]
	public void Analyze_ReturnsFalseAndLogsMissingDependency()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "consumer.proto",
			package = "example.consumer",
			syntax = "proto3"
		};
		descriptor.dependency.Add("missing.proto");

		var logger = new TestLogger();
		var service = new ProtoDescriptorService([descriptor], logger);

		var result = service.Analyze();

		Assert.False(result);
		Assert.Contains(logger.Warnings, message => message.Contains("Unknown dependency: missing.proto"));
		Assert.Contains(logger.Errors, message => message.Contains("Dependency not found: missing.proto"));
	}

	[Fact]
	public void Analyze_ReturnsFalseForAnUnresolvedNamedType()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "broken.proto",
			package = "example.broken",
			syntax = "proto3"
		};

		var message = new DescriptorProto { name = "Wrapper" };
		message.field.Add(new FieldDescriptorProto
		{
			name = "missing",
			number = 1,
			label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
			type = FieldDescriptorProto.Type.TYPE_MESSAGE,
			type_name = ".example.broken.DoesNotExist"
		});
		descriptor.message_type.Add(message);

		var logger = new TestLogger();
		var service = new ProtoDescriptorService([descriptor], logger);

		var result = service.Analyze();

		Assert.False(result);
		Assert.Contains(logger.Errors, message => message.Contains("Type not found: .example.broken.DoesNotExist"));
	}

	[Fact]
	public void Analyze_UsesDescriptorsProvidedToTheConstructor()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "valid.proto",
			package = "example.valid",
			syntax = "proto3"
		};
		descriptor.message_type.Add(new DescriptorProto { name = "Payload" });

		var service = new ProtoDescriptorService([descriptor], new TestLogger());

		Assert.True(service.Analyze());

		var dumpedFiles = new List<(string Name, string Schema)>();
		service.DumpFiles((file, schema) => dumpedFiles.Add((file.name, schema)));

		var dumped = Assert.Single(dumpedFiles);
		Assert.Equal("valid.proto", dumped.Name);
		Assert.Contains("message Payload", dumped.Schema);
	}

	[Fact]
	public void Analyze_ExplicitInputReplacesTheCurrentDescriptorSet()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "consumer.proto",
			package = "example.consumer",
			syntax = "proto3"
		};
		descriptor.dependency.Add("missing.proto");

		var logger = new TestLogger();
		var service = new ProtoDescriptorService([], logger);

		Assert.False(service.Analyze([descriptor]));
		Assert.Contains(logger.Errors, message => message.Contains("Dependency not found: missing.proto"));
	}

	[Fact]
	public void Analyze_AllowsAnOmittedWellKnownGoogleDependency()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "uses-options.proto",
			package = "example.options",
			syntax = "proto3"
		};
		descriptor.dependency.Add("google/protobuf/descriptor.proto");

		var service = new ProtoDescriptorService([descriptor], new TestLogger());

		Assert.True(service.Analyze());
	}

	[Fact]
	public void Analyze_AssumeDependenciesExist_AllowsMissingDependencyAndExternalType()
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

		var logger = new TestLogger();
		var service = new ProtoDescriptorService(
			[descriptor],
			logger,
			assumeDependenciesExist: true);

		Assert.True(service.Analyze());

		var schema = service.FormatFile(descriptor);
		Assert.Contains("import \"missing.proto\";", schema);
		Assert.Contains(".external.Payload payload = 1;", schema);
		Assert.Contains(
			logger.Warnings,
			message => message.Contains("Assuming dependency exists: missing.proto"));
	}

	[Fact]
	public void Analyze_AssumeDependenciesExist_StillRejectsAnUnresolvedLocalType()
	{
		var descriptor = new FileDescriptorProto
		{
			name = "broken.proto",
			package = "example.broken",
			syntax = "proto3"
		};

		var message = new DescriptorProto { name = "Wrapper" };
		message.field.Add(new FieldDescriptorProto
		{
			name = "missing",
			number = 1,
			label = FieldDescriptorProto.Label.LABEL_OPTIONAL,
			type = FieldDescriptorProto.Type.TYPE_MESSAGE,
			type_name = ".example.broken.DoesNotExist"
		});
		descriptor.message_type.Add(message);

		var service = new ProtoDescriptorService(
			[descriptor],
			new TestLogger(),
			assumeDependenciesExist: true);

		Assert.False(service.Analyze());
	}

}
