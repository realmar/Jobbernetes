// Invoke as follows:
// bash: jsonnet --ext-str cwd=$(pwd) k3d.volumes.jsonnet
// fish: jsonnet --ext-str cwd=(pwd) k3d.volumes.jsonnet

local cwd = std.extVar('cwd');

local Mapping(relativePath) =
  {
    hostRelative: 'var_data/' + relativePath,
    host: cwd + self.hostRelative,
    nodeRelative: relativePath,
    node: '/var/data/' + relativePath,
  };

[
  Mapping('influxdb'),
  Mapping('grafana'),
  Mapping('loki'),
  Mapping('prometheus/server'),
  Mapping('prometheus/alertmanager'),
  Mapping('prometheus/pushgateway'),
  Mapping('kafka'),
  Mapping('docker-registry'),
]
