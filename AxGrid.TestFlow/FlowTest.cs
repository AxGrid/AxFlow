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
        var flow = Flow<TestContext, State, Action>.Create()
            .On(State.B, builder =>
            {
                builder.Where(Action.X, (ctx) => { });
                builder.Where(null, ctx => { });
            })
            //.With((ctx) => { })
            .With(Action.X,(ctx) => { })
            .On(null, builder =>
            {
                builder.Where(Action.Z, (ctx) => { });
                builder.Where(null, ctx => { });
                builder.Where(Action.Y, (ctx) => { });
            })
            .With((ctx) => { })
            .On(State.B, builder =>
            {
                builder.Where(Action.Z, (ctx) => { });
            })
            .Build(State.A);
        
        Console.WriteLine(flow.ToString());
        
        // Assert.NotNull(flow);
        // var ctx = new TestContext();
        // Assert.IsNull(ctx.State);
        // flow.Execute(ctx, Action.X);
        // Assert.That(ctx.State, Is.EqualTo(State.A));
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