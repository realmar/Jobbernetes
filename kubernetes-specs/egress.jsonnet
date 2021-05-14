local components = import 'lib/components.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: components.egress.serviceName,
  prometheusPort:: 9098,

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 2,
      template+: {
        metadata+: jn.PrometheusAnnotations($.prometheusPort),
        spec+: {
          restartPolicy: 'Always',
          initContainers_+: jn.WaitForAll(),
          containers_+: {
            server: kube.Container($.name) {
              image: components.egress.image.fqn,
              imagePullPolicy: 'Always',
              resources: jn.ResourcesDefaults() {
                limits+: {
                  cpu: '500m',
                },
              },
              env_+: {
                // General
                RabbitMQConnectionOptions__Hostname: constants.RabbitMQDns,
                RabbitMQConnectionOptions__Username: 'admin',
                RabbitMQConnectionOptions__Password: 'admin',

                // Consumer
                RabbitMQConsumerOptions__Exchange: 'jobbernetes',
                RabbitMQConsumerOptions__Queue: 'jn-images-egress',
                RabbitMQConsumerOptions__RoutingKey: 'jn-images-egress',

                // MongoDB
                MongoOptions__ConnectionString: 'mongodb://' + constants.mongoDBDns + ':27017',
                MongoOptions__Database: 'jobbernetes',
                MongoOptions__Collection: 'images',

                // Prometheus
                MetricServerOptions__Hostname: '0.0.0.0',
                MetricServerOptions__Port: std.toString($.prometheusPort),
                MetricServerOptions__Path: '/metrics',
              },
            },
          },
        },
      },
    },
  },
}
