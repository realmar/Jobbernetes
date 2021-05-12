local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'image-scraper-job',

  cronjob: kube.CronJob(self.name) {
    spec+: {
      schedule: '*/10 * * * *',
      concurrencyPolicy: 'Forbid',
      jobTemplate+: {
        spec+: {
          parallelism: 20,
          completions: 20,
          template+: {
            spec+: {
              containers_+: {
                job: kube.Container($.name) {
                  image: jnci.image_scraper_job.fqn,
                  imagePullPolicy: 'Always',
                  restartPolicy: 'OnFailure',
                  env_+: {
                    // ExternalService
                    ExternalServiceOptions__Url: 'http://external-service:3000/images',

                    // Processing
                    JobOptions__BatchSize: '80',
                    JobOptions__MaxConcurrentJobs: '20',
                    JobOptions__MaxMessagesPerTask: '20',

                    // General
                    RabbitMQConnectionOptions__Hostname: 'rabbitmq',
                    RabbitMQConnectionOptions__Username: 'admin',
                    RabbitMQConnectionOptions__Password: 'admin',

                    // Consumer
                    RabbitMQConsumerOptions__Exchange: 'jobbernetes',
                    RabbitMQConsumerOptions__Queue: 'jn-images-ingress',
                    RabbitMQConsumerOptions__RoutingKey: 'jn-images-ingress',

                    // Producer
                    RabbitMQProducerOptions__Exchange: 'jobbernetes',
                    RabbitMQProducerOptions__Queue: 'jn-images-egress',
                    RabbitMQProducerOptions__RoutingKey: 'jn-images-egress',

                    // Prometheus
                    MetricsOptions__InstanceName: 'localhost_scraper',
                    MetricPusherOptions__Endpoint: 'http://prometheus-pushgateway:9091/metrics',
                    MetricPusherOptions__Job: 'ScraperJobService',

                    // Logging
                    Logging__LogLevel__Default: 'Information',
                  },
                },
              },
            },
          },
        },
      },
    },
  },
}
