using System;
using System.Collections.Generic;
using System.Messaging;
using MessageUtilities;

namespace Receiver
{
    [Flags]
    internal enum Queues
    {
        None = 0,
        Input = 1,
        Control = 2
    }

    internal class MessageBroker 
    {
        private readonly MessageQueue inputChannel;
        private readonly MessageQueue controlChannel;
        private bool isRunning;
        private readonly IDictionary<string, IList<MessageQueue>> routingTable = new Dictionary<string, IList<MessageQueue>>();  

        public MessageBroker(string inputChannelName, string controlChannelName)
        {
            inputChannel = EnsureQueueExists(inputChannelName);
            controlChannel = EnsureQueueExists(controlChannelName);

            inputChannel.MessageReadPropertyFilter.SetAll();
            controlChannel.MessageReadPropertyFilter.SetAll();

            inputChannel.ReceiveCompleted += Route;
            controlChannel.ReceiveCompleted += Subscribe;
        }


        public void Start()
        {
            isRunning = true;
            Receive(Queues.Input | Queues.Control);
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
            inputChannel.Close();
            controlChannel.Close();
            Console.WriteLine("Service stopped");
        }

        public MessageQueue EnsureQueueExists(string channelName)
        {
            var channel = !MessageQueue.Exists(channelName) ? MessageQueue.Create(channelName) : new MessageQueue(channelName);
            channel.Formatter = new XmlMessageFormatter(new[] {typeof (string)});
            return channel;
        }

        private void Route(object source, ReceiveCompletedEventArgs result)
        {
            try
            {
                var queue = (MessageQueue) source;
                var message = queue.EndReceive(result.AsyncResult);

                TraceMessage(message);

                var topic = Convert.ToBase64String(message.Extension);
                Console.WriteLine("Message Topic is {0", topic);

                var targetQueues = routingTable[topic];
                targetQueues.Each(targetQueue => targetQueue.Send(message));

            }
            catch (MessageQueueException mqe)
            {
                Console.WriteLine("{0} {1}", mqe.Message, mqe.MessageQueueErrorCode);
            }

            Receive(Queues.Input);
        }

        private void Receive(Queues queuesToListenOn)
        {
            if (isRunning)
            {
                if (queuesToListenOn.HasFlag(Queues.Input))
                {
                    inputChannel.BeginReceive(new TimeSpan(0, 0, 0, ConfigurationSettings.PollingTimeout));
                }

                if (queuesToListenOn.HasFlag(Queues.Control))
                {
                     controlChannel.BeginReceive(new TimeSpan(0, 0, 0, ConfigurationSettings.PollingTimeout));
                }
            }
        }

        private void Subscribe(object source, ReceiveCompletedEventArgs result)
        {
            try
            {
                var queue = (MessageQueue)source;
                var message = queue.EndReceive(result.AsyncResult);

                TraceMessage(message);

                SubscribeToTopic(queue, message);
            }
            catch (MessageQueueException mqe)
            {
                Console.WriteLine("{0} {1}", mqe.Message, mqe.MessageQueueErrorCode);
            }

            Receive(Queues.Control);
        }

        private void SubscribeToTopic(MessageQueue queue, Message message)
        {
            var routingInformation = ((String) message.Body);
            var split = (routingInformation.Split(new char[] {':'}, 2));
            
            if (split.Length != 2) return;
            
            var topic = split[0];
            var queueName = split[1];
            EnsureQueueExists(queueName);
            if (!routingTable.ContainsKey(topic))
            {
                routingTable.Add(topic, new List<MessageQueue>());
            }
            routingTable[topic].Add(queue);
            Console.WriteLine("Subscribed queue " + queueName + " for topic " + topic);
        }

        private static void TraceMessage(Message message)
        {
            if (message != null)
            {
                message.TraceMessage();
            }
        }
    }
}