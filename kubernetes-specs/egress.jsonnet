local jnci = import 'lib/container_images.libsonnet';
local jn = import 'lib/jobbernetes.libsonnet';
local kube = import 'vendor/kube.libsonnet';

{
  name:: 'egress',

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
              image: jnci.egress.fqn,
              imagePullPolicy: 'Always',
              restartPolicy: 'OnFailure',
              ports: [{ name: 'http', containerPort: 3000 }],
              env_+: {
                // General
                RabbitMQConnectionOptions__Hostname: 'rabbitmq',
                RabbitMQConnectionOptions__Username: 'admin',
                RabbitMQConnectionOptions__Password: 'admin',

                // Consumer
                RabbitMQConsumerOptions__Exchange: 'jobbernetes',
                RabbitMQConsumerOptions__Queue: 'jn-images-egress',
                RabbitMQConsumerOptions__RoutingKey: 'jn-images-egress',

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
}
