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

// exports
{
  Storage:: Storage,
  Image:: Image,
  JobImage:: JobImage,
}
