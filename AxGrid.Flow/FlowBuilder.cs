namespace AxFlow;

public enum Terminations
{
    None,
    Terminate,
    TerminateAndRetry
}

public class FlowBuilder<TC,TS,TA>
    where TC : IFlowContext<TS, TA>
    where TS : struct
    where TA : struct
{
    internal static int _id = 0;
    private readonly Dictionary<NullObject<TS?>, Dictionary<NullObject<TA?>, List<Flow<TC,TS,TA>.ActionHolder>>> _actions = new();
    private readonly Dictionary<NullObject<TS?>, Dictionary<NullObject<TA?>, List<Flow<TC,TS,TA>.ExceptionHolder>>> _exceptions = new();
    
    public delegate bool DFlowCheck(TC ctx);
    public delegate void DFlowOnAction(FlowStateBuilder builder);

    public FlowBuilder<TC, TS, TA> With(TA? action, Flow<TC, TS, TA>.DFlowAction method, DFlowCheck check = null, Terminations terminate = Terminations.None)
    {
        if (!_actions.ContainsKey(null))
            _actions.Add(null, new());
        if (!_actions[null].ContainsKey(action))
            _actions[null].Add(action, new());
        if (check != null || terminate != Terminations.None)
        {
            void RMethod(TC ctx)
            {
                if (check != null && !check(ctx)) return;
                method(ctx);
                switch (terminate)
                {
                    case Terminations.Terminate:
                        throw new FlowTerminateExceptions();
                    case Terminations.TerminateAndRetry:
                        throw new FlowRetryException<TA>();
                }
            }
            _actions[null][action].Add(new Flow<TC, TS, TA>.ActionHolder(_id++, RMethod));
        }
        else
            _actions[null][action].Add(new Flow<TC, TS, TA>.ActionHolder(_id++, method));

        return this;
    }
    
    public FlowBuilder<TC, TS, TA> With(Flow<TC, TS, TA>.DFlowAction method, DFlowCheck check = null, Terminations terminate = Terminations.None) => With(null, method, check, terminate);

    public FlowBuilder<TC, TS, TA> To(TA? action, TS to, DFlowCheck check = null,
        Terminations terminate = Terminations.None)
    {
        void RMethod(TC ctx)
        {
            ctx.State = to;
        }
        return With(action, RMethod, check, terminate);
    }

    public FlowBuilder<TC, TS, TA> To(TS to, DFlowCheck check = null,
        Terminations terminate = Terminations.None) => To(null, to, check, terminate);
    
    public FlowBuilder<TC, TS, TA> Catch(TA? action, Type ex, Flow<TC, TS, TA>.DFlowExceptionAction method,
        DFlowCheck check = null,  Terminations terminate = Terminations.None)
    {
        if (!_exceptions.ContainsKey(null))
            _exceptions.Add(null, new());
        if (!_exceptions[null].ContainsKey(action))
            _exceptions[null].Add(action, new());
        if (check != null || terminate != Terminations.None)
        {
            void RMethod(TC ctx, Exception e)
            {
                if (check != null && !check(ctx)) return;
                method(ctx, e);
                switch (terminate)
                {
                    case Terminations.Terminate:
                        throw new FlowTerminateExceptions();
                    case Terminations.TerminateAndRetry:
                        throw new FlowRetryException<TA>();
                }

            }
            _exceptions[null][action].Add(new Flow<TC, TS, TA>.ExceptionHolder(_id++, ex ?? typeof(Exception), RMethod));
        } else 
            _exceptions[null][action].Add(new Flow<TC, TS, TA>.ExceptionHolder(_id++, ex ?? typeof(Exception), method));
        return this;
    }

    public FlowBuilder<TC, TS, TA> Catch(Type ex, Flow<TC, TS, TA>.DFlowExceptionAction method,
        DFlowCheck check = null,  Terminations terminate = Terminations.None) => Catch(null, ex, method, check, terminate);
    
    public FlowBuilder<TC, TS, TA> Catch(Flow<TC, TS, TA>.DFlowExceptionAction method,
        DFlowCheck check = null,  Terminations terminate = Terminations.None) => Catch(null, method, check, terminate);


    public FlowBuilder<TC, TS, TA> CatchTo(TA? action, Type ex, TS to, DFlowCheck check = null,
        Terminations terminate = Terminations.None)
    {
        void RMethod(TC ctx, Exception e)
        {
            ctx.State = to;
        }
        return Catch(action, ex, RMethod, check, terminate);
    }

    public FlowBuilder<TC, TS, TA> CatchTo(Type ex, TS to, DFlowCheck check = null,
        Terminations terminate = Terminations.None) => CatchTo(null, ex, to, check, terminate);

    public FlowBuilder<TC, TS, TA> CatchTo(TS to, DFlowCheck check = null,
        Terminations terminate = Terminations.None) => CatchTo(null, to, check, terminate);
    
    public FlowBuilder<TC, TS, TA> On(TS? state, Action<FlowStateBuilder> builderAction)
    {
        var builder = new FlowStateBuilder();
        builderAction(builder);
        if (!_actions.ContainsKey(state))
            _actions.Add(state, new());
        var actionsDict = _actions[state];
        foreach (var kv in builder._actions)
        {
            if (!actionsDict.ContainsKey(kv.Key))
                actionsDict.Add(kv.Key, new());
            actionsDict[kv.Key].AddRange(kv.Value);
        }

        if (!_exceptions.ContainsKey(state))
            _exceptions.Add(state, new());
        var exceptionDict = _exceptions[state];
        foreach (var kv in builder._exceptions)
        {
            if (!exceptionDict.ContainsKey(kv.Key))
                exceptionDict.Add(kv.Key, new());
            exceptionDict[kv.Key].AddRange(kv.Value);
        }
        return this;
    }
    public Flow<TC, TS, TA> Build(TS startState)
    {
        Flow<TC, TS, TA> flow = new(startState);
        foreach (var kv in flow.Actions)
        {
            var stateDict = flow.Actions[kv.Key];
            var errDict = flow.Exceptions[kv.Key];
            foreach (var aKey in Enum.GetValues(typeof(TA)))
            {
                if (!stateDict.ContainsKey((TA) aKey))
                    stateDict.Add((TA) aKey, new());
                if (!errDict.ContainsKey((TA)aKey))
                    errDict.Add((TA) aKey, new());
            }
        }

        foreach (var skv in _actions)
        {
            if (skv.Key.Item != null)
            {
                var stateActions = flow.Actions[(TS) skv.Key];
                foreach (var akv in skv.Value)
                {
                    if (akv.Key.Item != null)
                        stateActions[(TA) akv.Key].AddRange(akv.Value);
                    else
                        foreach (var all in stateActions.Values)
                            all.AddRange(akv.Value);
                }
            }
            else
            {
                foreach (var stateActions in flow.Actions.Values)
                {
                    foreach (var akv in skv.Value)
                    {
                        if (akv.Key.Item != null)
                            stateActions[(TA) akv.Key.Item].AddRange(akv.Value);
                        else
                            foreach (var all in stateActions.Values)
                                all.AddRange(akv.Value);
                    }
                }
            }
        }
        foreach (var skv in _exceptions)
        {
            if (skv.Key.Item != null)
            {
                var stateActions = flow.Exceptions[(TS) skv.Key];
                foreach (var akv in skv.Value)
                {
                    if (akv.Key.Item != null)
                        stateActions[(TA) akv.Key].AddRange(akv.Value);
                    else
                        foreach (var all in stateActions.Values)
                            all.AddRange(akv.Value);
                }
            }
            else
            {
                foreach (var stateActions in flow.Exceptions.Values)
                {
                    foreach (var akv in skv.Value)
                    {
                        if (akv.Key.Item != null)
                            stateActions[(TA) akv.Key.Item].AddRange(akv.Value);
                        else
                            foreach (var all in stateActions.Values)
                                all.AddRange(akv.Value);
                    }
                }
            }
        }
        
        foreach (var actions in flow.Actions.SelectMany(akv => akv.Value))
            actions.Value.Sort();
        foreach (var actions in flow.Exceptions.SelectMany(akv => akv.Value))
            actions.Value.Sort();
        return flow;
    }

    internal FlowBuilder() { }
    
    
    public class FlowStateBuilder
    {
        internal readonly Dictionary<NullObject<TA?>, List<Flow<TC,TS,TA>.ActionHolder>> _actions = new();
        internal readonly Dictionary<NullObject<TA?>, List<Flow<TC,TS,TA>.ExceptionHolder>> _exceptions = new();

        public FlowStateBuilder Where(TA? action, Flow<TC, TS, TA>.DFlowAction method, DFlowCheck check = null,  Terminations terminate = Terminations.None)
        {
            if (!_actions.ContainsKey(action))
                _actions.Add(action, new ());
            if (check != null || terminate != Terminations.None)
            {
                void RMethod(TC ctx)
                {
                    if (check != null && !check(ctx)) return;
                    method(ctx);
                    switch (terminate)
                    {
                        case Terminations.Terminate:
                            throw new FlowTerminateExceptions();
                        case Terminations.TerminateAndRetry:
                            throw new FlowRetryException<TA>();
                    }
                }
                _actions[action].Add(new Flow<TC, TS, TA>.ActionHolder(_id++, RMethod));
            }else
                _actions[action].Add(new Flow<TC, TS, TA>.ActionHolder(_id++, method));
            return this;
        }

        public FlowStateBuilder Where(Flow<TC, TS, TA>.DFlowAction method, DFlowCheck check = null,
            Terminations terminate = Terminations.None) => Where(null, method, check, terminate);

        public FlowStateBuilder To(TA? action, TS to, DFlowCheck check = null,  Terminations terminate = Terminations.None)
        {
            void RMethod(TC ctx)
            {
                ctx.State = to;
            }
            return Where(action, RMethod, check, terminate);
        }
        
        public FlowStateBuilder Catch(TA? action, Type ex, Flow<TC, TS, TA>.DFlowExceptionAction method,
            DFlowCheck check = null,  Terminations terminate = Terminations.None)
        {
            if (!_exceptions.ContainsKey(action))
                _exceptions.Add(action, new());
            if (check != null || terminate != Terminations.None)
            {
                void RMethod(TC ctx, Exception e)
                {
                    if (check != null && !check(ctx)) return;
                    method(ctx, e);
                    switch (terminate)
                    {
                        case Terminations.Terminate:
                            throw new FlowTerminateExceptions();
                        case Terminations.TerminateAndRetry:
                            throw new FlowRetryException<TA>();
                    }
                }
                _exceptions[action].Add(new Flow<TC, TS, TA>.ExceptionHolder(_id++, ex ?? typeof(Exception), RMethod));
            } else 
                _exceptions[action].Add(new Flow<TC, TS, TA>.ExceptionHolder(_id++, ex ?? typeof(Exception), method));

            return this;
        }

        public FlowStateBuilder Catch(Type ex, Flow<TC, TS, TA>.DFlowExceptionAction method,
            DFlowCheck check = null,  Terminations terminate = Terminations.None) => Catch(null, ex, method, check, terminate);
        
        public FlowStateBuilder Catch(Flow<TC, TS, TA>.DFlowExceptionAction method,
            DFlowCheck check = null,  Terminations terminate = Terminations.None) => Catch(null, method, check, terminate);

        public FlowStateBuilder CatchTo(TA? action, Type ex, TS to, DFlowCheck check = null,  Terminations terminate = Terminations.None)
        {
            void RMethod(TC ctx, Exception e)
            {
                ctx.State = to;
            }
            return Catch(action, ex, RMethod, check, terminate);
        }

        public FlowStateBuilder CatchTo(Type ex, TS to, DFlowCheck check = null,  Terminations terminate = Terminations.None) => CatchTo(null, ex, to, check, terminate);
        public FlowStateBuilder CatchTo(TS to, DFlowCheck check = null,  Terminations terminate = Terminations.None) => CatchTo(null, to, check, terminate);
    }
    
    
    
}


