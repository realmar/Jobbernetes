local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'jn-external-service',

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 3,
      template+: {
        metadata+: jn.PrometheusAnnotations(),
        spec+: {
          restartPolicy: 'Always',
          containers_+: {
            server: kube.Container($.name) {
              image: jnci.external_service.fqn,
              imagePullPolicy: 'Always',
              ports: [{ name: 'http', containerPort: 3000 }],
              env_+: {
                // ASPNET
                ASPNETCORE_URLS: 'http://0.0.0.0:3000',
                ASPNETCORE_ENVIRONMENT: 'Production',
              },
              resources: jn.ResourcesMemory('128Mi', '256Mi'),
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
          host: 'external-service.jn.localhost',
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
