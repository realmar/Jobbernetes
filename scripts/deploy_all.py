#!/usr/bin/env python3

from lib import run_shell_print
import deploy_kubernetes
import deploy_helm


if __name__ == "__main__":
    deploy_kubernetes.deploy()
    deploy_helm.deploy()
