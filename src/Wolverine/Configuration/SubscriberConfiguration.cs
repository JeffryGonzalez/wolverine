using System;
using System.Collections.Generic;
using Baseline;
using Newtonsoft.Json;
using Wolverine.Runtime.Serialization;

namespace Wolverine.Configuration;

public class SubscriberConfiguration<T, TEndpoint> : DelayedEndpointConfiguration<TEndpoint>, ISubscriberConfiguration<T>
    where TEndpoint : Endpoint where T : ISubscriberConfiguration<T>
{
    public SubscriberConfiguration(TEndpoint endpoint) : base(endpoint)
    {
    }

    public T UseDurableOutbox()
    {
        add(e => e.Mode = EndpointMode.Durable);
        return this.As<T>();
    }

    public T UsePersistentOutbox()
    {
        return UseDurableOutbox();
    }


    public T BufferedInMemory()
    {
        add(e => e.Mode = EndpointMode.BufferedInMemory);
        return this.As<T>();
    }

    public T SendInline()
    {
        add(e => e.Mode = EndpointMode.Inline);
        return this.As<T>();
    }

    public T Named(string name)
    {
        add(e => e.Name = name);
        return this.As<T>();
    }

    public ISubscriberConfiguration<T> CustomNewtonsoftJsonSerialization(JsonSerializerSettings customSettings)
    {
        add(e =>
        {
            var serializer = new NewtonsoftSerializer(customSettings);
            e.RegisterSerializer(serializer);
            e.DefaultSerializer = serializer;
        });

        return this;
    }

    public ISubscriberConfiguration<T> DefaultSerializer(IMessageSerializer serializer)
    {
        add(e =>
        {
            e.RegisterSerializer(serializer);
            e.DefaultSerializer = serializer;
        });

        return this.As<T>();
    }

    public T CustomizeOutgoing(Action<Envelope> customize)
    {
        add(e => e.OutgoingRules.Add(new LambdaEnvelopeRule(customize)));

        return this.As<T>();
    }

    public T CustomizeOutgoingMessagesOfType<TMessage>(Action<Envelope> customize)
    {
        add(e => e.OutgoingRules.Add(new LambdaEnvelopeRule<TMessage>(customize)));

        return this.As<T>();
    }

    public T CustomizeOutgoingMessagesOfType<TMessage>(Action<Envelope, TMessage> customize)
    {
        return CustomizeOutgoing(env =>
        {
            if (env.Message is TMessage message)
            {
                customize(env, message);
            }
        });
    }


    /// <summary>
    ///     Fine-tune the circuit breaker parameters for this outgoing subscriber endpoint
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    public T CircuitBreaking(Action<ICircuitParameters> configure)
    {
        add(configure);
        return this.As<T>();
    }
}

internal class SubscriberConfiguration : SubscriberConfiguration<ISubscriberConfiguration, Endpoint>,
    ISubscriberConfiguration
{
    public SubscriberConfiguration(Endpoint endpoint) : base(endpoint)
    {
    }
}