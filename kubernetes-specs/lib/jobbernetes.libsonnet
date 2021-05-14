local kube = (import '../vendor/kube.libsonnet');

local Storage(name) = {
  name:: std.strReplace(name, '/', '-'),
  pvName:: self.name + '-pv',
  pvcName:: self.name + '-pvc',

  pv: kube.PersistentVolume(self.pvName) {
    spec+: {
      storageClassName: 'jobbernetes-volume',
      capacity: {
        storage: '10Gi',
      },
      accessModes:
        [
          'ReadWriteOnce',
        ],
      hostPath:
        {
          path: '/var/data/' + name,
        },
    },
  },
  pvc: kube.PersistentVolumeClaim(self.pvcName) {
    storageClass: 'jobbernetes-volume',
    storage: '10Gi',
    spec+: {
      selector: {
        matchLabels: {
          name: $.pvName,
        },
      },
    },
  },
};

local PrometheusAnnotations(port=3000) = {
  annotations+: {
    'prometheus.io/scrape': 'true',
    'prometheus.io/path': '/metrics',
    'prometheus.io/port': std.toString(port),
  },
};

local WaitFor(name) = {
  ['wait' + name]: kube.Container('wait-for-' + name) {
    image: 'groundnuty/k8s-wait-for:v1.4',
    imagePullPolicy: 'Always',
    args: [
      'pod',
      '-lapp.kubernetes.io/name=' + name,
    ],
  },
};

local WaitForRabbitMQ() = WaitFor('rabbitmq');
local WaitForMongoDB() = WaitFor('mongodb');
local WaitForAll() = std.mergePatch(WaitForRabbitMQ(), WaitForMongoDB());

local ResourcesMemory(request, limit) = {
  requests: {
    memory: request,
  },
  limits: {
    memory: limit,
  },
};

local ResourcesCpu(request, limit) = {
  requests: {
    cpu: request,
  },
  limits: {
    cpu: limit,
  },
};

local ResourcesDefaults() = std.mergePatch(
  ResourcesMemory('128Mi', '256Mi'),
  ResourcesCpu('100m', '1000m')
);

// exports
{
  Storage:: Storage,
  PrometheusAnnotations:: PrometheusAnnotations,
  WaitFor:: WaitFor,
  WaitForRabbitMQ:: WaitForRabbitMQ,
  WaitForMongoDB:: WaitForMongoDB,
  WaitForAll:: WaitForAll,
  ResourcesMemory:: ResourcesMemory,
  ResourcesCpu:: ResourcesCpu,
  ResourcesDefaults:: ResourcesDefaults,
}
