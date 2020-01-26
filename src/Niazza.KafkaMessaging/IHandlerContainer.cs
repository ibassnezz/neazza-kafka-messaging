using System;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging
{
    public interface IHandlerContainer
    {
        Type HandlerType { get; }
        Type MessageType { get; }
        Func<IServiceProvider, object> HandlerFunc { get; }
        CustomizedHandlerSettings Settings { get; }

    }
}