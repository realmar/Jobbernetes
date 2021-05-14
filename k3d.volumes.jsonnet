// Invoke as follows:
// bash: jsonnet --ext-str cwd=$(pwd) k3d.volumes.jsonnet
// fish: jsonnet --ext-str cwd=(pwd) k3d.volumes.jsonnet

local cwd = std.extVar('cwd');

// keepOnClusterDestroy indicates to the delete_cluster_and_data.py that it
// should not delete this volume when deleting the cluster. This is helpful
// for data that should persists between clusters.
local Mapping(relativePath, keepOnClusterDestroy=false) =
  {
    hostRelative: 'var_data/' + std.strReplace(relativePath, '/', '-'),
    host: cwd + '/' + self.hostRelative,
    nodeRelative: relativePath,
    node: '/var/data/' + relativePath,
    keep: keepOnClusterDestroy,
  };

[
  Mapping('influxdb'),
  Mapping('grafana'),
  Mapping('loki'),
  Mapping('prometheus/server'),
  Mapping('rabbitmq'),
  Mapping('mongodb'),
]
