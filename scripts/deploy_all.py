#!/usr/bin/env python3

import deploy_kubernetes
import deploy_helm


def deploy():
    deploy_kubernetes.deploy()
    deploy_helm.deploy()


if __name__ == "__main__":
    deploy()
