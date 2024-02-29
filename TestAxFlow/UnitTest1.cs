using AxFlow;

namespace TestAxFlow;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateAndExecuteAxFlow()
    {
        var flow = AxFlow<TestContext, State, Action>.Create();
        Assert.NotNull(flow);
        
    }
}

public enum State
{
    A,
    B,
    C
}

public enum Action
{
    X,
    Y,
    Z
}

public class TestContext : IAxFlowContext<State, Action>
{
    public State? State { get; set; }
    public Action? Action { get; set; }
    public Action? LastAction { get; }
    public Exception Throwable { get; set; }
    
    public TestContext()
    {
        this.State = TestAxFlow.State.A;
    }
}