local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'jn-dataviewer',

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 1,
      template+: {
        metadata+: jn.PrometheusAnnotations(),
        spec+: {
          restartPolicy: 'Always',
          initContainers_+: jn.WaitForMongoDB(),
          containers_+: {
            server: kube.Container($.name) {
              image: jnci.dataviewer.fqn,
              imagePullPolicy: 'Always',
              ports: [{ name: 'http', containerPort: 3000 }],
              resources: jn.ResourcesMemory('128Mi', '256Mi'),
              env_+: {
                // ASPNET
                ASPNETCORE_URLS: 'http://0.0.0.0:3000',
                ASPNETCORE_ENVIRONMENT: 'Production',

                // MongoDB
                MongoOptions__ConnectionString: 'mongodb://mongodb:27017',
                MongoOptions__Database: 'jobbernetes',
                MongoOptions__Collection: 'images',
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
          host: 'dataviewer.jn.localhost',
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
