#!/usr/bin/env python3

import shutil
import os
from lib import is_windows, run_shell, run_shell_print, get_volumes, get_registry, K3D_CLUSTER_NAME, K3D_CONFIG_PATH
import start_registry


required_commands = ["k3d", "kubectl", "docker"]
linux_required_commands = ["kubecfg", "jsonnet"]


def create_volumes(volumes):
    fmt = "Volume " + (("{:<" + str(max([len(x["hostRelative"]) for x in volumes]) + 1) + "}") * 2)

    for volume in volumes:
        host_path = volume["host"]
        host_path_relative = volume["hostRelative"]

        if not os.path.exists(host_path):
            print(fmt.format(host_path_relative, "does not exist. Creating ..."))
            os.makedirs(host_path)
        else:
            print(fmt.format(host_path_relative, "already exist. Nothing to do."))


def create_k3d_cli_volumes(volumes):
    return ' '.join(f"--volume {v['host']}:{v['node']}" for v in volumes)


def does_cluster_exist():
    result, _ = run_shell("k3d cluster list", silent=True)
    return K3D_CLUSTER_NAME in result


def run():
    # TODO: also check linux commands
    commands = required_commands
    if not is_windows():
        commands = commands + linux_required_commands

    missing = [x for x in commands if not shutil.which(x)]

    if any(missing):
        print("Following tools are missing, install them accoring to the readme")
        for tool in missing:
            print(f"  {tool}")

    volumes = get_volumes()
    create_volumes(volumes)
    print()

    if does_cluster_exist():
        print("Cluster found. Starting ...")
        print("Execute 'scripts/delete_cluster.py' do delete the cluster.")
        print()

        command = f"k3d cluster start {K3D_CLUSTER_NAME}"
    else:
        print(f"Cluster not found. Creating ...")

        registry = get_registry()
        print(registry)
        command = f"k3d cluster create {K3D_CLUSTER_NAME} " \
            + f"--k3s-server-arg \"--no-deploy=traefik\" " \
            + f"--config {K3D_CONFIG_PATH} " \
            + f"{create_k3d_cli_volumes(volumes)} " \
            + f"--registry-use {registry['registryUrl']}"

    start_registry.start()
    run_shell_print(command)


if __name__ == "__main__":
    run()
