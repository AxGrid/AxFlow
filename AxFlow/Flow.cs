namespace AxFlow;

public class Flow<T, TS, TA> 
    where T : IFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{

    public delegate void DFlowAction(T c);
    public delegate void DFlowExceptionAction(T c, Exception e = null);

    private int _id = 0;
    private readonly NullableDict<TS?, NullableDict<TA?, List<ActionHolder>>> _actions =
        new NullableDict<TS?, NullableDict<TA?, List<ActionHolder>>>();

    private readonly NullableDict<TS?, List<ExceptionHolder>> _exceptions =
        new NullableDict<TS?, List<ExceptionHolder>>();
        
    private TS _startState;

    internal void Add(TS? state, TA? action, DFlowAction method)
    {
            
        if (state == null)
        {
            AddAll(action, method);
            return;
        }
        if (!_actions.ContainsKey(state)) _actions.Add(state, new NullableDict<TA?, List<ActionHolder>>());
        if (!_actions[state].ContainsKey(action)) _actions[state].Add(action, new List<ActionHolder>());
        _actions[state][action].Add(new ActionHolder(_id++, method));
        _actions[state][action].Sort((a,b) => b.Id - a.Id);
    }

    internal void AddAll(TA? action, DFlowAction method)
    {
        foreach (var state in _actions.Keys)
        {
            if (!_actions[state].ContainsKey(action)) _actions[state].Add(action, new List<ActionHolder>());
            _actions[state][action].Add(new ActionHolder(_id++, method));
            _actions[state][action].Sort((a,b) => b.Id - a.Id);
        }
    }

    internal void AddExcept(TS? state, Type throwable, DFlowAction method = null, DFlowExceptionAction eMethod = null)
    {
        if (state == null)
        {
            AddAllExcept(throwable, method, eMethod); 
            return;
        }
        if (method != null) _exceptions[state].Add(new ExceptionHolder(throwable, method));
        if (eMethod != null) _exceptions[state].Add(new ExceptionHolder(throwable, eMethod));
    }

    internal void AddAllExcept(Type throwable,  DFlowAction method = null, DFlowExceptionAction eMethod = null)
    {
        var h = method != null ? new ExceptionHolder(throwable, method) : null;
        var eh = eMethod != null ? new ExceptionHolder(throwable, eMethod) : null;
        foreach (var list in _exceptions.Values)
        {
            if (h!=null) list.Add(h);
            if (eh != null) list.Add(eh);
        }
    }

        
    public T Execute(T ctx, TA? action) 
    {
        if (ctx.State == null)
        {
            Console.WriteLine($"Create new flow context.State = {_startState}");
            ctx.State = _startState;
        }
        Console.WriteLine($"Enter State:'{ctx.State}' {ctx.GetHashCode()}");
        ctx.Action = action;
        var allActions = GetAllActions(ctx.State, action);
        foreach (var act in allActions)
        {
            try
            {
                act.Method.Invoke(ctx);
            }
            catch (FlowTerminateException)
            {
                break;
            }
            catch (Exception e)
            {
                Except(ctx, e);
            }
        }
        Console.WriteLine($"Return State:'{ctx.State}' {ctx.GetHashCode()} Count:{allActions.Count()}");
        return ctx;
    }

    private void Except(T ctx, Exception e)
    {
        foreach (var eh in _exceptions[ctx.State].Where(eh => eh.Throwable == null || eh.Throwable.IsInstanceOfType(e)))
        {
            try
            {
                eh.Action?.Invoke(ctx);
                eh.EAction?.Invoke(ctx, e);
            }
            catch (FlowTerminateException)
            {
                break;                        
            }
        }
    }

    private IEnumerable<ActionHolder> GetAllActions(TS? state, TA? action)
    {
        foreach (var act in _actions.Keys)
        {
            Console.WriteLine($"State:'{act}' {act.GetHashCode()} Count:{_actions[act].Count}");
        }
        
        if (action == null) return _actions[state][null];
        var empty = _actions.ContainsKey(null) ? _actions[state][null] : new List<ActionHolder>();
        var notEmpty = _actions[state].ContainsKey(action) ? _actions[state][action] : new List<ActionHolder>();
        return empty.Concat(notEmpty).OrderBy(item => item.Id);
    }
        
    public static FlowBuilder<T, TS, TA> Create(TS startState)
    {
        return new FlowBuilder<T, TS, TA>(new Flow<T, TS, TA>(startState));
    }

    private Flow(TS startState)
    {
        _startState = startState;
        foreach (var value in Enum.GetValues(typeof(TS)))
        {
            _actions.Add((TS) value, new NullableDict<TA?, List<ActionHolder>>());
            _exceptions.Add((TS) value, new List<ExceptionHolder>());
        }

    }

    public class ActionHolder
    {
        public int Id { get; }
        public DFlowAction Method { get; }
        public ActionHolder(int id, DFlowAction method)
        {
            Id = id;
            Method = method;
        }
    }
        
    public class ExceptionHolder
    {
        public Type Throwable { get; }
        public DFlowAction Action { get; }
        public DFlowExceptionAction EAction { get; }

        public ExceptionHolder(Type throwable, DFlowAction action)
        {
            Action = action;
            Throwable = throwable;
            EAction = null;
        }
            
        public ExceptionHolder(Type throwable, DFlowExceptionAction action)
        {
            Action = null;
            Throwable = throwable;
            EAction = action;
        }
            
    }
}