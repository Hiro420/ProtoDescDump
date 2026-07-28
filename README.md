# ProtoDescDump
Recover protocol definitions from protobuf descriptors

## Arguments

| Option             | Short | Required | Default         | Description                                                                 |
|--------------------|-------|----------|-----------------|-----------------------------------------------------------------------------|
| `--input <path>`   | `-i`  | Yes      |                 | Input file or directory path.                                              |
| `--output <path>`  | `-o`  | No       | `DumpedProtos`  | Output directory for dumped `.proto` files.                                |
| `--recurse`        | `-r`  | No       | `false`         | If set with directory input, process all `.pb` files recursively.          |
| `--as-set`         | `-s`  | No       | `true`          | Treat input as a `FileDescriptorSet`. If `false`, treat as single `FileDescriptorProto`. |
| `--assume-dependencies-exist`         | `-no-dep`  | No       | `false`         | Continue when imported descriptor files are missing. |

# Examples:
```shell
# Restore every schema from a FileDescriptorSet
protodescdump -i descriptors.pb -o restored

# Restore one FileDescriptorProto
protodescdump -i one-file.pb -o restored --as-set false

# Process every .pb file under a directory
protodescdump -i ./descriptors -o restored --recurse
```

## License
GPL-3.0-only. See [LICENSE](LICENSE).\
Copyright © Hiro420
