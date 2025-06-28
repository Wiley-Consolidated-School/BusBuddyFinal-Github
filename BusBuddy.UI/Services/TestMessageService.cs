using System;
using System.Collections.Generic;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Test implementation of IMessageService that captures messages for verification
    /// </summary>
    public class TestMessageService : IMessageService
    {
        private readonly List<MessageRecord> _messages = new List<MessageRecord>();

        public IReadOnlyList<MessageRecord> Messages => _messages.AsReadOnly();

        public void ShowError(string message, string title = "Error")
        {
            _messages.Add(new MessageRecord(MessageType.Error, message, title));
            Console.WriteLine($"🧪 TEST ERROR: {title} - {message}");
        }

        public void ShowInfo(string message, string title = "Information")
        {
            _messages.Add(new MessageRecord(MessageType.Info, message, title));
            Console.WriteLine($"🧪 TEST INFO: {title} - {message}");
        }

        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            _messages.Add(new MessageRecord(MessageType.Confirmation, message, title));
            Console.WriteLine($"🧪 TEST CONFIRMATION: {title} - {message} (returning true)");
            return true; // Always confirm in tests
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            _messages.Add(new MessageRecord(MessageType.Warning, message, title));
            Console.WriteLine($"🧪 TEST WARNING: {title} - {message}");
        }

        /// <summary>
        /// Clear all captured messages
        /// </summary>
        public void ClearMessages()
        {
            _messages.Clear();
        }

        /// <summary>
        /// Get messages of a specific type
        /// </summary>
        public IEnumerable<MessageRecord> GetMessages(MessageType type)
        {
            return _messages.FindAll(m => m.Type == type);
        }

        /// <summary>
        /// Check if any error messages were captured
        /// </summary>
        public bool HasErrors => _messages.Exists(m => m.Type == MessageType.Error);

        /// <summary>
        /// Get the last message of a specific type
        /// </summary>
        public MessageRecord? GetLastMessage(MessageType type)
        {
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Type == type)
                    return _messages[i];
            }
            return null;
        }
    }

    /// <summary>
    /// Record of a message shown through the message service
    /// </summary>
    public record MessageRecord(MessageType Type, string Message, string Title)
    {
        public DateTime Timestamp { get; } = DateTime.Now;
    }

    /// <summary>
    /// Types of messages
    /// </summary>
    public enum MessageType
    {
        Error,
        Info,
        Warning,
        Confirmation
    }
}

