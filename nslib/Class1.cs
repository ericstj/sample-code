using System;
using mtLib;

namespace nslib
{
    public static class Class1
    {
        public static AgentThread Get() => new CopilotStudioAgentThread().GetNewThread();
        public static AgentThread Get2() => new CopilotStudioAgentThread2().GetNewThread();
        public static AgentThread Get3() => new CopilotStudioAgentThread3().GetNewThread();

    }


    public sealed class CopilotStudioAgentThread2 : CopilotStudioAgentThread
    {
        public override AgentThread GetNewThread() => new CopilotStudioAgentThread2();
    }
    public sealed class CopilotStudioAgentThread3 : CopilotStudioAgentThread
    {
        public override AgentThread GetNewThread() => base.GetNewThread();
    }
}
