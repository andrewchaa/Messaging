using System;
using NDesk.Options;

namespace Sender
{
    class Program
    {
        //sender -m="my name is" -c=polling_consumer
        static void Main(string[] args)
        {
            var channel = string.Empty;
            var message = string.Empty;
            var p = new OptionSet()
                        {
                            {"c|channel=", v => channel = v},
                            {"m|message=", m => message = m}
                        };
            p.Parse(args);
            if (string.IsNullOrEmpty(channel))
            {
                Console.WriteLine("You must provide a channel name");
                return;
            }

            string channelName = string.Format(@".\private$\{0}", channel);

            var producer = new Producer(channelName);
            producer.Send(message);
        }
    }
}
