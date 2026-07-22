using System;
using System.Collections.Generic;
using System.Text;

namespace BookChat.AIChat.Implement
{
    public class CopilotProvider : IAIChatWebProvider
    {
        public string Name => "Copilot";

        public string Description => "Microsoft Copilot is an AI-powered conversational assistant designed to boost productivity.";

        public string Icon => "https://upload.wikimedia.org/wikipedia/en/a/aa/Microsoft_Copilot_Icon.svg";

        public string Url => "https://copilot.microsoft.com/";
    }
}
