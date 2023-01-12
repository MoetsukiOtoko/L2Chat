using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2JChat.MVVM.Model
{
    internal class ChatMessage
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Color { get; set; }
        public DateTime Time { get; set; }

        public ChatMessage(string message, string type = "ServerMessage", string sender = "Server", string color = "White")
        {
            Type = type;
            Message = message;
            Sender = sender;
            Color = color;
            Time = DateTime.Now;
        }
    }
}
