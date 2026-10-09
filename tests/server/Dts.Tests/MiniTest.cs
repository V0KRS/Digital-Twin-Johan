namespace Dts.Tests;

/// <summary>
/// Interim dependency-free test harness. NuGet was unreachable in the authoring sandbox, so xUnit could not be
/// restored. Tests are plain methods tagged [Fact]-like via TestAttribute; migrate to xUnit when NuGet is available
/// (see docs/DECISIONS/0002-interim-test-harness.md).
/// </summary>
[AttributeUsage(AttributeTargets.Method)] public sealed class TestAttribute : Attribute { }

public static class Assert
{
    public static void Equal<T>(T expected, T actual)
    { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected '{expected}', got '{actual}'"); }
    public static void True(bool c, string msg = "Expected true") { if (!c) throw new Exception(msg); }
}

public static class Runner
{
    public static async Task<int> RunAllAsync()
    {
        int passed = 0, failed = 0;
        foreach (var t in typeof(Runner).Assembly.GetTypes())
        foreach (var m in t.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0))
        {
            var name = $"{t.Name}.{m.Name}";
            try
            {
                var inst = Activator.CreateInstance(t);
                var r = m.Invoke(inst, null);
                if (r is Task task) await task;
                Console.WriteLine($"PASS {name}"); passed++;
            }
            catch (Exception e)
            { Console.WriteLine($"FAIL {name}: {(e.InnerException ?? e).Message}"); failed++; }
        }
        Console.WriteLine($"Summary: {passed} passed, {failed} failed");
        return failed == 0 && passed > 0 ? 0 : 1;
    }
}
