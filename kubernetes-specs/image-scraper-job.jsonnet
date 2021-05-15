local components = import 'lib/components.libsonnet';
local config = import 'lib/configuration.libsonnet';
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
                image: components.image_scraper_job.image.fqn,
                imagePullPolicy: 'Always',
                resources: jn.ResourcesDefaults() {
                  limits+: {
                    cpu: $.cpus,
                  },
                },
                env_+: config.Logging() +
                       config.RabbitMQConnection() +
                       config.RabbitMQConsumer('jobbernetes', 'jn-images-ingress', 'jn-images-ingress') +
                       config.RabbitMQProducer('jobbernetes', 'jn-images-egress', 'jn-images-egress') +
                       config.MetricPusher(prometheus_instance) +
                       {
                         // ExternalService
                         ExternalServiceOptions__Url: 'http://' + components.external_service.serviceName + ':3000/images',

                         // Processing
                         JobOptions__BatchSize: '300',
                         JobOptions__MaxConcurrentJobs: std.toString($.cpus),
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
  job01: ImageJob('jn-image-scraper-01-job', 'image_scraper_01_job'),
  job02: ImageJob('jn-image-scraper-02-job', 'image_scraper_02_job'),
}
