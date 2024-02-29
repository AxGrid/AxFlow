namespace AxFlow.Internal;

public class AxFlow<TC, TS, TA> : IAxFlow<TC, TS, TA>
    where TC : IAxFlowContext<TS, TA> 
    where TS : struct
    where TA : struct
{
    
    public delegate void DFlowAction(TC c);
    public delegate void DFlowExceptionAction(TC c, Exception e = null);
    private int _id = 0;
    
    private readonly NullableDict<TS?, NullableDict<TA?, List<ActionHolder>>> actions = new();
    
    private readonly NullableDict<TS?, List<ExceptionHolder>> exceptions = new();
    
    
    public void Execute(TC ctx, TA? action)
    {
        throw new NotImplementedException();
    }

    public AxFlow()
    {
        foreach (var value in Enum.GetValues(typeof(TS)))
        {
            actions.Add((TS) value, new NullableDict<TA?, List<ActionHolder>>());
            exceptions.Add((TS) value, new List<ExceptionHolder>());
        }
    }
    
    internal void Add(TS? state, TA? action, DFlowAction method)
    {
            
        if (state == null)
        {
            AddAll(action, method);
            return;
        }
        if (!actions.ContainsKey(state)) actions.Add(state, new NullableDict<TA?, List<ActionHolder>>());
        if (!actions[state].ContainsKey(action)) actions[state].Add(action, new List<ActionHolder>());
        actions[state][action].Add(new ActionHolder(_id++, method));
        actions[state][action].Sort((a,b) => b.Id - a.Id);
    }

    internal void AddAll(TA? action, DFlowAction method)
    {
        foreach (var state in actions.Keys)
        {
            if (!actions[state].ContainsKey(action)) actions[state].Add(action, new List<ActionHolder>());
            actions[state][action].Add(new ActionHolder(_id++, method));
            actions[state][action].Sort((a,b) => b.Id - a.Id);
        }
    }

    internal void AddExcept(TS? state, Type throwable, DFlowAction method = null, DFlowExceptionAction eMethod = null)
    {
        if (state == null)
        {
            AddAllExcept(throwable, method, eMethod); 
            return;
        }
        if (method != null) exceptions[state].Add(new ExceptionHolder(throwable, method));
        if (eMethod != null) exceptions[state].Add(new ExceptionHolder(throwable, eMethod));
    }

    internal void AddAllExcept(Type throwable,  DFlowAction method = null, DFlowExceptionAction eMethod = null)
    {
        var h = method != null ? new ExceptionHolder(throwable, method) : null;
        var eh = eMethod != null ? new ExceptionHolder(throwable, eMethod) : null;
        foreach (var list in exceptions.Values)
        {
            if (h!=null) list.Add(h);
            if (eh != null) list.Add(eh);
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




