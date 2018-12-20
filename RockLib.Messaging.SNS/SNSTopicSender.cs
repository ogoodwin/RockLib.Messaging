﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace RockLib.Messaging.SNS
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to Amazon SNS.
    /// </summary>
    public class SNSTopicSender : ISender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SNSTopicSender"/> class.
        /// Uses a default implementation of the <see cref="IAmazonSimpleNotificationService"/> to
        /// communicate with SNS.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topicArn">The arn of the SNS topic.</param>
        /// <param name="region">The region of the SNS topic.</param>
        public SNSTopicSender(string name, string topicArn, string region = null)
            : this(region == null ? new AmazonSimpleNotificationServiceClient() : new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(region)), name, topicArn)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SNSTopicSender"/> class.
        /// </summary>
        /// <param name="client">An object that communicates with SNS.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topicArn">The arn of the SNS topic.</param>
        public SNSTopicSender(IAmazonSimpleNotificationService client, string name, string topicArn)
        {
            SnsClient = client ?? throw new ArgumentNullException(nameof(client));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TopicArn = topicArn ?? throw new ArgumentNullException(nameof(topicArn));
        }

        /// <summary>
        /// Gets the simple notification service client.
        /// </summary>
        public IAmazonSimpleNotificationService SnsClient { get; }

        /// <summary>
        /// Gets the name of this instance of <see cref="SNSTopicSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the topic's amazon resource name.
        /// </summary>
        public string TopicArn { get; }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "SNS";

            var publishMessage = new PublishRequest(TopicArn, message.StringPayload);

            foreach (var header in message.Headers)
            {
                publishMessage.MessageAttributes[header.Key] =
                    new MessageAttributeValue { StringValue = header.Value.ToString(), DataType = "String" };
            }

            return SnsClient.PublishAsync(publishMessage, cancellationToken);
        }

        /// <summary>
        /// Disposes the backing <see cref="IAmazonSimpleNotificationService"/> field.
        /// </summary>
        public void Dispose()
        {
            SnsClient.Dispose();
        }
    }
}
