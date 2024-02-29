namespace AxFlow;

public interface IAxFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    TS? State { get; set; }
    TA? Action { get; set; }
    TA? LastAction { get; }
    Exception Throwable { get; set; }
}