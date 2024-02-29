using AxFlow.Internal;

namespace AxFlow;

public static class AxFlow<TC, TS, TA> 
    where TC : IAxFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    public static AxFlowBuilder<TC, TS, TA> Create()
    {
        return new AxFlowBuilder<TC, TS, TA>(new Internal.AxFlow<TC, TS, TA>());
    }
}