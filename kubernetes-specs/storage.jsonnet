local k3dVolumes = import '../k3d.volumes.jsonnet';
local jn = import 'lib/jobbernetes.libsonnet';

[
  jn.Storage(volume.nodeRelative)
  for volume in k3dVolumes
]
