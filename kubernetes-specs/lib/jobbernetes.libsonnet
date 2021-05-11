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

local local_registry(image) = 'docker-registry/' + image;

local jn_image(name) = 'jn-' + name;


// exports
{
  Storage:: Storage,
  local_registry:: local_registry,
  jn_image:: jn_image,
}
