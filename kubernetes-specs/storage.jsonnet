local djs = import 'djs.libsonnet';

[
  djs.Storage('grafana'),
  djs.Storage('influxdb'),
  djs.Storage('loki'),
  djs.Storage('prometheus/alertmanager'),
  djs.Storage('prometheus/server'),
  djs.Storage('prometheus/pushgateway'),
]
