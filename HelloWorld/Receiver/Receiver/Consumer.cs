using System;
using System.Messaging;
using MessageUtilities;

namespace Receiver
{
    internal class Consumer
    {
        private readonly MessageQueue channel;

        public Consumer(string channelName)
        {
            //We need to identify how the message is formatted - xml is the default
            channel = new MessageQueue(channelName) {Formatter = new XmlMessageFormatter(new[] {typeof (string)})};
            //We want to trace message headers such as correlation id, so we need to tell MSMQ to retrieve those
            channel.MessageReadPropertyFilter.SetAll();
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