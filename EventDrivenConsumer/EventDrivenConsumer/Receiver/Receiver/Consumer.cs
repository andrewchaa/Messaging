using System;
using System.Timers;
using System.Messaging;
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
            channel.ReceiveCompleted += Consume;
        }

        public void Start()
        {
            isRunning = true;
            Receive();
            Console.WriteLine("Service started");
        }


        public void Pause()
        {
            isRunning = false;
            Console.WriteLine("Service paused");
        }

        public void Stop()
        {
            isRunning = false;
            channel.Close();
            Console.WriteLine("Service stopped");
        }

        private void Consume(object source, ReceiveCompletedEventArgs result)
        {
            try
            {
                var queue = (MessageQueue) source;
                var message = queue.EndReceive(result.AsyncResult);
                if (message != null)
                {
                    message.TraceMessage();
                }
            }
            catch (MessageQueueException mqe)
            {
                Console.WriteLine("{0} {1}", mqe.Message, mqe.MessageQueueErrorCode);
            }

            Receive();
        }

        private void Receive()
        {
            if (isRunning)
            {
                channel.BeginReceive(new TimeSpan(0, 0, 0, ConfigurationSettings.PollingTimeout));
            }
        }
    }
}