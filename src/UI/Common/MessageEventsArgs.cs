using System;
using Gtk;

namespace AutoGestPro.src.UI.Common
{
    /// <summary>
    /// Common event args for message passing between views and windows
    /// </summary>
    public class MessageEventArgs(string message, MessageType messageType) : EventArgs
    {
        public string Message { get; } = message;
        public MessageType MessageType { get; } = messageType;
    }
}