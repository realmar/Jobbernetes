#!/usr/bin/env python3

import __init__
import os
from lib import SPECS_DIR, run_shell_print


commands = [
    f"kubectl create namespace cattle-system --dry-run=client -o yaml | kubectl apply -f -",
    f"kubectl create namespace cert-manager --dry-run=client -o yaml | kubectl apply -f-",

    f"kubectl apply --validate=false -f {os.path.join(SPECS_DIR, 'vendor', 'cert-manager.crds.v1.0.4.yaml')}",

    f"helm repo add jetstack https://charts.jetstack.io",
    f"helm repo update",

    f"helm upgrade --install cert-manager jetstack/cert-manager --namespace cert-manager --version v1.0.4",
    f"helm upgrade --install rancher rancher-latest/rancher --namespace cattle-system --set hostname=rancher.localhost"
]


def deploy():
    for command in commands:
        run_shell_print(command)


if __name__ == "__main__":
    deploy()
