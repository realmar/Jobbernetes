local kube = import 'vendor/kube.libsonnet';

{
  name:: 'kafka-ui',
  deployment: kube.Deployment(self.name + '-deployment') {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 1,
      template+: {
        spec+: {
          containers_+: {
            server: kube.Container($.name + '-container') {
              image: 'provectuslabs/kafka-ui',
              ports: [{ name: 'http', containerPort: 8080 }],
              env_+: {
                KAFKA_CLUSTERS_0_NAME: 'Jobbernetes',
                KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS: 'kafka:9092',
              },
            },
          },
        },
      },
    },
  },
  service: kube.Service(self.name + '-service') {
    target_pod: $.deployment.spec.template,
  },
  ingress: kube.Ingress(self.name + '-ingress') {
    apiVersion: 'networking.k8s.io/v1',
    spec+: {
      rules+: [
        {
          host: 'kafka-ui.localhost',
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
