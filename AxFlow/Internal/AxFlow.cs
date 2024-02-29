namespace AxFlow.Internal;

public class AxFlow<TC, TS, TA> : IAxFlow<TC, TS, TA>
    where TC : IAxFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    public void Execute(TC ctx, TA? action)
    {
        throw new NotImplementedException();
    }
}