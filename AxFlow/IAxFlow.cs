using AxFlow.Internal;

namespace AxFlow;

public interface IAxFlow<in TC, TS, TA>
    where TC : IAxFlowContext<TS, TA>
    where TS : struct
    where TA : struct
{

    public void Execute(TC ctx, TA? action);
    
    
}

