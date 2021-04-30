local jn = import 'jobbernetes.libsonnet';

[
  jn.Storage('grafana'),
  jn.Storage('influxdb'),
  jn.Storage('loki'),

  jn.Storage('prometheus/alertmanager'),
  jn.Storage('prometheus/server'),
  jn.Storage('prometheus/pushgateway'),

  jn.Storage('kafka'),

  jn.Storage('docker-registry'),
]
