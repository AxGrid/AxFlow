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