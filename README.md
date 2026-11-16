# rx-aot-fork

**Not the official Rx.** This is a fork of [System.Reactive](https://github.com/dotnet/reactive) 7.0.0
with one internal change that makes first use of an operator chain cheap under NativeAOT.
The public API is the same as System.Reactive 7.0.0. The official README is in
[README.upstream.md](README.upstream.md).

Packages:

| Package | Replaces |
|---|---|
| `Bitzeal.Reactive.AotFork` | `System.Reactive` 7.0.0 |
| `Bitzeal.Reactive.AotFork.Testing` | `Microsoft.Reactive.Testing` 7.0.0 |

The assembly names, namespaces, strong-name key and `AssemblyVersion` (7.0.0.0) stay the same, so
code and other libraries built against System.Reactive 7.0.0 work without changes. Only the
`netstandard2.0` and `net8.0` targets are built.

Do not reference `System.Reactive` and `Bitzeal.Reactive.AotFork` in the same app. Both contain
`System.Reactive.dll`, and the SDK keeps only one of them (the one with the higher file version)
without an error. The fork's file version (`7.0.0.6000N`) is above the official one, but check that
no other package pulls in `System.Reactive` (`dotnet list package --include-transitive`).
`Bitzeal.Reactive.AotFork.Testing` depends on the fork, not on `System.Reactive`.

## What differs from System.Reactive

In System.Reactive, every `Observable` operator calls an `IQueryLanguage` interface, and the
implementation `QueryLanguage` has one `public virtual` generic method per operator. Under
NativeAOT a generic virtual method needs runtime work the first time it is called with a new set of
reference type arguments. That costs milliseconds and hundreds of KB per operator chain on first use,
and an app's start path can have many such chains.

The fork removes the interface from the call path:

- `Observable` and `ObservableEx` hold a `sealed QueryLanguage` / `QueryLanguageEx` directly.
- The operator methods in `QueryLanguage*.cs` (and the two `.tt` templates) are no longer `virtual`.

That is the whole change: 23 files, one word or one line each, made by [aot-fork/apply.sh](aot-fork/apply.sh).
No operator code changes. The `IQueryServices` hook goes away; in Rx 7.0.0 it only does something with a
debugger attached, and then hands back the same object.

The rest stays as in Rx. In particular operators still schedule through `CurrentThreadScheduler`
as Rx does, which leaves about 0.1 ms per new chain.

Measured on a Samsung Galaxy S23 (Android, NativeAOT, 7 cold starts), first use of a 5-operator
chain on reference types:

| | First use | Allocated |
|---|---|---|
| System.Reactive 7.0.0 | 2.6-4.5 ms | 600-800 KB |
| This fork | 0.04-0.13 ms | 8-16 KB |

A second use of the same chain costs 0.00-0.06 ms in both.

## How it is checked

- Rx's own test suite (`Tests.System.Reactive`, net10.0) passes on the fork: 3817 of 3817.
- The public API of the fork and of upstream 7.0.0 is compared with
  [aot-fork/api-diff](aot-fork/api-diff) (PublicApiGenerator), for the lib and the ref assemblies.
  The diff must be empty.
- [aot-fork/check.sh](aot-fork/check.sh) fails if any `public virtual` is left in `QueryLanguage*`
  (for example a new operator after an upstream upgrade).

Commands, from `Rx.NET/Source`:

```sh
../../aot-fork/check.sh
NBGV_GitEngine=Disabled dotnet test tests/Tests.System.Reactive -f net10.0 -c Release -p:EnableWindowsTargeting=true
dotnet pack src/System.Reactive -c Release -o <folder>
dotnet pack src/Microsoft.Reactive.Testing -c Release -o <folder>
```

## Versions

Set by hand in [Rx.NET/Source/AotFork.props](Rx.NET/Source/AotFork.props), not by Nerdbank.GitVersioning:

- Package version `7.0.0-aotfork.N`. N goes up on every fork release.
- `AssemblyVersion` `7.0.0.0`, as upstream.
- `FileVersion` `7.0.0.6000N`, above the official `7.0.0.53081`.
- `InformationalVersion` `7.0.0+aot-fork.N` (plus the commit), so logs can tell the fork apart.

## Taking an upstream release

Each Rx release gets a branch `aot/<version>`: the upstream tag plus the fork commits
(the script result, then packaging). `main` points to the newest one.

1. `git fetch upstream --tags`, then `git switch -c aot/<new> rxnet-v<new>`.
2. Run `aot-fork/apply.sh` (copy `aot-fork/` from `main` first) and commit the result.
   The script runs on the new source, so there are no merge conflicts in the operator files.
3. Cherry-pick the packaging commit. Set `AotForkUpstreamVersion`, reset `AotForkRelease` to 1.
4. Run the test suite, the API diff against the new upstream and `check.sh`.
5. Pack, publish, and move `main` to the new branch.

A fix only in the fork (same upstream version): commit on `main`, raise `AotForkRelease`, pack.

## License

MIT, as Rx. See [LICENSE](LICENSE) (Copyright (c) .NET Foundation and Contributors) and
[AUTHORS.txt](AUTHORS.txt). File headers are kept as they are. This fork is not made, endorsed or
supported by the .NET Foundation or the Rx maintainers; report fork issues here, not upstream.
