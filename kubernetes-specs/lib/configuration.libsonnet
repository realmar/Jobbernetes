local constants = import 'constants.libsonnet';

local AspNetCore() = {
  ASPNETCORE_URLS: 'http://0.0.0.0:3000',
  ASPNETCORE_ENVIRONMENT: 'Production',
};

local RabbitMQConnection() = {
  RabbitMQConnectionOptions__Hostname: constants.RabbitMQDns,
  RabbitMQConnectionOptions__Username: 'admin',
  RabbitMQConnectionOptions__Password: 'admin',
};

local RMQConsumer(exchange, queue, routingKey) = {
  RabbitMQConsumerOptions__Exchange: exchange,
  RabbitMQConsumerOptions__Queue: queue,
  RabbitMQConsumerOptions__BindingKey: routingKey,
  RabbitMQConsumerOptions__RoutingKey: routingKey,
};

local RMQProducer(exchange, queue, routingKey) = {
  RabbitMQProducerOptions__Exchange: exchange,
  RabbitMQProducerOptions__Queue: queue,
  RabbitMQProducerOptions__BindingKey: routingKey,
  RabbitMQProducerOptions__RoutingKey: routingKey,
};

local MongoDB(database, collection) = {
  MongoOptions__ConnectionString: 'mongodb://' + constants.mongoDBDns + ':27017',
  MongoOptions__Database: database,
  MongoOptions__Collection: collection,
};

local MetricServer(prometheusPort) = {
  MetricServerOptions__Hostname: '0.0.0.0',
  MetricServerOptions__Port: std.toString(prometheusPort),
  MetricServerOptions__Path: '/metrics',
};

local MetricPusher(jobName) = {
  MetricPusherOptions__Endpoint: 'http://' + constants.prometheusGatewayDns + ':9091/metrics',
  MetricPusherOptions__Job: 'jobbernetes_jobs',
  MetricPusherOptions__JobName: jobName,
};

local Logging() = {
  Serilog__MinimumLevel__Default: 'Information',
  Serilog__Loki__Hostname: 'http://' + constants.lokiDns + ':3100/loki/api/v1/push',
};

// Exports
{
  AspNetCore:: AspNetCore,
  RabbitMQConnection:: RabbitMQConnection,
  RabbitMQConsumer:: RMQConsumer,
  RabbitMQProducer:: RMQProducer,
  MongoDB:: MongoDB,
  MetricServer:: MetricServer,
  MetricPusher:: MetricPusher,
  Logging:: Logging,
}
