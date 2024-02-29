namespace AxFlow.Internal;

public class AxFlowBuilder<TC, TS, TA>
    where TC : IAxFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    
    private readonly AxFlow<TC, TS, TA> _flow;
    
    public AxFlowBuilder<TC, TS, TA> On(TA action, IAxFlow<TC, TS, TA>.DFlowAction actionHandler)
    {
        _flow.AddAction(action, actionHandler);
        return this;
    }


    public IAxFlow<TC, TS, TA> Build()
    {
        return _flow;
    }
    
    internal AxFlowBuilder(AxFlow<TC, TS, TA> flow)
    {
        this._flow = flow;
    }
}