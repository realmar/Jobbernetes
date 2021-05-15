local components = import 'lib/components.libsonnet';
local config = import 'lib/configuration.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: components.output_service.serviceName,
  prometheusPort:: 9098,

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 2,
      template+: {
        metadata+: jn.AllAnnotations($.prometheusPort),
        spec+: {
          restartPolicy: 'Always',
          initContainers_+: jn.WaitForAll(),
          containers_+: {
            server: kube.Container($.name) {
              image: components.output_service.image.fqn,
              imagePullPolicy: 'Always',
              resources: jn.ResourcesDefaults() {
                limits+: {
                  cpu: '500m',
                },
              },
              env_+: config.Logging() +
                     config.RabbitMQConnection() +
                     config.RabbitMQConsumer('jobbernetes', 'jn-images-output', 'jn-images-output') +
                     config.MongoDB('jobbernetes', 'images') +
                     config.MetricServer(std.toString($.prometheusPort)),
            },
          },
        },
      },
    },
  },
}
