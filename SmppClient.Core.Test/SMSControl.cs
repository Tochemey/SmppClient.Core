using System;

namespace SmppClient.Core.Test
{
    internal class SMSControl
    {
        #region Properties

        /// <summary> A reference to the esme manager </summary>
        private static EsmeManager connectionManager;

        #endregion

        private static void Main(string[] args)
        {
            var server = "gateway.domain.com"; // IP Address or Name of the server
            short port = 9900; // Port
            var shortLongCode = "55555"; // The short or long code for this bind
            var systemId = "systemid"; // The system id for authentication
            var password = "password"; // The password of authentication
            var dataCoding = DataCodings.ASCII; // The encoding to use if Default is returned in any PDU or encoding request

            // Create a esme manager to communicate with an ESME
            connectionManager = new EsmeManager("Test",
                shortLongCode,
                ConnectionEventHandler,
                ReceivedMessageHandler,
                ReceivedGenericNackHandler,
                SubmitMessageHandler,
                QueryMessageHandler,
                LogEventHandler,
                null);

            // Bind one single Receiver connection
            connectionManager.AddConnections(1,
                ConnectionModes.Receiver,
                server,
                port,
                systemId,
                password,
                "Receiver",
                dataCoding);

            // Bind one Transmitter connection
            connectionManager.AddConnections(1,
                ConnectionModes.Transmitter,
                server,
                port,
                systemId,
                password,
                "Transceiver",
                dataCoding);

            // Accept command input
            var bQuit = false;

            for (;;)
            {
                // Hit Enter in the terminal once the binds are up to see this prompt

                Console.WriteLine("Commands");
                Console.WriteLine("send 12223334444 hello jack");
                Console.WriteLine("quit");
                Console.WriteLine("");

                Console.Write("\n#>");

                var command = Console.ReadLine();
                if (command.Length == 0) continue;

                switch (command.Split(' ')[0])
                {
                    case "quit":
                    case "exit":
                        bQuit = true;
                        break;

                    default:
                        ProcessCommand(command);
                        break;
                }

                if (bQuit) break;
            }

            if (connectionManager != null) connectionManager.Dispose();
        }

        private static void ProcessCommand(string command)
        {
            var parts = command.Split(' ');

            switch (parts[0])
            {
                case "send":
                    SendMessage(command);
                    break;

                case "query":
                    QueryMessage(command);
                    break;
            }
        }

        private static void SendMessage(string command)
        {
            var parts = command.Split(' ');
            var phoneNumber = parts[1];
            var message = string.Join(" ",
                parts,
                2,
                parts.Length - 2);

            // This is set in the Submit PDU to the SMSC
            // If you are responding to a received message, make this the same as the received message
            var submitDataCoding = DataCodings.Default;

            // Use this to encode the message
            // We need to know the actual encoding.
            var encodeDataCoding = DataCodings.ASCII;

            // There is a default encoding set for each connection. This is used if the encodeDataCoding is Default

            SubmitSm submitSm = null;
            SubmitSmResp submitSmResp = null;
            connectionManager.SendMessage(phoneNumber,
                null,
                Ton.National,
                Npi.ISDN,
                submitDataCoding,
                encodeDataCoding,
                message,
                out submitSm,
                out submitSmResp);
        }

        private static void QueryMessage(string command)
        {
            var parts = command.Split(' ');
            var messageId = parts[1];

            var querySm = connectionManager.SendQuery(messageId);
        }

        private static void ReceivedMessageHandler(string logKey,
            string serviceType,
            Ton sourceTon,
            Npi sourceNpi,
            string shortLongCode,
            DateTime dateReceived,
            string phoneNumber,
            DataCodings dataCoding,
            string message)
        {
            Console.WriteLine("ReceivedMessageHandler: {0}",
                message);
        }

        private static void ReceivedGenericNackHandler(string logKey,
            int sequence)
        { }

        private static void SubmitMessageHandler(string logKey,
            int sequence,
            string messageId)
        {
            Console.WriteLine("SubmitMessageHandler: {0}",
                messageId);
        }

        private static void QueryMessageHandler(string logKey,
            int sequence,
            string messageId,
            DateTime finalDate,
            int messageState,
            long errorCode)
        {
            Console.WriteLine("QueryMessageHandler: {0} {1} {2}",
                messageId,
                finalDate,
                messageState);
        }

        private static void LogEventHandler(LogEventNotificationTypes logEventNotificationType,
            string logKey,
            string shortLongCode,
            string message)
        {
            Console.WriteLine(message);
        }

        private static void ConnectionEventHandler(string logKey,
            ConnectionEventTypes connectionEventType,
            string message)
        {
            Console.WriteLine("ConnectionEventHandler: {0} {1}",
                connectionEventType,
                message);
        }
    }
}