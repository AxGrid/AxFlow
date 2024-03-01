namespace AxFlow;


public interface IFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    TS? State { get; set; }
    TA? Action { get; set; }
    TA? LastAction { get; }
    Exception Throwable { get; set; }
}

public class FlowContext<TS, TA> : IFlowContext<TS, TA>
    where TS : struct
    where TA : struct
{
    public TS? State { get; set; }

    private TA? _action;

    public TA? Action
    {
        get => _action;
        set
        {
            LastAction = _action;
            _action = value;
        }
    }

    public TA? LastAction { get; private set;  }
    public Exception Throwable { get; set; }
}
