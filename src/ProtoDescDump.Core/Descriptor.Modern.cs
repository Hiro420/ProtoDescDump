using ProtoBuf;

namespace google.protobuf;

public enum Edition
{
    EDITION_UNKNOWN = 0,
    EDITION_LEGACY = 900,
    EDITION_PROTO2 = 998,
    EDITION_PROTO3 = 999,
    EDITION_2023 = 1000,
    EDITION_2024 = 1001,
    EDITION_UNSTABLE = 9999,
    EDITION_1_TEST_ONLY = 1,
    EDITION_2_TEST_ONLY = 2,
    EDITION_99997_TEST_ONLY = 99997,
    EDITION_99998_TEST_ONLY = 99998,
    EDITION_99999_TEST_ONLY = 99999,
    EDITION_MAX = int.MaxValue,
}

public partial class FileDescriptorProto
{
    [ProtoMember(14)]
    public Edition edition
    {
        get => __modern_edition ?? Edition.EDITION_UNKNOWN;
        set => __modern_edition = value;
    }
    public bool ShouldSerializeedition() => __modern_edition != null;
    public void Resetedition() => __modern_edition = null;
    private Edition? __modern_edition;

    [ProtoMember(15)]
    public List<string> option_dependency { get; } = [];
}

[ProtoContract]
public partial class FeatureSet : IExtensible
{
    private IExtension? __modern_extensionData;
    IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        => Extensible.GetExtensionObject(ref __modern_extensionData, createIfMissing);

    [ProtoMember(1)]
    public FieldPresence field_presence
    {
        get => __field_presence ?? FieldPresence.FIELD_PRESENCE_UNKNOWN;
        set => __field_presence = value;
    }
    public bool ShouldSerializefield_presence() => __field_presence != null;
    private FieldPresence? __field_presence;

    [ProtoMember(2)]
    public EnumType enum_type
    {
        get => __enum_type ?? EnumType.ENUM_TYPE_UNKNOWN;
        set => __enum_type = value;
    }
    public bool ShouldSerializeenum_type() => __enum_type != null;
    private EnumType? __enum_type;

    [ProtoMember(3)]
    public RepeatedFieldEncoding repeated_field_encoding
    {
        get => __repeated_field_encoding ?? RepeatedFieldEncoding.REPEATED_FIELD_ENCODING_UNKNOWN;
        set => __repeated_field_encoding = value;
    }
    public bool ShouldSerializerepeated_field_encoding() => __repeated_field_encoding != null;
    private RepeatedFieldEncoding? __repeated_field_encoding;

    [ProtoMember(4)]
    public Utf8Validation utf8_validation
    {
        get => __utf8_validation ?? Utf8Validation.UTF8_VALIDATION_UNKNOWN;
        set => __utf8_validation = value;
    }
    public bool ShouldSerializeutf8_validation() => __utf8_validation != null;
    private Utf8Validation? __utf8_validation;

    [ProtoMember(5)]
    public MessageEncoding message_encoding
    {
        get => __message_encoding ?? MessageEncoding.MESSAGE_ENCODING_UNKNOWN;
        set => __message_encoding = value;
    }
    public bool ShouldSerializemessage_encoding() => __message_encoding != null;
    private MessageEncoding? __message_encoding;

    [ProtoMember(6)]
    public JsonFormat json_format
    {
        get => __json_format ?? JsonFormat.JSON_FORMAT_UNKNOWN;
        set => __json_format = value;
    }
    public bool ShouldSerializejson_format() => __json_format != null;
    private JsonFormat? __json_format;

    [ProtoMember(7)]
    public EnforceNamingStyle enforce_naming_style
    {
        get => __enforce_naming_style ?? EnforceNamingStyle.ENFORCE_NAMING_STYLE_UNKNOWN;
        set => __enforce_naming_style = value;
    }
    public bool ShouldSerializeenforce_naming_style() => __enforce_naming_style != null;
    private EnforceNamingStyle? __enforce_naming_style;

    [ProtoMember(8)]
    public VisibilityFeature.DefaultSymbolVisibility default_symbol_visibility
    {
        get => __default_symbol_visibility ?? VisibilityFeature.DefaultSymbolVisibility.DEFAULT_SYMBOL_VISIBILITY_UNKNOWN;
        set => __default_symbol_visibility = value;
    }
    public bool ShouldSerializedefault_symbol_visibility() => __default_symbol_visibility != null;
    private VisibilityFeature.DefaultSymbolVisibility? __default_symbol_visibility;

    public enum FieldPresence
    {
        FIELD_PRESENCE_UNKNOWN = 0,
        EXPLICIT = 1,
        IMPLICIT = 2,
        LEGACY_REQUIRED = 3,
    }

    public enum EnumType
    {
        ENUM_TYPE_UNKNOWN = 0,
        OPEN = 1,
        CLOSED = 2,
    }

    public enum RepeatedFieldEncoding
    {
        REPEATED_FIELD_ENCODING_UNKNOWN = 0,
        PACKED = 1,
        EXPANDED = 2,
    }

    public enum Utf8Validation
    {
        UTF8_VALIDATION_UNKNOWN = 0,
        VERIFY = 2,
        NONE = 3,
    }

    public enum MessageEncoding
    {
        MESSAGE_ENCODING_UNKNOWN = 0,
        LENGTH_PREFIXED = 1,
        DELIMITED = 2,
    }

    public enum JsonFormat
    {
        JSON_FORMAT_UNKNOWN = 0,
        ALLOW = 1,
        LEGACY_BEST_EFFORT = 2,
    }

    public enum EnforceNamingStyle
    {
        ENFORCE_NAMING_STYLE_UNKNOWN = 0,
        STYLE2024 = 1,
        STYLE_LEGACY = 2,
    }

    public static class VisibilityFeature
    {
        public enum DefaultSymbolVisibility
        {
            DEFAULT_SYMBOL_VISIBILITY_UNKNOWN = 0,
            EXPORT_ALL = 1,
            EXPORT_TOP_LEVEL = 2,
            LOCAL_ALL = 3,
            STRICT = 4,
        }
    }
}


public partial class ExtensionRangeOptions
{
    [ProtoMember(2)]
    public List<Declaration> declaration { get; } = [];

    [ProtoMember(50)]
    public FeatureSet? features { get; set; }

    [ProtoMember(3)]
    public VerificationState verification
    {
        get => __modern_verification ?? VerificationState.DECLARATION;
        set => __modern_verification = value;
    }
    public bool ShouldSerializeverification() => __modern_verification != null;
    private VerificationState? __modern_verification;

    public enum VerificationState
    {
        DECLARATION = 0,
        UNVERIFIED = 1,
    }

    [ProtoContract]
    public sealed class Declaration
    {
        [ProtoMember(1)]
        public int number
        {
            get => __number.GetValueOrDefault();
            set => __number = value;
        }
        public bool ShouldSerializenumber() => __number != null;
        private int? __number;

        [ProtoMember(2)]
        public string full_name
        {
            get => __full_name ?? string.Empty;
            set => __full_name = value;
        }
        public bool ShouldSerializefull_name() => __full_name != null;
        private string? __full_name;

        [ProtoMember(3)]
        public string type
        {
            get => __type ?? string.Empty;
            set => __type = value;
        }
        public bool ShouldSerializetype() => __type != null;
        private string? __type;

        [ProtoMember(5)]
        public bool reserved
        {
            get => __reserved.GetValueOrDefault();
            set => __reserved = value;
        }
        public bool ShouldSerializereserved() => __reserved != null;
        private bool? __reserved;

        [ProtoMember(6)]
        public bool repeated
        {
            get => __repeated.GetValueOrDefault();
            set => __repeated = value;
        }
        public bool ShouldSerializerepeated() => __repeated != null;
        private bool? __repeated;
    }
}

public partial class FileOptions
{
    [ProtoMember(50)]
    public FeatureSet? features { get; set; }
}

public partial class MessageOptions
{
    [ProtoMember(11)]
    public bool deprecated_legacy_json_field_conflicts
    {
        get => __modern_deprecated_legacy_json_field_conflicts.GetValueOrDefault();
        set => __modern_deprecated_legacy_json_field_conflicts = value;
    }
    public bool ShouldSerializedeprecated_legacy_json_field_conflicts() => __modern_deprecated_legacy_json_field_conflicts != null;
    private bool? __modern_deprecated_legacy_json_field_conflicts;

    [ProtoMember(12)]
    public FeatureSet? features { get; set; }
}

public partial class FieldOptions
{
    [ProtoMember(15)]
    public bool unverified_lazy
    {
        get => __modern_unverified_lazy.GetValueOrDefault();
        set => __modern_unverified_lazy = value;
    }
    public bool ShouldSerializeunverified_lazy() => __modern_unverified_lazy != null;
    private bool? __modern_unverified_lazy;

    [ProtoMember(16)]
    public bool debug_redact
    {
        get => __modern_debug_redact.GetValueOrDefault();
        set => __modern_debug_redact = value;
    }
    public bool ShouldSerializedebug_redact() => __modern_debug_redact != null;
    private bool? __modern_debug_redact;

    [ProtoMember(17)]
    public OptionRetention retention
    {
        get => __modern_retention ?? OptionRetention.RETENTION_UNKNOWN;
        set => __modern_retention = value;
    }
    public bool ShouldSerializeretention() => __modern_retention != null;
    private OptionRetention? __modern_retention;

    [ProtoMember(19)]
    public List<OptionTargetType> targets { get; } = [];

    [ProtoMember(20)]
    public List<EditionDefault> edition_defaults { get; } = [];

    [ProtoMember(21)]
    public FeatureSet? features { get; set; }

    [ProtoMember(22)]
    public FeatureSupport? feature_support { get; set; }

    public enum OptionRetention
    {
        RETENTION_UNKNOWN = 0,
        RETENTION_RUNTIME = 1,
        RETENTION_SOURCE = 2,
    }

    public enum OptionTargetType
    {
        TARGET_TYPE_UNKNOWN = 0,
        TARGET_TYPE_FILE = 1,
        TARGET_TYPE_EXTENSION_RANGE = 2,
        TARGET_TYPE_MESSAGE = 3,
        TARGET_TYPE_FIELD = 4,
        TARGET_TYPE_ONEOF = 5,
        TARGET_TYPE_ENUM = 6,
        TARGET_TYPE_ENUM_ENTRY = 7,
        TARGET_TYPE_SERVICE = 8,
        TARGET_TYPE_METHOD = 9,
    }

    [ProtoContract]
    public sealed class EditionDefault
    {
        [ProtoMember(3)]
        public Edition edition
        {
            get => __edition ?? Edition.EDITION_UNKNOWN;
            set => __edition = value;
        }
        public bool ShouldSerializeedition() => __edition != null;
        private Edition? __edition;

        [ProtoMember(2)]
        public string value
        {
            get => __value ?? string.Empty;
            set => __value = value;
        }
        public bool ShouldSerializevalue() => __value != null;
        private string? __value;
    }

    [ProtoContract]
    public sealed class FeatureSupport
    {
        [ProtoMember(1)]
        public Edition edition_introduced
        {
            get => __edition_introduced ?? Edition.EDITION_UNKNOWN;
            set => __edition_introduced = value;
        }
        public bool ShouldSerializeedition_introduced() => __edition_introduced != null;
        private Edition? __edition_introduced;

        [ProtoMember(2)]
        public Edition edition_deprecated
        {
            get => __edition_deprecated ?? Edition.EDITION_UNKNOWN;
            set => __edition_deprecated = value;
        }
        public bool ShouldSerializeedition_deprecated() => __edition_deprecated != null;
        private Edition? __edition_deprecated;

        [ProtoMember(3)]
        public string deprecation_warning
        {
            get => __deprecation_warning ?? string.Empty;
            set => __deprecation_warning = value;
        }
        public bool ShouldSerializedeprecation_warning() => __deprecation_warning != null;
        private string? __deprecation_warning;

        [ProtoMember(4)]
        public Edition edition_removed
        {
            get => __edition_removed ?? Edition.EDITION_UNKNOWN;
            set => __edition_removed = value;
        }
        public bool ShouldSerializeedition_removed() => __edition_removed != null;
        private Edition? __edition_removed;
    }
}

public partial class OneofOptions
{
    [ProtoMember(1)]
    public FeatureSet? features { get; set; }
}

public partial class EnumOptions
{
    [ProtoMember(6)]
    public bool deprecated_legacy_json_field_conflicts
    {
        get => __modern_deprecated_legacy_json_field_conflicts.GetValueOrDefault();
        set => __modern_deprecated_legacy_json_field_conflicts = value;
    }
    public bool ShouldSerializedeprecated_legacy_json_field_conflicts() => __modern_deprecated_legacy_json_field_conflicts != null;
    private bool? __modern_deprecated_legacy_json_field_conflicts;

    [ProtoMember(7)]
    public FeatureSet? features { get; set; }
}

public partial class EnumValueOptions
{
    [ProtoMember(2)]
    public FeatureSet? features { get; set; }

    [ProtoMember(3)]
    public bool debug_redact
    {
        get => __modern_debug_redact.GetValueOrDefault();
        set => __modern_debug_redact = value;
    }
    public bool ShouldSerializedebug_redact() => __modern_debug_redact != null;
    private bool? __modern_debug_redact;

    [ProtoMember(4)]
    public FieldOptions.FeatureSupport? feature_support { get; set; }
}

public partial class ServiceOptions
{
    [ProtoMember(34)]
    public FeatureSet? features { get; set; }
}

public partial class MethodOptions
{
    [ProtoMember(35)]
    public FeatureSet? features { get; set; }
}
