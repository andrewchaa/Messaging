using System;
using System.Messaging;
using MessageUtilities;

namespace Receiver
{
    internal class Consumer
    {
        private MessageQueue channel;

        public Consumer(string channelName)
        {
            channel = new MessageQueue {Formatter = new XmlMessageFormatter(new[] {typeof (string)})};

        }

        public void Consume()
        {
            var message = channel.Receive();
            if (message != null)
            {
                message.TraceMessage();
            }
        }
    }
}