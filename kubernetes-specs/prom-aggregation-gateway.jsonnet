local constants = import 'lib/constants.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: constants.prometheusGatewayDns,
  port:: 9091,

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 1,
      template+: {
        metadata+: jn.PrometheusAnnotations($.port) {
          labels+: {
            'app.kubernetes.io/name': $.name,
          },
        },
        spec+: {
          restartPolicy: 'Always',
          containers_+: {
            server: kube.Container($.name) {
              image: 'weaveworks/prom-aggregation-gateway',
              imagePullPolicy: 'Always',
              ports: [{ name: 'http', containerPort: $.port }],
              args_+: { listen: '0.0.0.0:' + $.port },
              resources: {
                limits: {
                  cpu: 2,
                  memory: '256Mi',
                },
                requests: {
                  cpu: 0.2,
                  memory: '128Mi',
                },
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
}
