# Pachyderm GH2 repaired copy

This is an isolated repair of the component library. The original project is at `C:\Users\Arthu\OneDrive\Desktop\DEV\PachydermAcoustic_Grasshopper2\Pachyderm_GH2`. No repair was applied there or to the shared acoustic core.

## Build and validation

- Compiled plugin: `bin/Release/net7.0/PachydermGH2.rhp`.
- Release build: successful, zero warnings and zero errors. See `build.log`.
- Regression suite: 16 passed, zero failed. See `tests/results.log` and `tests/Program.cs`.
- Original-file SHA-256 comparison: see `original-verification.txt`.
- Reviewable source changes: `component-repairs.patch`; file inventory: `changed-files.txt`.

The tests invoke selected component processing methods with a substitute data-access object and exercise the real payload classes and SDK tree types. They cover sample isolation, metadata copying, enumerators, access methods, channel timing, chunking, scalar spectrum output, both delay directions, iteration path separation, Definition, STI validation, source delay lookup, source summation and pressure-conversion topology. They do not exercise component registration, native acoustical solvers, document serialization or the Rhino UI.

## Repairs

- Corrected item/tree access mismatches and array-valued numeric outputs; protected output paths from merging across component iterations.
- Repaired signal and spectrum enumeration, buffer ownership, duplication, channel timing and shape checks.
- Corrected analysis calculations that used zero-filled data, channel-zero arrival times or accumulators shared across receivers.
- Reworked response construction to populate simulation inputs, handle missing optional results, pair sources with receivers, sum sources once and report the generated sample rate. Uncombined responses use `{source;receiver}` paths; combined responses use `{receiver}`. FDBEM branches use `{source;receiver}`, with frequency values inside each branch.
- Connected ambisonic format selection to the explicit core overload. Preserved the native degree-local FuMa convention and added validation for unsupported selections.
- Repaired chunking, FFT bin labels and spectrum lengths, channel exports, delay cropping, MLS tail insertion, pressure-to-energy buffer independence, WAV normalization and playback.
- Added declared input defaults and settings persistence. Exposed Combine controls where the old menus were commented out.
- Fixed scene object filters and optional geometry handling; Polygon Scene converts Surface and Extrusion values to Breps. Fixed material-array scaling, receiver-bank collection and source-delay transfer into simulations.
- Removed process-priority changes. Ray tracing now uses per-run cancellation and cleans up its Escape handler and convergence window. Components run serially; document/UI components use UI-thread processing.
- Corrected finite-volume recording collection, Rhino Results output layout, false-colour mesh construction, custom-mesh input forwarding and ray hit-position bookkeeping.
- Replaced absolute build references with references to the copied `lib` dependencies and corrected component resource names where a matching resource exists.

## Changes visible in GH2

- WAV Output has a **Write** input and Play Signal has a **Play** input, both initially false. A true value performs the action on each evaluation; reset it after use.
- Loudspeaker CLF selection is available in its input panel instead of opening a dialog during every evaluation.
- Rhino Results has a mode input and a fixed four-output layout.
- FFT returns a spectrum payload for each channel. Stationary Receiver returns a bank for each source. Several formerly inconsistent outputs now correctly expose trees.
- Ambisonics returns an individual harmonic degree: W for degree 0; three channels for degree 1; five for degree 2; seven for degree 3. It does not assemble a cumulative multidegree buffer. Higher-degree SID is rejected because the inspected core does not implement its ordering.
- Broadband response APIs require Frequency Scope `0–7`; restricted scopes now produce an explanatory error instead of being silently ignored. Use Octave Filter for an individual audio band. Energy Time Curve supports octave intervals directly.
- Signal Correlation uses ranks of waveform samples after delay alignment; Schroeder Correlation uses integrated squared pressure. Confirm those domains match the comparison you intend.

## Trying the copy

Keep this folder as a separate project. The repaired library keeps the original plugin and component identities so it can replace the old version for testing; **load only one version in a Rhino session**. Do not install both side by side under the same identities. Existing installed files have not been replaced by this task.

Use the compiled RHP with your GH2/Pachyderm loading setup and the matching Pachyderm for Rhino installation. The `lib` folder contains the dependencies copied from the original build for compilation; do not replace Rhino's own GH2/RhinoCommon/Eto assemblies with those copies. Native FFT/audio and solver dependencies still come from the Pachyderm installation. Compatibility with a different GH2 WIP must be checked in that host.

Start with a duplicate GH2 document and newly placed repaired components: changed input/output schemas may require replacing old component instances. Verify a simple room with one source/receiver, then two sources and receivers, Combine on/off, optional simulation inputs and branched data. Next check cancellation/repeated runs, CLF selection, colours/preview, saving/reopening settings, WAV output and playback.

## Remaining limits

This is a compiled and regression-tested repair candidate, not a claim that all 62 active components have passed end-to-end GH2 tests. The four previously commented-out components remain inactive. Custom simulation/audio payload persistence, SDK conversions, document loading, native solver behavior, preview and UI lifecycle still require Rhino/GH2 validation.

The separately discussed core ambisonic formulas, elevation-at-poles behavior and higher-degree indexing were not changed: they reside outside the GH2 library. The copied core dependency is unchanged. Correct channel selection does not establish normalization or physical correctness of that dependency. Atmospheric pressure scaling is also retained pending resolution of the core's inconsistent pressure-unit contract.

Cancellation is wired to the ray-tracing core API. Other legacy solvers expose blocking worker-state polling without the same cancellation API; prompt cancellation of those solvers is not implemented or certified. Serial component iterations do not prove safety when different components access the same mutable core simulation object concurrently. Filter preparation is serialized within the shared response helper, but whole-host concurrency needs integration testing.

## Rebuilding

From this folder: `dotnet build PachydermGH2.csproj -c Release`. The project targets .NET 7 and references `lib`; it has no project references back to the originals and does not automatically deploy or package itself. The included regression harness targets .NET 9 and references a DLL-named copy of the built RHP in `tests/refs` because the standalone .NET host cannot load an RHP as a normal dependency entry.
