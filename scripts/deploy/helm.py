#!/usr/bin/env python3

import __init__
import argparse
from lib import is_windows, run_shell_print


repositories = [
    ("influxdata", "https://helm.influxdata.com/"),
    ("grafana", "https://grafana.github.io/helm-charts"),
    ("prometheus-community", "https://prometheus-community.github.io/helm-charts"),
    ("bitnami", "https://charts.bitnami.com/bitnami"),
    ("groundhog2k", "https://groundhog2k.github.io/helm-charts/"),
]

charts = [
    ("influxdb", "influxdata/influxdb", "helm-configs/influxdb.yaml"),
    ("grafana", "grafana/grafana", "helm-configs/grafana.yaml"),
    ("promtail", "grafana/promtail", "helm-configs/promtail.yaml"),
    ("loki", "grafana/loki", "helm-configs/loki.yaml"),
    ("prometheus", "prometheus-community/prometheus", "helm-configs/prometheus.yaml"),
    ("mongodb", "bitnami/mongodb", "helm-configs/mongodb.yaml"),
    ("rabbitmq", "groundhog2k/rabbitmq", "helm-configs/rabbitmq.yaml"),
    ("prometheus-rabbitmq-exporter", "prometheus-community/prometheus-rabbitmq-exporter", "helm-configs/prometheus-rabbitmq-exporter.yaml"),
]


def deploy(ignore_errors=False):
    repos = [f"helm repo add {name} {url}" for name, url in repositories]
    update = ["helm repo update"]
    install = [
        f"helm install {name} {app} -f {config}" for name, app, config in charts]

    for command in repos + update + install:
        try:
            if is_windows():
                command = f"wsl -- {command}"
            run_shell_print(command)
        except:
            if not ignore_errors:
                raise


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--ignore-errors', '-e',
                        dest='ignore_errors', action='store_true')

    args = parser.parse_args()

    deploy(args.ignore_errors)
