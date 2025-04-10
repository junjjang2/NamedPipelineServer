using System;

namespace ArduinoSerialRead
{
    public class MessageProcessor
    {
        public bool ProcessMessage(string message, out string part1, out string part2)
        {
            string[] parts = message.Split(',');

            if (parts.Length >= 2)
            {
                part1 = parts[0];
                part2 = parts[1];
                return true;
            }
            else
            {
                part1 = string.Empty;
                part2 = string.Empty;
                return false;
            }
        }
    }
}
