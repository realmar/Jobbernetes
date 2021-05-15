local components = import 'lib/components.libsonnet';
local config = import 'lib/configuration.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

local ImageJob(name, prometheus_instance) = kube.CronJob(name) {
  processes:: 2,
  threads:: 2,

  spec+: {
    schedule: '*/1 * * * *',
    successfulJobsHistoryLimit: 3,
    failedJobsHistoryLimit: 8,
    concurrencyPolicy: 'Forbid',
    jobTemplate+: {
      spec+: {
        parallelism: $.processes,
        completions: $.processes,
        template+: {
          metadata+: jn.LoggingAnnotations() {
            labels+: {
              jobbernetes_job_name: prometheus_instance,
            },
          },
          spec+: {
            restartPolicy: 'OnFailure',
            initContainers_+: std.mergePatch(
              jn.WaitForRabbitMQ(),
              jn.WaitFor(constants.prometheusGatewayDns),
            ),
            containers_+: {
              job: kube.Container(name) {
                image: components.image_scrape_job.image.fqn,
                imagePullPolicy: 'Always',
                resources: jn.ResourcesDefaults() {
                  limits+: {
                    cpu: $.threads,
                  },
                },
                env_+: config.Logging() +
                       config.RabbitMQConnection() +
                       config.RabbitMQConsumer('jobbernetes', 'jn-images-input', 'jn-images-input') +
                       config.RabbitMQProducer('jobbernetes', 'jn-images-output', 'jn-images-output') +
                       config.MetricPusher(prometheus_instance) +
                       {
                         // ExternalService
                         ExternalServiceOptions__Url: 'http://' + components.external_service.serviceName + ':3000/images',

                         // Processing
                         JobOptions__BatchSize: '300',
                         JobOptions__MaxConcurrentJobs: std.toString($.threads),
                         JobOptions__MaxMessagesPerTask: '20',

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
  job01: ImageJob('jn-image-scrape-spaceship-job', 'image_scrape_spaceship_job'),
  job02: ImageJob('jn-image-scrape-airplane-job', 'image_scrape_airplane_job'),
  job03: ImageJob('jn-image-scrape-automobile-job', 'image_scrape_automobile_job'),
}
