using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBusTestClient
{
    public class NServiceBusHelper<Message, Reply> : IHandleMessages<Reply> where Message : IMessage where Reply : IMessage
    {
        public static IMessageHandlerContext Context { get; set; }

        private static ManualResetEvent _resetEvent;
        private static Reply _message;

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(IEndpointInstance endPoint, Message message, SendOptions options, TimeSpan timeout)
        {
            _resetEvent = new ManualResetEvent(false);

            await endPoint.Send(message, options).ConfigureAwait(false);

            if (_resetEvent.WaitOne(timeout))
            {
                return _message;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(string destination, IEndpointInstance endPoint, Message message, TimeSpan timeout)
        {
            _resetEvent = new ManualResetEvent(false);

            await endPoint.Send(destination, message).ConfigureAwait(false);

            if (_resetEvent.WaitOne(timeout))
            {
                return _message;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(IMessageHandlerContext endPoint, Message message, SendOptions options, TimeSpan timeout)
        {
            _resetEvent = new ManualResetEvent(false);

            await endPoint.Send(message, options).ConfigureAwait(false);

            if (_resetEvent.WaitOne(timeout))
            {
                return _message;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(string destination, IMessageHandlerContext endPoint, Message message, TimeSpan timeout)
        {
            _resetEvent = new ManualResetEvent(false);

            await endPoint.Send(destination, message).ConfigureAwait(false);

            if (_resetEvent.WaitOne(timeout))
            {
                return _message;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Generic message handler. Make sure to configure the endpoint to ensure that this handler
        ///     is called by using the ExecuteTheseHandlersFirst property! NServiceBus can't handle 
        ///     generic messages so it has to be told how to route the message. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Handle(Reply message, IMessageHandlerContext context)
        {
            _message = message;
            Context = context;

            if (_resetEvent != null)
            {
                _resetEvent.Set();
            }

            return Task.CompletedTask;
        }
    }
}
