#!/usr/bin/env python3

import argparse
from lib import run_shell_print


def deploy(ignore_errors):
    commands = [
        "helm repo add influxdata https://helm.influxdata.com/",
        "helm repo add grafana https://grafana.github.io/helm-charts",

        "helm repo update",

        "helm install influxdb influxdata/influxdb -f helm-configs/monitor/influxdb.yaml",
        "helm install grafana grafana/grafana -f helm-configs/monitor/grafana.yaml"
    ]

    for command in commands:
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