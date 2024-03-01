namespace AxFlow;

public class FlowBuilder<T, TS, TA> 
    where T : IFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    private readonly Flow<T, TS, TA> _flow;
    public Flow<T, TS, TA> Build() => _flow;

    public delegate bool DFlowCheck(T ctx);

    public delegate void DFlowOnAction(FlowOnBuilder<T, TS, TA> builder);

    public FlowBuilder<T, TS, TA> On(TS state, DFlowOnAction inState)
    {
        inState.Invoke(new FlowOnBuilder<T, TS, TA>(_flow, state, this));
        return this;
    }
        
    public FlowBuilder<T, TS, TA> When(TS state, TA action, Flow<T, TS, TA>.DFlowAction method)
    {
        _flow.Add(state, action, method);
        return this;
    }

    public FlowBuilder<T, TS, TA> When(TS state, Flow<T, TS, TA>.DFlowAction method)
    {
        _flow.Add(state, default, method);
        return this;
    }

    public FlowBuilder<T, TS, TA> When(TA? action, Flow<T, TS, TA>.DFlowAction method)
    {
        _flow.AddAll(action, method);
        return this;
    }

    public FlowBuilder<T, TS, TA> When(Flow<T, TS, TA>.DFlowAction method)
    {
        _flow.AddAll(default, method);
        return this;
    }

        
    public FlowBuilder<T, TS, TA> To(TS state, TA action, TS to, DFlowCheck check = null, bool terminate = false)
    {
        return When(state, action, (ctx) =>
        {
            if (check != null && !check(ctx)) return;
            ctx.State = to;
            if (terminate) throw new FlowTerminateException();
        });
    }

    public FlowBuilder<T, TS, TA> Catch(TS? state, Type ex, Flow<T, TS, TA>.DFlowAction method, bool terminate = false)
    {
        if (!terminate)
            _flow.AddExcept(state, ex, method);
        else
            _flow.AddExcept(state, ex, (ctx) =>
            {
                method.Invoke(ctx);
                throw new FlowTerminateException();
            });
        return this;
    }
        

    public FlowBuilder<T, TS, TA> Catch(TS? state, Type ex, Flow<T, TS, TA>.DFlowExceptionAction method, bool terminate = false)
    {
        if (!terminate)
            _flow.AddExcept(state, ex, eMethod: method);
        else
            _flow.AddExcept(state, ex, eMethod: (ctx, eArg) =>
            {
                method.Invoke(ctx, eArg);
                throw new FlowTerminateException();
            });
        return this;
    }


    public FlowBuilder<T, TS, TA> Catch(Type ex, Flow<T, TS, TA>.DFlowAction method, bool terminate = false)
    {
        return Catch(null, ex, method, terminate);
    }

    public FlowBuilder<T, TS, TA> Catch(Flow<T, TS, TA>.DFlowAction method, bool terminate = false)
    {
        return Catch(null, typeof(Exception), method, terminate);
    }
        
        
    public FlowBuilder<T, TS, TA> Catch(TS? state, Type ex, TS to, bool terminate = false)
    {
        _flow.AddExcept(state, ex, eMethod: (ctx, eArg) =>
        {
            ctx.State = to;
            if (terminate) throw new FlowTerminateException();
        });
        return this;
    }


    public FlowBuilder<T, TS, TA> Catch(Type ex, TS to, bool terminate = false)
    {
        return Catch(null, ex, to, terminate);
    }

    public FlowBuilder<T, TS, TA> Catch(TS to, bool terminate = false)
    {
        return Catch(null, typeof(Exception), to, terminate);
    }

    public FlowBuilder<T, TS, TA> Terminate(TS? state, TA action, DFlowCheck check)
    {
        _flow.Add(state, action, (ctx) =>
        {
            if (check.Invoke(ctx)) throw new FlowTerminateException();
        });
        return this;
    }
        
    public FlowBuilder<T, TS, TA> Terminate(TS? state, DFlowCheck check)
    {
        _flow.Add(state, null, (ctx) =>
        {
            if (check.Invoke(ctx)) throw new FlowTerminateException();
        });
        return this;
    }
        
    public FlowBuilder<T, TS, TA> Terminate(DFlowCheck check)
    {
        _flow.Add(null, null, (ctx) =>
        {
            if (check.Invoke(ctx)) throw new FlowTerminateException();
        });
        return this;
    }
        
    internal FlowBuilder(Flow<T, TS, TA> flow)
    {
        _flow = flow;
    }
        
}