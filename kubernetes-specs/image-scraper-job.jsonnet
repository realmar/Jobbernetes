local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'jn-image-scraper-job',

  cronjob: kube.CronJob(self.name) {
    spec+: {
      schedule: '*/1 * * * *',
      successfulJobsHistoryLimit: 2,
      failedJobsHistoryLimit: 8,
      concurrencyPolicy: 'Forbid',
      jobTemplate+: {
        spec+: {
          parallelism: 6,
          completions: 6,
          template+: {
            spec+: {
              restartPolicy: 'OnFailure',
              initContainers_+: jn.WaitForRabbitMQ(),
              containers_+: {
                job: kube.Container($.name) {
                  image: jnci.image_scraper_job.fqn,
                  imagePullPolicy: 'Always',
                  resources: jn.ResourcesMemory('128Mi', '256Mi'),
                  env_+: {
                    // ExternalService
                    ExternalServiceOptions__Url: 'http://jn-external-service:3000/images',

                    // Processing
                    JobOptions__BatchSize: '1000',
                    JobOptions__MaxConcurrentJobs: '3',
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
                    //MetricsOptions__InstanceName: kube.FieldRef('metadata.name'),
                    MetricsOptions__InstanceName: 'image_scraper_job',
                    MetricPusherOptions__Endpoint: 'http://prometheus-pushgateway:9091/metrics',
                    MetricPusherOptions__Job: 'jobbernetes_jobs',

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
