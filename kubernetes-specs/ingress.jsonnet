local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'ingress',

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 3,
      template+: {
        spec+: {
          containers_+: {
            server: kube.Container($.name) {
              image: jnci.ingress,
              imagePullPolicy: 'Always',
              restartPolicy: 'OnFailure',
              ports: [{ name: 'http', containerPort: 3000 }],
              env_+: {
                // ASPNET
                ASPNETCORE_URLS: 'http://localhost:3000',
                ASPNETCORE_ENVIRONMENT: 'Production',

                // General
                RabbitMQConnectionOptions__Hostname: 'rabbitmq',
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
          host: 'ingress.jn.localhost',
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
