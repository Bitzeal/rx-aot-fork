# Bitzeal.Reactive.AotFork

**Not the official Rx.** A fork of [System.Reactive](https://www.nuget.org/packages/System.Reactive) 7.0.0
(MIT, .NET Foundation and Contributors) with one internal change: `Observable` calls a sealed
`QueryLanguage` directly instead of through the `IQueryLanguage` interface, and the operator methods are
no longer virtual. Under NativeAOT this removes the first-use cost of each operator chain
(on a Galaxy S23: 2.6-4.5 ms and 600-800 KB with System.Reactive, 0.04-0.13 ms and 8-16 KB with the fork).

The public API, assembly name and namespaces are the same as System.Reactive 7.0.0. Do not reference
`System.Reactive` in the same app. `Bitzeal.Reactive.AotFork.Testing` replaces `Microsoft.Reactive.Testing`.

Source, versions and how upstream releases are taken: https://github.com/Bitzeal/rx-aot-fork
