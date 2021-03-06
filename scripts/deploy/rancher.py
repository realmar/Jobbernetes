#!/usr/bin/env python3

import __init__
import os
from lib import HELM_DIR, SPECS_DIR, run_shell_print


commands = [
    f"kubectl create namespace cattle-system --dry-run=client -o yaml | kubectl apply -f -",
    f"kubectl create namespace cert-manager --dry-run=client -o yaml | kubectl apply -f-",

    f"kubectl apply --validate=false -f {os.path.join(SPECS_DIR, 'vendor', 'cert-manager.crds.v1.0.4.yaml')}",

    f"helm repo add rancher-latest https://releases.rancher.com/server-charts/latest",
    f"helm repo add jetstack https://charts.jetstack.io",
    f"helm repo update",

    f"helm upgrade --install cert-manager jetstack/cert-manager --namespace cert-manager --version v1.0.4",

    f"kubectl wait pod --for condition=ready -l app=cainjector --namespace cert-manager",
    f"kubectl wait pod --for condition=ready -l app=cert-manager --namespace cert-manager",
    f"kubectl wait pod --for condition=ready -l app=webhook --namespace cert-manager",

    f"helm upgrade --install rancher rancher-latest/rancher --namespace cattle-system -f {os.path.join(HELM_DIR, 'rancher.yaml')}"
]


def deploy():
    for command in commands:
        run_shell_print(command)


if __name__ == "__main__":
    deploy()
