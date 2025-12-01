# ProtoDescDump
Recover protocol definitions from protobuf descriptors

# Usage 
- Compile via `dotnet build` or Visual Studio
- Run `ProtoDescDumper.exe <your-arguments>`

## Arguments

| Option             | Short | Required | Default         | Description                                                                 |
|--------------------|-------|----------|-----------------|-----------------------------------------------------------------------------|
| `--input <path>`   | `-i`  | Yes      | —               | Input file or directory path.                                              |
| `--output <path>`  | `-o`  | No       | `DumpedProtos`  | Output directory for dumped `.proto` files.                                |
| `--recurse`        | `-r`  | No       | `false`         | If set with directory input, process all `.pb` files recursively.          |
| `--as-set`         | `-s`  | No       | `true`          | Treat input as a `FileDescriptorSet`. If `false`, treat as single `FileDescriptorProto`. |

#

Copyright© Hiro420