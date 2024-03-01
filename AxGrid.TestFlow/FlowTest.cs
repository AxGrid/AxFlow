using AxFlow;

namespace TestAxFlow;

public class FlowTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestCreateFlow()
    {
        var flow = Flow<TestContext, State, Action>.Create()
            .On(State.B, builder =>
            {
                builder.Where(Action.X, (ctx) => { });
                builder.Where(null, ctx => { });
                builder.CatchTo(Action.Z, null, State.B);
            })
            //.With((ctx) => { })
            .With(Action.X,(ctx) => { })
            .On(null, builder =>
            {
                builder.Where(Action.Z, (ctx) => { });
                builder.Where(null, ctx => { });
                builder.Where(Action.Y, (ctx) => { });
                builder.CatchTo(Action.Z, null, State.B);
            })
            .With((ctx) => { })
            .On(State.B, builder =>
            {
                builder.Where(Action.Z, (ctx) => { });
            })
            .CatchTo(State.B)
            .Build(State.A);
        
        Console.WriteLine(flow.ToString());
    }

    [Test]
    public void TestExecuteFlow()
    {
        var flow = Flow<TestContext, State, Action>.Create()
            .Catch((ctx, ex) =>
            {
                Console.WriteLine($"Found exception: {ctx.Throwable.Message}");
                ctx.Throwable = null;
            })
            .On(State.A, builder =>
            {
                builder.Where(Action.X, ctx =>
                {
                    throw new Exception("Oops!");
                });
                builder.To(Action.X, State.B);
            }).Build(State.A);
        var ctx = new TestContext();
        flow.Execute(ctx, Action.X);
        Assert.IsNull(ctx.Throwable);
        Assert.That(ctx.State, Is.EqualTo(State.A));
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