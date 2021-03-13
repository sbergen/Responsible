---
uid: Responsible
summary: *content
---
Namespace for the main functionality of Responsible.
Other namespaces should not be needed, unless you are extending the base functionality.

To get started with Responsible, construct an instance of
[TestInstructionExecutor](xref:Responsible.TestInstructionExecutor),
build some test operations using
[Responsibly](xref:Responsible.Responsibly),
and execute them in a test method:
```cs
[SetUp]
public void SetUp()
{
    this.Executor = new TestInstructionExecutor();
}

[UnityTest]
public IEnumerator SomeTest()
{
    var instruction = ...;
    yield return instruction.ToYieldInstruction(this.Executor);
}
```
