local components = import 'lib/components.libsonnet';
local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: components.dataviewer.serviceName,

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
              image: components.dataviewer.image.fqn,
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

                // MongoDB
                MongoOptions__ConnectionString: 'mongodb://' + constants.mongoDBDns + ':27017',
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
          host: components.dataviewer.ingress,
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
