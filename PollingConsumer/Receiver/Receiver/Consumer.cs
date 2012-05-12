using System;
using System.Timers;
using System.Messaging;
using MessageUtilities;

namespace Receiver
{
    internal class Consumer : IDisposable
    {
        private readonly MessageQueue channel;
        private bool isRunning;
        private readonly Timer timer;

        public Consumer(string channelName)
        {
            //We need to identify how the message is formatted - xml is the default
            channel = new MessageQueue(channelName) {Formatter = new XmlMessageFormatter(new[] {typeof (string)})};
            //We want to trace message headers such as correlation id, so we need to tell MSMQ to retrieve those
            channel.MessageReadPropertyFilter.SetAll();

            //we use a timer to poll the queue at a regular interval, of course this may need to be re-entrant but we have no state to worry about
            timer = new Timer(ConfigurationSettings.PollingInterval) {AutoReset = true};
            timer.Elapsed += Consume;
        }

        public void Start()
        {
            timer.Start();
            Console.WriteLine("Service started, will read queue every {0} ms", ConfigurationSettings.PollingInterval);
        }

        public void Pause()
        {
            timer.Stop();
            Console.WriteLine("Service paused");
        }

        public void Stop()
        {
            timer.Stop();
            timer.Close();
            Console.WriteLine("Service stopped");
        }

        private void Consume(object state, ElapsedEventArgs args)
        {
            try
            {
                var message = channel.Receive(new TimeSpan(TimeSpan.TicksPerSecond*ConfigurationSettings.PollingTimeout));
                if (message != null)
                {
                    message.TraceMessage();
                }
            }
            catch (MessageQueueException mqe)
            {
                Console.WriteLine("{0} {1}", mqe.Message, mqe.MessageQueueErrorCode);
            }
        }

        public void Dispose()
        {
           timer.Close(); 
        }
    }
}