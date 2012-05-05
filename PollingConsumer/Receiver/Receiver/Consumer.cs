using System;
using System.Messaging;
using System.Threading;
using MessageUtilities;

namespace Receiver
{
    internal class Consumer
    {
        private readonly MessageQueue channel;
        private bool isRunning;

        public Consumer(string channelName)
        {
            //We need to identify how the message is formatted - xml is the default
            channel = new MessageQueue(channelName) {Formatter = new XmlMessageFormatter(new[] {typeof (string)})};
            //We want to trace message headers such as correlation id, so we need to tell MSMQ to retrieve those
            channel.MessageReadPropertyFilter.SetAll();
        }

        public void Start()
        {
            isRunning = true;
            while (isRunning)
            {
                try
                {
                    var message = channel.Receive(new TimeSpan(TimeSpan.TicksPerSecond * ConfigurationSettings.PollingTimeout));
                    if (message != null)
                    {
                        message.TraceMessage();
                    }
                }
                catch (MessageQueueException mqe)
                {
                    Console.WriteLine("{0} {1}", mqe.Message, mqe.MessageQueueErrorCode);
                }
                Thread.Sleep(ConfigurationSettings.PollingInterval);
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}