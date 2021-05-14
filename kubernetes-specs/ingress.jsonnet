local components = import 'lib/components.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: components.ingress.serviceName,

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 2,
      template+: {
        metadata+: jn.PrometheusAnnotations(),
        spec+: {
          restartPolicy: 'Always',
          initContainers_+: jn.WaitForRabbitMQ(),
          containers_+: {
            server: kube.Container($.name) {
              image: components.ingress.image.fqn,
              imagePullPolicy: 'Always',
              ports: [{ name: 'http', containerPort: 3000 }],
              resources: jn.ResourcesDefaults() {
                limits+: {
                  cpu: '500m',
                },
              },
              env_+: {
                // ASPNET
                ASPNETCORE_URLS: 'http://0.0.0.0:3000',
                ASPNETCORE_ENVIRONMENT: 'Production',

                // General
                RabbitMQConnectionOptions__Hostname: constants.RabbitMQDns,
                RabbitMQConnectionOptions__Username: 'admin',
                RabbitMQConnectionOptions__Password: 'admin',

                // Producer
                RabbitMQProducerOptions__Exchange: 'jobbernetes',
                RabbitMQProducerOptions__Queue: 'jn-images-ingress',
                RabbitMQProducerOptions__RoutingKey: 'jn-images-ingress',
              },
            },
          },
        },
      },
    },
  },

  service: kube.Service(self.name) {
    target_pod: $.deployment.spec.template,
  },

  ingress: kube.Ingress(self.name) {
    apiVersion: 'networking.k8s.io/v1',
    spec+: {
      rules+: [
        {
          host: components.ingress.ingress,
          http: {
            paths:
              [
                {
                  path: '/',
                  pathType: 'Prefix',
                  backend: {
                    service: {
                      name: $.service.name_port.serviceName,
                      port: { number: $.service.name_port.servicePort },
                    },
                  },
                },
              ],
          },
        },
      ],
    },
  },
}
