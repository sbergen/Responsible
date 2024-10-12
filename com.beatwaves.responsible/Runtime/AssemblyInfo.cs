using System.Runtime.CompilerServices;

// Testing the Unity Editor utilities is very tedious without
// implementing fake operation states.
// We don't want to test internals in the core tests,
// but here it's ok, as long as the use is very limited.
[assembly:InternalsVisibleTo("Responsible.EditorTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
