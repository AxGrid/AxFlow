using System.Text;

namespace AxFlow;

public class Flow<TC, TS, TA>
    where TC : IFlowContext<TS, TA>
    where TS : struct
    where TA : struct
{
   
    internal readonly Dictionary<TS, Dictionary<TA, List<ActionHolder>>> _actions = new();
    internal readonly Dictionary<TS, Dictionary<TA, List<ExceptionHolder>>> _exceptions = new();
    private readonly TS startState;
    public delegate void DFlowAction(TC ctx);
    public delegate void DFlowExceptionAction(TC ctx, Exception e = null);

    public void Execute(TC ctx, TA? action)
    {
        
    }
    
    public static FlowBuilder<TC, TS, TA> Create() => new FlowBuilder<TC, TS, TA>();
    
    internal Flow(TS startState)
    {
        this.startState = startState;
        foreach (var value in Enum.GetValues(typeof(TS)))
        {
            _actions.Add((TS) value, new());
            _exceptions.Add((TS) value, new());
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var tss in _actions)
        {
            sb.Append($"{tss.Key}\n");
            foreach (var taa in tss.Value)
            {
                sb.Append($" - {taa.Key} A:({taa.Value.Count}) ");
                foreach (var el in taa.Value)
                {
                    sb.Append($"{el.Id} ");
                }
                sb.Append($" EX:({_exceptions[tss.Key][taa.Key].Count}) ");
                foreach (var el in  _exceptions[tss.Key][taa.Key])
                {
                    sb.Append($"{el.Id} ");
                }
                
                sb.Append("\n");
            }
        }
        return sb.ToString();
    }


    internal class ActionHolder : IEquatable<ActionHolder> , IComparable<ActionHolder>
    {
        public int Id { get; }
        public DFlowAction Method { get; }
        public ActionHolder(int id, DFlowAction method)
        {
            Id = id;
            Method = method;
        }

        public bool Equals(ActionHolder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public int CompareTo(ActionHolder other)
        {
            return ReferenceEquals(null, other) ? 1 : Id.CompareTo(other.Id);
        }

        public override string ToString()
        {
            return $"[{Id}] {Method.Method.Name}()";
        }
    }
    
    internal class ExceptionHolder
    {
        
        public int Id { get; }
        public Type Throwable { get; }
        public DFlowAction Action { get; }
        public DFlowExceptionAction EAction { get; }

        public ExceptionHolder(int id, Type throwable, DFlowAction action)
        {
            Id = id;
            Action = action;
            Throwable = throwable;
            EAction = null;
        }
            
        public ExceptionHolder(int id, Type throwable, DFlowExceptionAction action)
        {
            Id = id;
            Action = null;
            Throwable = throwable;
            EAction = action;
        }
            
    }
    
}