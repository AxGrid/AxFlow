namespace AxFlow.Internal;

public class AxFlowBuilder<TC, TS, TA>
    where TC : IAxFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    
    private readonly AxFlow<TC, TS, TA> flow;
    
    internal AxFlowBuilder(AxFlow<TC, TS, TA> flow)
    {
        this.flow = flow;
    }
}