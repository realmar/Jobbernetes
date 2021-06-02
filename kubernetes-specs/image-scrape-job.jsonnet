local components = import 'lib/components.libsonnet';
local config = import 'lib/configuration.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

local ImageJob(name, prometheusInstance, textPrefix, textPostfix) = kube.CronJob(name) {
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
              jobbernetes_job_name: prometheusInstance,
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
                       config.MetricPusher(prometheusInstance) +
                       {
                         // ExternalService
                         ExternalServiceOptions__Url: 'http://' + components.external_service.serviceName + ':3000/images',

                         // Processing
                         JobOptions__BatchSize: '300',
                         JobOptions__MaxDegreeOfParallelism: std.toString($.threads),

                         // Demo
                         DemoOptions__TextPrefix: textPrefix,
                         DemoOptions__TextPostfix: textPostfix,
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
  job01: ImageJob('jn-image-scrape-spaceship-job', 'image_scrape_spaceship_job', 'SPACESHIP - ', ' - SPACESHIP'),
  job02: ImageJob('jn-image-scrape-airplane-job', 'image_scrape_airplane_job', 'AIRPLANE - ', ' - AIRPLANE'),
  job03: ImageJob('jn-image-scrape-car-job', 'image_scrape_car_job', 'CAR - ', ' - CAR'),
}
