using AxFlow;

namespace TestAxFlow;

public class FlowTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateAndExecuteAxFlow()
    {
        var flow = Flow<TestContext, State, Action>.Create(State.A)
            .On(State.B, builder =>
            {
                builder.When(Action.X, context =>
                {
                    Console.WriteLine($"When State B but is {context.State}");
                    context.State = State.C;
                });
            })
            .Build();
        Assert.NotNull(flow);
        var ctx = new TestContext();
        Assert.IsNull(ctx.State);
        flow.Execute(ctx, Action.X);
        Assert.That(ctx.State, Is.EqualTo(State.A));
        // ctx.State = State.B;
        // ctx = flow.Execute(ctx, Action.X);
        // Assert.That(ctx.State, Is.EqualTo(State.C));
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

public class TestContext : FlowContext<State, Action>
{
}