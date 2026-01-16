using Confluent.Kafka;
using DomainUserService.Interfaces.IServices;
using KafkaUserService.Configuration;
using KafkaUserService.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace KafkaUserService.Services;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<KafkaEventPublisher> _logger;
    private readonly KafkaOptions _options;
    private readonly IProducer<string, string> _producer;
    private readonly JsonSerializerOptions _jsonOptions;

    public KafkaEventPublisher(
        ILogger<KafkaEventPublisher> logger,
        IOptions<KafkaOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        Acks acks = ParseAcks(_options.Acks);

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = acks,
            EnableIdempotence = _options.EnableIdempotence,
            MaxInFlight = _options.MaxInFlight,
            RetryBackoffMs = _options.RetryBackoffMs,
            MessageSendMaxRetries = _options.MessageSendMaxRetries,
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka producer error: {Error}", error);
            })
            .Build();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async Task PublishUserRegisteredEvent(string userId)
    {
        try
        {
            var @event = new UserRegisteredEvent
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
            };

            string message = JsonSerializer.Serialize(@event, _jsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = userId,
                Value = message,
            };

            DeliveryResult<string, string>? deliveryResult = await _producer.ProduceAsync(
                _options.UserRegisteredTopic,
                kafkaMessage);

            _logger.LogInformation(
                "Published UserRegisteredEvent for user {UserId} to topic {Topic}, partition {Partition}, offset {Offset}",
                userId,
                deliveryResult.Topic,
                deliveryResult.Partition,
                deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish UserRegisteredEvent for user {UserId}",
                userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing UserRegisteredEvent for user {UserId}",
                userId);
            throw;
        }
    }

    public async Task PublishUserDeletedEvent(string userId)
    {
        try
        {
            var @event = new UserDeletedEvent
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
            };

            string message = JsonSerializer.Serialize(@event, _jsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = userId,
                Value = message,
            };

            DeliveryResult<string, string>? deliveryResult = await _producer.ProduceAsync(
                _options.UserDeletedTopic,
                kafkaMessage);

            _logger.LogInformation(
                "Published UserDeletedEvent for user {UserId} to topic {Topic}, partition {Partition}, offset {Offset}",
                userId,
                deliveryResult.Topic,
                deliveryResult.Partition,
                deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish UserDeletedEvent for user {UserId}",
                userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing UserDeletedEvent for user {UserId}",
                userId);
            throw;
        }
    }

    public async Task PublishPointsSpentEvent(string userId, int amount, string? referenceId)
    {
        try
        {
            var @event = new PointsSpentEvent
            {
                UserId = userId,
                Amount = amount,
                ReferenceId = referenceId,
                Timestamp = DateTime.UtcNow,
            };

            string message = JsonSerializer.Serialize(@event, _jsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = userId,
                Value = message,
            };

            DeliveryResult<string, string>? deliveryResult = await _producer.ProduceAsync(
                _options.PointsSpentTopic,
                kafkaMessage);

            _logger.LogInformation(
                "Published PointsSpentEvent for user {UserId}, amount {Amount}, referenceId {ReferenceId} to topic {Topic}, partition {Partition}, offset {Offset}",
                userId,
                amount,
                referenceId,
                deliveryResult.Topic,
                deliveryResult.Partition,
                deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish PointsSpentEvent for user {UserId}, amount {Amount}",
                userId,
                amount);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing PointsSpentEvent for user {UserId}, amount {Amount}",
                userId,
                amount);
            throw;
        }
    }

    public async Task PublishPointsAddedEvent(string userId, int amount, string? referenceId)
    {
        try
        {
            var @event = new PointsAddedEvent
            {
                UserId = userId,
                Amount = amount,
                ReferenceId = referenceId,
                Timestamp = DateTime.UtcNow,
            };

            string message = JsonSerializer.Serialize(@event, _jsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = userId,
                Value = message,
            };

            DeliveryResult<string, string>? deliveryResult = await _producer.ProduceAsync(
                _options.PointsAddedTopic,
                kafkaMessage);

            _logger.LogInformation(
                "Published PointsAddedEvent for user {UserId}, amount {Amount}, referenceId {ReferenceId} to topic {Topic}, partition {Partition}, offset {Offset}",
                userId,
                amount,
                referenceId,
                deliveryResult.Topic,
                deliveryResult.Partition,
                deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish PointsAddedEvent for user {UserId}, amount {Amount}",
                userId,
                amount);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing PointsAddedEvent for user {UserId}, amount {Amount}",
                userId,
                amount);
            throw;
        }
    }

    public async Task PublishPointsCompensatedEvent(string userId, int amount, string originalTransactionId)
    {
        try
        {
            var @event = new PointsCompensatedEvent
            {
                UserId = userId,
                Amount = amount,
                OriginalTransactionId = originalTransactionId,
                Timestamp = DateTime.UtcNow,
            };

            string message = JsonSerializer.Serialize(@event, _jsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = userId,
                Value = message,
            };

            DeliveryResult<string, string>? deliveryResult = await _producer.ProduceAsync(
                _options.PointsCompensatedTopic,
                kafkaMessage);

            _logger.LogInformation(
                "Published PointsCompensatedEvent for user {UserId}, amount {Amount}, originalTransactionId {OriginalTransactionId} to topic {Topic}, partition {Partition}, offset {Offset}",
                userId,
                amount,
                originalTransactionId,
                deliveryResult.Topic,
                deliveryResult.Partition,
                deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish PointsCompensatedEvent for user {UserId}, amount {Amount}",
                userId,
                amount);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing PointsCompensatedEvent for user {UserId}, amount {Amount}",
                userId,
                amount);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }

    private static Acks ParseAcks(string acks)
    {
        return acks.ToLowerInvariant() switch
        {
            "0" or "none" => Acks.None,
            "1" or "leader" => Acks.Leader,
            "all" or "-1" => Acks.All,
            _ => Acks.All,
        };
    }
}