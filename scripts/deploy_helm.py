#!/usr/bin/env python3

import argparse
from lib import run_shell_print


repositories = [
    ("influxdata", "https://helm.influxdata.com/"),
    ("grafana", "https://grafana.github.io/helm-charts"),
    ("prometheus-community", "https://prometheus-community.github.io/helm-charts")
]

charts = [
    ("influxdb", "influxdata/influxdb", "helm-configs/influxdb.yaml"),
    ("grafana", "grafana/grafana", "helm-configs/grafana.yaml"),
    ("loki", "grafana/loki", "helm-configs/loki.yaml"),
    ("prometheus", "prometheus-community/prometheus", "helm-configs/prometheus.yaml")
]


def deploy(ignore_errors=False):
    repos = [f"helm repo add {name} {url}" for name, url in repositories]
    update = ["helm repo update"]
    install = [
        f"helm install {name} {app} -f {config}" for name, app, config in charts]

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
