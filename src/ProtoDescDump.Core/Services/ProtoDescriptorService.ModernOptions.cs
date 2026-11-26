using google.protobuf;
using ProtoBuf;
using System.Globalization;
using System.Text;

namespace ProtoDescDump.Core;

public sealed partial class ProtoDescriptorService
{
    Dictionary<string, string> DumpOptions(FileDescriptorProto source, ExtensionRangeOptions options)
    {
        var optionsKv = new Dictionary<string, string>();
        if (options == null)
            return optionsKv;

        if (options.ShouldSerializeverification())
            optionsKv["verification"] = options.verification.ToString();
        if (options.declaration.Count > 0)
            optionsKv["declaration"] = FormatExtensionDeclarations(options.declaration);

        AddFeatureSetOptions(source, options.features, optionsKv);
        AddUninterpretedOptions(options.uninterpreted_option, optionsKv);
        DumpOptionsMatching(source, ".google.protobuf.ExtensionRangeOptions", options, optionsKv);
        return optionsKv;
    }

    Dictionary<string, string> DumpOptions(FileDescriptorProto source, OneofOptions options)
    {
        var optionsKv = new Dictionary<string, string>();
        if (options == null)
            return optionsKv;

        AddFeatureSetOptions(source, options.features, optionsKv);
        AddUninterpretedOptions(options.uninterpreted_option, optionsKv);
        DumpOptionsMatching(source, ".google.protobuf.OneofOptions", options, optionsKv);
        return optionsKv;
    }

    void AddFeatureSetOptions(
        FileDescriptorProto source,
        FeatureSet? features,
        Dictionary<string, string> optionsKv,
        string prefix = "features")
    {
        if (features == null)
            return;

        if (features.ShouldSerializefield_presence())
            optionsKv[$"{prefix}.field_presence"] = features.field_presence.ToString();
        if (features.ShouldSerializeenum_type())
            optionsKv[$"{prefix}.enum_type"] = features.enum_type.ToString();
        if (features.ShouldSerializerepeated_field_encoding())
            optionsKv[$"{prefix}.repeated_field_encoding"] = features.repeated_field_encoding.ToString();
        if (features.ShouldSerializeutf8_validation())
            optionsKv[$"{prefix}.utf8_validation"] = features.utf8_validation.ToString();
        if (features.ShouldSerializemessage_encoding())
            optionsKv[$"{prefix}.message_encoding"] = features.message_encoding.ToString();
        if (features.ShouldSerializejson_format())
            optionsKv[$"{prefix}.json_format"] = features.json_format.ToString();
        if (features.ShouldSerializeenforce_naming_style())
            optionsKv[$"{prefix}.enforce_naming_style"] = features.enforce_naming_style.ToString();
        if (features.ShouldSerializedefault_symbol_visibility())
            optionsKv[$"{prefix}.default_symbol_visibility"] = features.default_symbol_visibility.ToString();

        DumpOptionsMatching(source, ".google.protobuf.FeatureSet", features, optionsKv, prefix);
    }

    static void AddUninterpretedOptions(
        IEnumerable<UninterpretedOption> options,
        Dictionary<string, string> optionsKv)
    {
        foreach (var option in options)
        {
            var name = string.Join(".", option.name.Select(part =>
                part.is_extension ? $"({part.name_part})" : part.name_part));
            if (string.IsNullOrEmpty(name))
                continue;

            string? value = null;
            if (option.ShouldSerializeidentifier_value())
                value = option.identifier_value;
            else if (option.ShouldSerializepositive_int_value())
                value = option.positive_int_value.ToString(CultureInfo.InvariantCulture);
            else if (option.ShouldSerializenegative_int_value())
                value = option.negative_int_value.ToString(CultureInfo.InvariantCulture);
            else if (option.ShouldSerializedouble_value())
                value = FormatDouble(option.double_value);
            else if (option.ShouldSerializestring_value())
                value = FormatBytesLiteral(option.string_value);
            else if (option.ShouldSerializeaggregate_value())
                value = option.aggregate_value;

            if (value == null)
                continue;

            if (optionsKv.TryGetValue(name, out var existing))
                optionsKv[name] = $"[{existing}, {value}]";
            else
                optionsKv[name] = value;
        }
    }

    static string FormatExtensionDeclarations(IReadOnlyList<ExtensionRangeOptions.Declaration> declarations)
    {
        var values = declarations.Select(item =>
        {
            var fields = new List<string>();
            if (item.ShouldSerializenumber())
                fields.Add($"number: {item.number}");
            if (item.ShouldSerializefull_name())
                fields.Add($"full_name: {Util.ToLiteral(item.full_name)}");
            if (item.ShouldSerializetype())
                fields.Add($"type: {Util.ToLiteral(item.type)}");
            if (item.ShouldSerializereserved())
                fields.Add($"reserved: {(item.reserved ? "true" : "false")}");
            if (item.ShouldSerializerepeated())
                fields.Add($"repeated: {(item.repeated ? "true" : "false")}");
            return $"{{ {string.Join(" ", fields)} }}";
        });

        return $"[{string.Join(", ", values)}]";
    }

    static string FormatEditionDirective(Edition edition)
    {
        var name = edition.ToString();
        if (name.StartsWith("EDITION_", StringComparison.Ordinal))
            name = name[8..];
        return name;
    }

    static string FormatFeatureSupport(FieldOptions.FeatureSupport support)
    {
        var fields = new List<string>();
        if (support.ShouldSerializeedition_introduced())
            fields.Add($"edition_introduced: {support.edition_introduced}");
        if (support.ShouldSerializeedition_deprecated())
            fields.Add($"edition_deprecated: {support.edition_deprecated}");
        if (support.ShouldSerializedeprecation_warning())
            fields.Add($"deprecation_warning: {Util.ToLiteral(support.deprecation_warning)}");
        if (support.ShouldSerializeedition_removed())
            fields.Add($"edition_removed: {support.edition_removed}");
        return $"{{ {string.Join(" ", fields)} }}";
    }

    static string FormatEditionDefaults(IReadOnlyList<FieldOptions.EditionDefault> defaults)
    {
        var values = defaults.Select(item =>
        {
            var fields = new List<string>();
            if (item.ShouldSerializeedition())
                fields.Add($"edition: {item.edition}");
            if (item.ShouldSerializevalue())
                fields.Add($"value: {Util.ToLiteral(item.value)}");
            return $"{{ {string.Join(" ", fields)} }}";
        });

        return $"[{string.Join(", ", values)}]";
    }

    List<string> GetUnknownOptionComments(
        FileDescriptorProto source,
        string optionTypeName,
        IExtensible? options)
    {
        if (options == null)
            return [];

        var declaredNumbers = GetDeclaredOptionNumbers(source, optionTypeName);
        var extension = options.GetExtensionObject(false);
        var comments = new List<string>();

        if (extension != null)
        {
            var stream = extension.BeginQuery();
            try
            {
                while (TryReadWireVarint(stream, out var tag))
                {
                    if (tag == 0)
                        break;

                    var fieldNumber = (int)(tag >> 3);
                    var wireType = (int)(tag & 7);
                    if (fieldNumber <= 0)
                        throw new InvalidDataException($"Invalid protobuf option tag 0x{tag:X}.");

                    if (declaredNumbers.Contains(fieldNumber))
                    {
                        SkipWireValue(stream, wireType);
                        continue;
                    }

                    comments.Add(
                        $"unknown {optionTypeName[(optionTypeName.LastIndexOf('.') + 1)..]} extension #{fieldNumber} " +
                        $"(wire {wireType}) = {ReadUnknownWireValue(stream, wireType)}");
                }
            }
            finally
            {
                extension.EndQuery(stream);
            }
        }

        var features = options switch
        {
            google.protobuf.FileOptions value => value.features,
            ExtensionRangeOptions value => value.features,
            MessageOptions value => value.features,
            FieldOptions value => value.features,
            OneofOptions value => value.features,
            EnumOptions value => value.features,
            EnumValueOptions value => value.features,
            ServiceOptions value => value.features,
            MethodOptions value => value.features,
            _ => null,
        };

        if (features != null)
        {
            foreach (var comment in GetUnknownOptionComments(source, ".google.protobuf.FeatureSet", features))
                comments.Add($"features: {comment}");
        }

        return comments;
    }

    HashSet<FileDescriptorProto> GetOptionVisibleFiles(FileDescriptorProto source)
    {
        var result = new HashSet<FileDescriptorProto>();
        if (protobufMap.TryGetValue(source.name, out var sourceNode))
            result.UnionWith(sourceNode.AllPublicDependencies);

        result.Add(source);

        foreach (var optionDependency in source.option_dependency)
        {
            if (protobufMap.TryGetValue(optionDependency, out var dependencyNode) && dependencyNode.Proto != null)
                result.Add(dependencyNode.Proto);
        }

        return result;
    }

    HashSet<int> GetDeclaredOptionNumbers(FileDescriptorProto source, string typeName)
    {
        var result = new HashSet<int>();
        var dependencies = GetOptionVisibleFiles(source);

        foreach (var type in protobufTypeMap.Values)
        {
            if (type.Proto != null &&
                dependencies.Contains(type.Proto) &&
                type.Source is FieldDescriptorProto field &&
                field.extendee == typeName)
            {
                result.Add(field.number);
            }
        }

        return result;
    }

    static string ReadUnknownWireValue(Stream stream, int wireType)
    {
        switch (wireType)
        {
            case 0:
                if (!TryReadWireVarint(stream, out var varint))
                    throw new EndOfStreamException("Truncated unknown option varint.");
                return varint.ToString(CultureInfo.InvariantCulture);

            case 1:
                return $"0x{ReadFixed64(stream):X16}";

            case 2:
            {
                var payload = ReadLengthDelimited(stream, "unknown custom option");
                if (TryFormatReadableUtf8(payload, out var text))
                    return $"{text} (hex {Convert.ToHexString(payload)})";
                return $"hex {Convert.ToHexString(payload)}";
            }

            case 3:
                SkipWireValue(stream, wireType);
                return "<group>";

            case 5:
                return $"0x{ReadFixed32(stream):X8}";

            default:
                throw new InvalidDataException($"Unsupported protobuf wire type {wireType}.");
        }
    }

    static bool TryFormatReadableUtf8(byte[] payload, out string value)
    {
        try
        {
            var text = new UTF8Encoding(false, true).GetString(payload);
            if (text.All(ch => !char.IsControl(ch) || ch is '\r' or '\n' or '\t'))
            {
                value = Util.ToLiteral(text);
                return true;
            }
        }
        catch (DecoderFallbackException)
        {
        }

        value = string.Empty;
        return false;
    }
}
