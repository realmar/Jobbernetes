local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'exterrnal-service',

  deployment: kube.Deployment(self.name) {
    metadata+: {
      labels: { app: $.name },
    },
    spec+: {
      replicas: 8,
      template+: {
        spec+: {
          containers_+: {
            server: kube.Container($.name) {
              image: jnci.external_service,
              imagePullPolicy: 'Always',
              restartPolicy: 'OnFailure',
              ports: [{ name: 'http', containerPort: 3000 }],
              env_+: {
                // ASPNET
                ASPNETCORE_URLS: 'http://localhost:3000',
                ASPNETCORE_ENVIRONMENT: 'Production',
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
