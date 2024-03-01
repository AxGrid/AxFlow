namespace AxFlow;

public class FlowOnBuilder<T, TS, TA> 
    where T : IFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    //private readonly Flow<T, TS, TA> _flow;
    private readonly TS _state;
    private readonly FlowBuilder<T, TS, TA> _builder;

    internal FlowOnBuilder(Flow<T, TS, TA> flow, TS state, FlowBuilder<T,TS,TA> builder)
    {
        //_flow = flow;
        _state = state;
        _builder = builder;
    }

    public FlowOnBuilder<T, TS, TA> To(TA action, TS to, FlowBuilder<T, TS, TA>.DFlowCheck check = null, bool terminate = false)
    {
        _builder.To(_state, action, to, check, terminate);
        return this;
    }

    public FlowOnBuilder<T, TS, TA> When(TA action, Flow<T, TS, TA>.DFlowAction method)
    {
        // if (action == null)
        // {
        //     _builder.When(_state, method);
        //     return this;
        // }
        //
        _builder.When(action, method);
        return this;
    }
        
    public FlowOnBuilder<T, TS, TA> When(Flow<T, TS, TA>.DFlowAction method)
    {
        _builder.When(method);
        return this;
    }
        
    public FlowOnBuilder<T, TS, TA> Catch(Type ex, Flow<T, TS, TA>.DFlowAction method, bool terminate = false)
    {
        _builder.Catch(_state, ex, method, terminate);
        return this;
    }
        
    public FlowOnBuilder<T, TS, TA> Catch(Flow<T, TS, TA>.DFlowAction method, bool terminate = false)
    {
        _builder.Catch(_state, typeof(Exception), method, terminate);
        return this;
    }


    public FlowOnBuilder<T, TS, TA> Catch(Type ex, Flow<T, TS, TA>.DFlowExceptionAction method,
        bool terminate = false)
    {
        _builder.Catch(_state, ex, method, terminate);
        return this;
    }
        
    public FlowOnBuilder<T, TS, TA> Catch(Flow<T, TS, TA>.DFlowExceptionAction method,
        bool terminate = false)
    {
        _builder.Catch(_state, typeof(Exception), method, terminate);
        return this;
    }
        
    public FlowOnBuilder<T, TS, TA> Catch(Type ex,TS to, bool terminate = false)
    {
        _builder.Catch(_state, ex, to, terminate);
        return this;
    }
        
    public FlowOnBuilder<T, TS, TA> Catch(TS to, bool terminate = false)
    {
        _builder.Catch(_state, typeof(Exception), to, terminate);
        return this;
    }
}