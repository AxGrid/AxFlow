using System.Text;

namespace AxFlow;



public class Flow<TC, TS, TA>
    where TC : IFlowContext<TS, TA>
    where TS : struct
    where TA : struct
{
    const int MaxRetry = 1024;
    internal readonly Dictionary<TS, Dictionary<TA, List<ActionHolder>>> Actions = new();
    internal readonly Dictionary<TS, Dictionary<TA, List<ExceptionHolder>>> Exceptions = new();
    private readonly TS _startState;
    public delegate void DFlowAction(TC ctx);
    public delegate void DFlowExceptionAction(TC ctx, Exception e = null);

    public void Execute(TC ctx, TA action) => Execute(ctx, action, 0);
    private void Execute(TC ctx, TA action, int count)
    {
        if (ctx.State == null) ctx.State = _startState;
        var stateActions = Actions[ctx.State.Value];
        if (!stateActions.ContainsKey(action)) return;
        foreach (var executions in stateActions[action])
        {
            try
            {
                executions.Method(ctx);   
            } catch (FlowTerminateExceptions) {
                return;
            } catch (FlowRetryException<TA> e) {
                if (count < MaxRetry)
                {
                    if (e.Action != null)
                        Execute(ctx, e.Action ?? action, count + 1);
                    else
                        Execute(ctx, action, count + 1);
                }
                else
                    throw new StackOverflowException($"Max retry reached ({MaxRetry})");
                return;
            } catch (Exception e) {
                ctx.Throwable = e;
                ExecuteException(ctx, action, e, count + 1);
                return;
            }
        }
    }
    
    private void ExecuteException(TC ctx, TA action, Exception e, int count)
    {
        if (ctx.State == null) ctx.State = _startState;
        var stateExceptions = Exceptions[ctx.State.Value];
        
        foreach (var executions in stateExceptions[action])
        {   
            try
            {
                executions.Action?.Invoke(ctx);
                executions.EAction?.Invoke(ctx, e);
            }catch (FlowTerminateExceptions)
            {
                return;
            }catch (FlowRetryException<TA> ex)
            {
                if (count < MaxRetry)
                {
                    if (ex.Action != null)
                        ExecuteException(ctx, (TA)ex.Action, e, count + 1);
                    else
                        ExecuteException(ctx, action, e, count + 1);
                }
                else
                    throw new StackOverflowException($"Max retry reached ({MaxRetry})");
                return;
            }
        }
    }
    
    
    public static FlowBuilder<TC, TS, TA> Create() => new FlowBuilder<TC, TS, TA>();
    
    internal Flow(TS startState)
    {
        this._startState = startState;
        foreach (var value in Enum.GetValues(typeof(TS)))
        {
            Actions.Add((TS) value, new());
            Exceptions.Add((TS) value, new());
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var tss in Actions)
        {
            sb.Append($"{tss.Key}\n");
            foreach (var taa in tss.Value)
            {
                sb.Append($" - {taa.Key} A:({taa.Value.Count}) ");
                foreach (var el in taa.Value)
                {
                    sb.Append($"{el.Id} ");
                }
                sb.Append($" EX:({Exceptions[tss.Key][taa.Key].Count}) ");
                foreach (var el in  Exceptions[tss.Key][taa.Key])
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
    internal class ExceptionHolder : IEquatable<ExceptionHolder> , IComparable<ExceptionHolder>
    {
        
        public int Id { get; }
        public Type Throwable { get; }
        public DFlowAction Action { get; }
        public DFlowExceptionAction EAction { get; }
        
        public bool Check(Type throwable)
        {
            return Throwable.IsAssignableFrom(throwable);
        }

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

        public bool Equals(ExceptionHolder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public int CompareTo(ExceptionHolder other)
        {
            return ReferenceEquals(null, other) ? 1 : Id.CompareTo(other.Id);
        }
    }
    
}