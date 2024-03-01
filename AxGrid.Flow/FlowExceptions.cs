namespace AxFlow;

/// <summary>
/// Exception to terminate the flow execute process
/// </summary>
public class FlowTerminateExceptions : Exception { }


/// <summary>
/// Exception to terminate current flow process and retry new flow process
/// </summary>
public class FlowRetryException : Exception { }