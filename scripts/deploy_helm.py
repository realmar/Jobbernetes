#!/usr/bin/env python3

import argparse
from lib import run_shell_print


repos = [
    ("influxdata", "https://helm.influxdata.com/"),
    ("grafana", "https://grafana.github.io/helm-charts"),
    ("prometheus-community", "https://prometheus-community.github.io/helm-charts")
]

charts = [
    ("influxdb", "influxdata/influxdb", "helm-configs/monitor/influxdb.yaml"),
    ("grafana", "grafana/grafana", "helm-configs/monitor/grafana.yaml"),
    ("loki", "grafana/loki", "helm-configs/monitor/loki.yaml"),
    ("prometheus", "prometheus-community/prometheus", "helm-configs/monitor/prometheus.yaml")
]


def deploy(ignore_errors=False):
    repos = [f"helm repo add {name} {url}" for name, url in repos]
    update = ["helm repo update"]
    install = [
        f"helm install {name} {app} {config}" for name, app, config in charts]

    for command in repos + update + install:
        try:
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
