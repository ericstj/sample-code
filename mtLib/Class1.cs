using System;

namespace mtLib;

public abstract class AgentThread
{
    public abstract AgentThread GetNewThread();
    public abstract object Foo();
}

public class CopilotStudioAgentThread : AgentThread
{
#if NET
    public override CopilotStudioAgentThread GetNewThread() => new();
    public override string Foo() => "Hello"; 
#else
    public override AgentThread GetNewThread() => new CopilotStudioAgentThread();
    public override object Foo() => "Hello";
#endif
}