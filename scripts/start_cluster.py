#!/usr/bin/env python3

import shutil
import os
from lib import run_shell, run_shell_print, get_volumes, K3D_CLUSTER_NAME, K3D_CONFIG_PATH


required_commands = ["k3d", "kubectl", "docker"]


def create_volumes(volumes):
    for volume in volumes:
        host_path = volume["host"]
        if not os.path.exists(host_path):
            print(f"Volume {host_path} does not exist. Creating ...")
            os.mkdir(host_path)
        else:
            print(f"Volume {host_path} already exist. Nothing to do.")


def create_k3d_cli_volumes(volumes):
    return ' '.join(f"--volume {v['host']}:{v['node']}" for v in volumes)


def does_cluster_exist():
    result = run_shell("k3d cluster list")
    return K3D_CLUSTER_NAME in result


if __name__ == "__main__":
    missing = [x for x in required_commands if not shutil.which(x)]
    if any(missing):
        print("Following tools are missing, install them accoring to the readme")
        for tool in missing:
            print(f"  {tool}")

    volumes = get_volumes()
    create_volumes(volumes)

    if does_cluster_exist():
        print(
            "Cluster found. "
            + f"Execute 'scripts/delete_cluster.py' do delete the cluster. "
            + "Starting ...")
        command = f"k3d cluster start {K3D_CLUSTER_NAME}"
    else:
        print(f"Cluster not found. Creating ...")
        command = f"k3d cluster create {K3D_CLUSTER_NAME} --config {K3D_CONFIG_PATH} {create_k3d_cli_volumes(volumes)}"

    run_shell_print(command)
