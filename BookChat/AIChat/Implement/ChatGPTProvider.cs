using System;
using System.Collections.Generic;
using System.Text;

namespace BookChat.AIChat.Implement
{
    public class ChatGPTProvider : IAIChatWebProvider
    {
        public string Name => "ChatGPT";
        public string Description => "ChatGPT is an AI language model developed by OpenAI that can generate human-like text based on the input it receives. It can be used for a variety of applications, including chatbots, content generation, and more.";
        public string Icon => "https://upload.wikimedia.org/wikipedia/commons/e/ef/ChatGPT-Logo.svg";
        public string Url => "https://chatgpt.com/";
    }
}
