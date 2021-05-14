local components = import 'lib/components.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

local ImageJob(name, prometheus_instance) = kube.CronJob(name) {
  cpus:: 2,
  spec+: {
    schedule: '*/1 * * * *',
    successfulJobsHistoryLimit: 3,
    failedJobsHistoryLimit: 8,
    concurrencyPolicy: 'Forbid',
    jobTemplate+: {
      spec+: {
        parallelism: 2,
        completions: 2,
        template+: {
          spec+: {
            restartPolicy: 'OnFailure',
            initContainers_+: std.mergePatch(
              jn.WaitForRabbitMQ(),
              jn.WaitFor(constants.prometheusGatewayDns),
            ),
            containers_+: {
              job: kube.Container(name) {
                image: components.image_scraper_job.image.fqn,
                imagePullPolicy: 'Always',
                resources: jn.ResourcesDefaults() {
                  limits+: {
                    cpu: $.cpus,
                  },
                },
                env_+: {
                  // ExternalService
                  ExternalServiceOptions__Url: 'http://' + components.external_service.serviceName + ':3000/images',

                  // Processing
                  JobOptions__BatchSize: '300',
                  JobOptions__MaxConcurrentJobs: std.toString($.cpus),
                  JobOptions__MaxMessagesPerTask: '20',

                  // General
                  RabbitMQConnectionOptions__Hostname: constants.RabbitMQDns,
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
                  MetricPusherOptions__Endpoint: 'http://' + constants.prometheusGatewayDns + ':9091/metrics',
                  MetricPusherOptions__Job: 'jobbernetes_jobs',
                  MetricPusherOptions__JobName: prometheus_instance,

                  // Logging
                  Logging__LogLevel__Default: 'Information',

                  // Demo
                  DemoOptions__ProcessingDelayMilliseconds__Min: '100',
                  DemoOptions__ProcessingDelayMilliseconds__Max: '300',
                  DemoOptions__FailureProbability: '0.2',
                },
              },
            },
          },
        },
      },
    },
  },
};


{
  job01: ImageJob('jn-image-scraper-01-job', 'image_scraper_01_job'),
  job02: ImageJob('jn-image-scraper-02-job', 'image_scraper_02_job'),
}
