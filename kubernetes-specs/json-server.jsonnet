local kube = import 'vendor/kube.libsonnet';
// local djs = import 'djs.libsonnet';

{
  name:: 'json-server',
  configMapVolume:: kube.ConfigMapVolume(self.config) {
    name: $.config.metadata.name,
    configMap+: {
      items: [
        {
          key: 'db.json',
          path: 'db.json',
        },
      ],
    },
  },

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
              image: 'vimagick/json-server',
              ports: [{ name: 'http', containerPort: 3000 }],
              args: ['-H', '0.0.0.0', '-p', '3000', '-w', 'db.json'],
              volumeMounts_+: {
                config: {
                  name: $.configMapVolume.configMap.name,
                  mountPath: '/data/db.json',
                  subPath: 'db.json',
                },
              },
            },
          },
          volumes_+: {
            config: $.configMapVolume,
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
          host: 'json-server.localhost',
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
  config: kube.ConfigMap(self.name + '-configmap') {
    data: {
      'db.json': std.toString(import 'json-server-db.jsonnet'),
    },
  },
}
