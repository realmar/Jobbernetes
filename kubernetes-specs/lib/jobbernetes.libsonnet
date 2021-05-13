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

local Registry = import '../lib/registry.libsonnet';

local image_name(name) = 'jn-' + name;

local image_fqn(name) = Registry.registryUrl + '/' + image_name(name);

local image_job_fqn(name) = image_fqn('job-' + name);

local Image(name) = Registry {
  relativeName: name,
  imageName: image_name(name),
  fqn: image_fqn(name),
};

local JobImage(name) = Image(name) {
  fqn: image_fqn('job-' + name),
};

local Component(_name) = {
  name: _name,
  serviceName: image_name(_name),
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

// exports
{
  Storage:: Storage,
  Image:: Image,
  JobImage:: JobImage,
  Component:: Component,
  PrometheusAnnotations:: PrometheusAnnotations,
  WaitForRabbitMQ:: WaitForRabbitMQ,
  WaitForMongoDB:: WaitForMongoDB,
  WaitForAll:: WaitForAll,
  ResourcesMemory:: ResourcesMemory,
}
